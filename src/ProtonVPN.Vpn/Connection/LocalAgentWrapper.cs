/*
 * Copyright (c) 2023 Proton AG
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtonVPN.Common;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Go;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectionLogs;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectLogs;
using ProtonVPN.Common.Logging.Categorization.Events.LocalAgentLogs;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Config;
using ProtonVPN.Vpn.Gateways;
using ProtonVPN.Vpn.LocalAgent;
using ProtonVPN.Vpn.LocalAgent.Contracts;
using ProtonVPN.Vpn.SplitTunnel;
using PInvoke = ProtonVPN.Vpn.LocalAgent.PInvoke;

namespace ProtonVPN.Vpn.Connection
{
    internal class LocalAgentWrapper : ISingleVpnConnection
    {
        private const int DEFAULT_PORT = 65432;
        private const int CONNECT_TIMEOUT = 10000;

        private readonly ILogger _logger;
        private readonly EventReceiver _eventReceiver;
        private readonly SplitTunnelRouting _splitTunnelRouting;
        private readonly IGatewayCache _gatewayCache;
        private readonly ISingleVpnConnection _origin;

        private VpnStatus _currentStatus;
        private VpnEndpoint _endpoint;
        private VpnCredentials _credentials;
        private VpnConfig _vpnConfig;
        private bool _isTlsChannelActive;
        private bool _isConnectRequested;
        private bool _tlsConnected;
        private EventArgs<VpnState> _vpnState;
        private string _clientCertPem = string.Empty;
        private string _localIp = string.Empty;
        private readonly ISingleAction _timeoutAction;
        private readonly List<VpnError> _avoidDisconnectOnErrors = new()
        {
            VpnError.UnableToVerifyCert,
            VpnError.CertificateNotYetProvided,
        };

        public LocalAgentWrapper(
            ILogger logger,
            EventReceiver eventReceiver,
            SplitTunnelRouting splitTunnelRouting,
            IGatewayCache gatewayCache,
            ISingleVpnConnection origin)
        {
            _logger = logger;
            _eventReceiver = eventReceiver;
            _splitTunnelRouting = splitTunnelRouting;
            _gatewayCache = gatewayCache;
            _origin = origin;
            origin.StateChanged += OnVpnStateChanged;
            eventReceiver.StateChanged += OnLocalAgentStateChanged;
            eventReceiver.ErrorOccurred += OnLocalAgentErrorOccurred;
            _timeoutAction = new SingleAction(TimeoutAction);
            _timeoutAction.Completed += OnTimeoutActionCompleted;
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged;
        public event EventHandler<ConnectionDetails> ConnectionDetailsChanged
        {
            add => _eventReceiver.ConnectionDetailsChanged += value;
            remove => _eventReceiver.ConnectionDetailsChanged -= value;
        }

        public InOutBytes Total => _origin.Total;

        public void Connect(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig config)
        {
            _logger.Info<LocalAgentLog>("Connect action started");
            _isConnectRequested = true;
            _endpoint = endpoint;
            _credentials = credentials;
            _vpnConfig = config;
            _clientCertPem = credentials.ClientCertPem;
            _origin.Connect(endpoint, credentials, config);
        }

        public void Disconnect(VpnError error)
        {
            _logger.Info<LocalAgentLog>("Disconnect action started");
            StopTimeoutAction();
            _eventReceiver.Stop();
            CloseTlsChannel();
            _origin.Disconnect(error);
        }

        public void SetFeatures(VpnFeatures vpnFeatures)
        {
            if (!_isTlsChannelActive)
            {
                return;
            }

            UpdateVpnConfig(vpnFeatures);
            using GoString goFeatures = GetFeatures(vpnFeatures).ToGoString();
            PInvoke.SetFeatures(goFeatures);
        }

        private void UpdateVpnConfig(VpnFeatures vpnFeatures)
        {
            if (_vpnConfig != null)
            {
                _vpnConfig = new VpnConfig(CreateVpnConfigParameters(vpnFeatures));
            }
        }

        private VpnConfigParameters CreateVpnConfigParameters(VpnFeatures vpnFeatures)
        {
            return new()
            {
                Ports = _vpnConfig.Ports,
                CustomDns = _vpnConfig.CustomDns,
                SplitTunnelMode = _vpnConfig.SplitTunnelMode,
                SplitTunnelIPs = _vpnConfig.SplitTunnelIPs,
                OpenVpnAdapter = _vpnConfig.OpenVpnAdapter,
                VpnProtocol = _vpnConfig.VpnProtocol,
                PreferredProtocols = _vpnConfig.PreferredProtocols,
                NetShieldMode = vpnFeatures.NetShieldMode,
                SplitTcp = vpnFeatures.SplitTcp,
                PortForwarding = vpnFeatures.PortForwarding
            };
        }

        public void UpdateAuthCertificate(string certificate)
        {
            _clientCertPem = certificate;
            _logger.Info<LocalAgentLog>("Client certificate updated. Closing existing TLS channel and reconnecting.");
            _eventReceiver.Stop();
            CloseTlsChannel();
            ConnectToTlsChannel();
        }

        private async Task TimeoutAction(CancellationToken cancellationToken)
        {
            await Task.Delay(CONNECT_TIMEOUT, cancellationToken);

            if (!_tlsConnected)
            {
                _logger.Info<LocalAgentLog>(
                    $"Failed to connect to TLS channel in {TimeSpan.FromMilliseconds(CONNECT_TIMEOUT).Seconds} seconds. " +
                    "Disconnecting with ServerUnreachable error.");
                _origin.Disconnect(VpnError.ServerUnreachable);
            }
        }

        private void OnTimeoutActionCompleted(object sender, TaskCompletedEventArgs e)
        {
            _logger.Info<LocalAgentLog>("Timeout action completed.");
        }

        private void OnLocalAgentStateChanged(object sender, EventArgs<LocalAgentState> e)
        {
            _logger.Info<LocalAgentStateChangeLog>($"State changed to {e.Data}");

            switch (e.Data)
            {
                case LocalAgentState.Connected:
                    OnLocalAgentStateChangedToConnected();
                    break;
                case LocalAgentState.ServerCertificateError:
                    _origin.Disconnect(VpnError.TlsCertificateError);
                    break;
                case LocalAgentState.ClientCertificateExpiredError:
                case LocalAgentState.ClientCertificateUnknownCA:
                    InvokeStateChange(VpnStatus.ActionRequired, VpnError.CertificateExpired);
                    break;
            }
        }

        private void OnLocalAgentStateChangedToConnected()
        {
            if (_tlsConnected)
            {
                InvokeStateChange(_currentStatus);
            }
            else
            {
                _tlsConnected = true;
                if (_vpnConfig.VpnProtocol == VpnProtocol.WireGuard)
                {
                    _splitTunnelRouting.SetUpRoutingTable(_vpnConfig, _vpnState.Data.LocalIp);
                }

                StopTimeoutAction();
                _logger.Info<ConnectConnectedLog>("Connected state triggered by Local Agent.");
                InvokeStateChange(VpnStatus.Connected);
            }
        }

        private void StopTimeoutAction()
        {
            if (_timeoutAction.IsRunning)
            {
                _timeoutAction.Cancel();
            }
        }

        private void OnLocalAgentErrorOccurred(object sender, LocalAgentErrorArgs e)
        {
            _logger.Info<LocalAgentErrorLog>($"Error event received {e.Error} {e.Description}");

            if (!_avoidDisconnectOnErrors.Contains(e.Error))
            {
                switch (e.Error)
                {
                    case VpnError.CertificateRevoked:
                    case VpnError.CertRevokedOrExpired:
                    case VpnError.PlanNeedsToBeUpgraded:
                        _origin.Disconnect(e.Error);
                        break;
                    case VpnError.CertificateExpired:
                        InvokeStateChange(VpnStatus.ActionRequired, VpnError.CertificateExpired);
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

        private string GetFeatures()
        {
            return GetFeaturesJson(new FeaturesContract
            {
                Bouncing = _endpoint.Server.Label,
                SplitTcp = _vpnConfig.SplitTcp,
                NetShieldLevel = _vpnConfig.NetShieldMode,
                SafeMode = !_vpnConfig.AllowNonStandardPorts,
                PortForwarding = _vpnConfig.PortForwarding,
                RandomizedNat = !_vpnConfig.ModerateNat,
            });
        }

        private string GetFeatures(VpnFeatures vpnFeatures)
        {
            return GetFeaturesJson(new FeaturesContract
            {
                SplitTcp = vpnFeatures.SplitTcp,
                NetShieldLevel = vpnFeatures.NetShieldMode,
                SafeMode = !vpnFeatures.AllowNonStandardPorts,
                PortForwarding = vpnFeatures.PortForwarding,
                RandomizedNat = !vpnFeatures.ModerateNat,
            });
        }

        private string GetFeaturesJson(FeaturesContract contract)
        {
            return JsonConvert.SerializeObject(contract, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        private void OnVpnStateChanged(object sender, EventArgs<VpnState> e)
        {
            if (_isConnectRequested)
            {
                switch (e.Data.Status)
                {
                    case VpnStatus.Connected:
                        _localIp = e.Data.LocalIp;
                        HandleVpnConnectedState();
                        return;
                    case VpnStatus.Disconnected:
                    case VpnStatus.Reconnecting:
                        HandleVpnDisconnectedState();
                        break;
                }
            }

            _vpnState = e;
            InvokeStateChange(e);
        }

        private void HandleVpnConnectedState()
        {
            if (_credentials.ClientCertPem.IsNullOrEmpty())
            {
                InvokeStateChange(VpnStatus.Connected);
            }
            else
            {
                InvokeStateChange(VpnStatus.AssigningIp);
                ConnectToTlsChannel();
                _timeoutAction.Run();
            }
        }

        private void HandleVpnDisconnectedState()
        {
            if (_credentials.ClientCertPem.IsNullOrEmpty())
            {
                return;
            }

            CloseTlsChannel();
            _eventReceiver.Stop();
            if (_vpnConfig.VpnProtocol == VpnProtocol.WireGuard)
            {
                _splitTunnelRouting.DeleteRoutes(_vpnConfig);
            }
        }

        private void CloseTlsChannel()
        {
            if (_isTlsChannelActive)
            {
                _isTlsChannelActive = false;
                _tlsConnected = false;
                PInvoke.Close();
            }
        }

        private void ConnectToTlsChannel()
        {
            IPAddress gatewayIPAddress = _gatewayCache.Get();
            if (gatewayIPAddress == null)
            {
                _logger.Error<ConnectionErrorLog>("Default gateway is missing. Disconnecting.");
                _origin.Disconnect(VpnError.Unknown);
                return;
            }

            using GoString clientCertPem = _clientCertPem.ToGoString();
            using GoString clientKeyPem = _credentials.ClientKeyPair.SecretKey.Pem.ToGoString();
            using GoString serverCaPem = VpnCertConfig.RootCa.ToGoString();
            using GoString host = $"{gatewayIPAddress}:{DEFAULT_PORT}".ToGoString();
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
                _logger.Error<LocalAgentLog>("Failed to connect to TLS channel: " + result);
                _origin.Disconnect(GetVpnError(result));
            }
        }

        private VpnError GetVpnError(string result)
        {
            return result.Contains("private key does not match public key")
                ? VpnError.ClientKeyMismatch
                : VpnError.Unknown;
        }

        private void InvokeStateChange(VpnStatus status, VpnError? error = null)
        {
            _currentStatus = status;
            InvokeStateChange(new EventArgs<VpnState>(new VpnState(
                status,
                error ?? _vpnState?.Data.Error ?? VpnError.None,
                _localIp,
                _vpnState?.Data.RemoteIp ?? string.Empty,
                _vpnConfig?.VpnProtocol ?? VpnProtocol.Smart,
                _vpnConfig?.PortForwarding ?? false,
                _vpnConfig?.OpenVpnAdapter,
                _vpnState?.Data.Label ?? string.Empty)));
        }

        private void InvokeStateChange(EventArgs<VpnState> state)
        {
            StateChanged?.Invoke(this, state);
        }
    }
}