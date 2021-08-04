/*
 * Copyright (c) 2021 Proton Technologies AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ProtonVPN.Common;
using ProtonVPN.Common.Go;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Config;
using ProtonVPN.Vpn.LocalAgent;
using ProtonVPN.Vpn.LocalAgent.Contracts;
using ProtonVPN.Vpn.SplitTunnel;
using PInvoke = ProtonVPN.Vpn.LocalAgent.PInvoke;

namespace ProtonVPN.Vpn.Connection
{
    internal class LocalAgentWrapper : ISingleVpnConnection
    {
        private const string LocalAgentHost = "10.2.0.1:65432";

        private readonly ILogger _logger;
        private readonly EventReceiver _eventReceiver;
        private readonly SplitTunnelRouting _splitTunnelRouting;
        private readonly ISingleVpnConnection _origin;

        private IVpnEndpoint _endpoint;
        private VpnCredentials _credentials;
        private VpnConfig _vpnConfig;
        private bool _isTlsChannelActive;
        private bool _connectRequested;
        private LocalAgentState _localAgentState;
        private VpnStatus _vpnStatus = VpnStatus.Disconnected;
        private EventArgs<VpnState> _connectedState;
        private string _clientCertPem = string.Empty;

        public LocalAgentWrapper(
            ILogger logger,
            EventReceiver eventReceiver,
            SplitTunnelRouting splitTunnelRouting,
            ISingleVpnConnection origin)
        {
            _logger = logger;
            _eventReceiver = eventReceiver;
            _splitTunnelRouting = splitTunnelRouting;
            _origin = origin;
            origin.StateChanged += OnVpnStateChanged;
            eventReceiver.StateChanged += OnLocalAgentStateChanged;
            eventReceiver.ErrorOccurred += OnLocalAgentErrorOccurred;
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged;

        public InOutBytes Total => _origin.Total;

        public void Connect(IVpnEndpoint endpoint, VpnCredentials credentials, VpnConfig config)
        {
            _logger.Info("[LocalAgentWrapper] Connect action started");
            _connectRequested = true;
            _endpoint = endpoint;
            _credentials = credentials;
            _vpnConfig = config;
            _clientCertPem = credentials.ClientCertPem;
            _origin.Connect(endpoint, credentials, config);
        }

        public void Disconnect(VpnError error)
        {
            _logger.Info("[LocalAgentWrapper] Disconnect action started");
            InvokeStateChange(new EventArgs<VpnState>(
                new VpnState(VpnStatus.Disconnecting, error, string.Empty, string.Empty, _vpnConfig?.VpnProtocol ?? VpnProtocol.Smart, _vpnConfig?.OpenVpnAdapter)));
            _eventReceiver.Stop();
            CloseTlsChannel();
            _origin.Disconnect(error);
            _vpnStatus = VpnStatus.Disconnected;
        }

        public void SetFeatures(VpnFeatures vpnFeatures)
        {
            using GoString goFeatures = GetFeatures(vpnFeatures).ToGoString();
            PInvoke.SetFeatures(goFeatures);
        }

        public void UpdateAuthCertificate(string certificate)
        {
            _clientCertPem = certificate;
            _eventReceiver.Stop();
            CloseTlsChannel();
            ConnectToTlsChannel();
        }

        private void OnLocalAgentStateChanged(object sender, EventArgs<LocalAgentState> e)
        {
            _localAgentState = e.Data;
            _logger.Info($"[LocalAgentWrapper] state changed to {_localAgentState}");

            switch (_localAgentState)
            {
                case LocalAgentState.Connected when _vpnStatus != VpnStatus.Connected:
                    _vpnStatus = VpnStatus.Connected;
                    if (_vpnConfig.VpnProtocol == VpnProtocol.WireGuard)
                    {
                        _splitTunnelRouting.SetUpRoutingTable(_vpnConfig, _connectedState.Data.LocalIp);
                    }

                    InvokeStateChange(_connectedState);
                    break;
                case LocalAgentState.ServerCertificateError:
                    _origin.Disconnect(VpnError.TlsCertificateError);
                    break;
                case LocalAgentState.ClientCertificateError:
                    InvokeActionRequiredEvent(VpnError.CertificateExpired);
                    break;
            }
        }

        private void OnLocalAgentErrorOccurred(object sender, LocalAgentErrorArgs e)
        {
            _logger.Info($"[LocalAgentWrapper] error event received {e.Error} {e.Description}");

            List<VpnError> avoidDisconnectOnErrors = new()
            {
                VpnError.UnableToVerifyCert,
                VpnError.CertificateNotYetProvided,
            };

            if (_localAgentState == LocalAgentState.HardJailed && !avoidDisconnectOnErrors.Contains(e.Error))
            {
                switch (e.Error)
                {
                    case VpnError.CertificateRevoked:
                    case VpnError.CertRevokedOrExpired:
                        _origin.Disconnect(e.Error);
                        break;
                    case VpnError.CertificateExpired:
                        InvokeActionRequiredEvent(VpnError.CertificateExpired);
                        break;
                    case VpnError.SessionLimitReached:
                    case VpnError.SessionKilledDueToMultipleKeys:
                    case VpnError.SessionLimitReachedFree:
                    case VpnError.SessionLimitReachedBasic:
                    case VpnError.SessionLimitReachedPlus:
                    case VpnError.SessionLimitReachedPro:
                    case VpnError.SessionLimitReachedVisionary:
                    case VpnError.SessionLimitReachedUnknown:
                        _origin.Disconnect(VpnError.AuthorizationError);
                        break;
                }
            }
        }

        private void InvokeActionRequiredEvent(VpnError error)
        {
            InvokeStateChange(new EventArgs<VpnState>(new VpnState(VpnStatus.ActionRequired, error, _vpnConfig.VpnProtocol)));
        }

        private string GetFeatures()
        {
            return GetFeaturesJson(new FeaturesContract
            {
                NetShieldLevel = _vpnConfig.NetShieldMode,
                SplitTcp = _vpnConfig.SplitTcp,
                Bouncing = _endpoint.Server.Label,
            });
        }

        private string GetFeatures(VpnFeatures vpnFeatures)
        {
            return GetFeaturesJson(new FeaturesContract
            {
                NetShieldLevel = vpnFeatures.NetShieldMode, SplitTcp = vpnFeatures.SplitTcp,
            });
        }

        private string GetFeaturesJson(FeaturesContract contract)
        {
            return JsonConvert.SerializeObject(contract, Formatting.None,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
        }

        private void OnVpnStateChanged(object sender, EventArgs<VpnState> e)
        {
            if (_connectRequested)
            {
                switch (e.Data.Status)
                {
                    case VpnStatus.Connected:
                        _connectedState = e;
                        ConnectToTlsChannel();
                        return;
                    case VpnStatus.Disconnected:
                        CloseTlsChannel();
                        _eventReceiver.Stop();
                        _vpnStatus = VpnStatus.Disconnected;
                        if (_vpnConfig.VpnProtocol == VpnProtocol.WireGuard)
                        {
                            _splitTunnelRouting.DeleteRoutes(_vpnConfig);
                        }

                        break;
                }
            }

            InvokeStateChange(e);
        }

        private void CloseTlsChannel()
        {
            if (_isTlsChannelActive)
            {
                _isTlsChannelActive = false;
                PInvoke.Close();
            }
        }

        private void ConnectToTlsChannel()
        {
            using GoString clientCertPem = _clientCertPem.ToGoString();
            using GoString clientKeyPem = _credentials.ClientKeyPair.SecretKey.Pem.ToGoString();
            using GoString serverCaPem = VpnCertConfig.RootCa.ToGoString();
            using GoString host = LocalAgentHost.ToGoString();
            using GoString featuresJson = GetFeatures().ToGoString();
            using GoString certServerName = _endpoint.Server.Name.ToGoString();
            string result = PInvoke
                .Connect(clientCertPem, clientKeyPem, serverCaPem, host, certServerName, featuresJson, true)
                .ConvertToString();
            if (result == "")
            {
                _isTlsChannelActive = true;
                _eventReceiver.Start();
            }
            else
            {
                _logger.Error("[LocalAgentWrapper] Failed to connect to TLS channel: " + result);
                _origin.Disconnect(VpnError.Unknown);
            }
        }

        private void InvokeStateChange(EventArgs<VpnState> state)
        {
            StateChanged?.Invoke(this, state);
        }
    }
}