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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.Threading;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.DisconnectLogs;
using ProtonVPN.Logging.Contracts.Events.ServerSwitchLogs;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.ServerValidation;

namespace ProtonVPN.Vpn.Connection
{
    internal class ReconnectingWrapper : IVpnConnection
    {
        private readonly CancellationHandle _cancellationHandle = new();
        private readonly ILogger _logger;
        private readonly IVpnEndpointCandidates _candidates;
        private readonly IServerValidator _serverValidator;
        private readonly IEndpointScanner _endpointScanner;
        private readonly IIssueReporter _issueReporter;
        private readonly ISingleVpnConnection _origin;

        private VpnState _state;
        private VpnConfig _config;
        private VpnCredentials _credentials;
        private VpnEndpoint _endpoint;
        private bool _isToConnect;
        private bool _isToReconnect;
        private bool _isToDiscardProtocol;

        public ReconnectingWrapper(
            ILogger logger,
            IVpnEndpointCandidates candidates,
            IServerValidator serverValidator,
            IEndpointScanner endpointScanner,
            IIssueReporter issueReporter,
            ISingleVpnConnection origin)
        {
            _logger = logger;
            _candidates = candidates;
            _serverValidator = serverValidator;
            _endpointScanner = endpointScanner;
            _issueReporter = issueReporter;
            _origin = origin;

            _origin.StateChanged += Origin_StateChanged;
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged;

        public event EventHandler<ConnectionDetails> ConnectionDetailsChanged
        {
            add => _origin.ConnectionDetailsChanged += value;
            remove => _origin.ConnectionDetailsChanged -= value;
        }

        public NetworkTraffic NetworkTraffic => _origin.NetworkTraffic;

        public void Connect(IReadOnlyList<VpnHost> servers, VpnConfig config, VpnCredentials credentials)
        {
            _candidates.Set(servers);
            _candidates.Reset();
            _config = config;
            _credentials = credentials;
            _isToConnect = true;
            _isToReconnect = true;

            _logger.Info<DisconnectTriggerLog>("Requesting disconnect as the first step of a connection process.");
            CancelTokenAndDisconnect(VpnError.NoneKeepEnabledKillSwitch);
        }

        public void ResetConnection()
        {
            if (_isToReconnect)
            {
                _candidates.Reset();
                _isToConnect = true;
                _isToReconnect = true;

                _logger.Info<DisconnectTriggerLog>("Requesting disconnect as the first step of a connection reset process.");
                CancelTokenAndDisconnect(VpnError.NoneKeepEnabledKillSwitch);
            }
        }

        public void SetFeatures(VpnFeatures vpnFeatures)
        {
            _origin.SetFeatures(vpnFeatures);
        }

        public void RequestNetShieldStats()
        {
            _origin.RequestNetShieldStats();
        }

        public void RequestConnectionDetails()
        {
            _origin.RequestConnectionDetails();
        }

        public void Disconnect(VpnError error = VpnError.None)
        {
            _isToConnect = false;
            _isToReconnect = false;
            CancelTokenAndDisconnect(error);
        }

        private void CancelTokenAndDisconnect(VpnError error)
        {
            _logger.Info<DisconnectLog>($"A disconnect was requested with error '{error}'.");
            _cancellationHandle.Cancel();
            _origin.Disconnect(error);
        }

        private void Origin_StateChanged(object sender, EventArgs<VpnState> e)
        {
            _state = e.Data;

            if (_state.Status == VpnStatus.Connecting || _state.Status == VpnStatus.Reconnecting)
            {
                _isToDiscardProtocol = true;
            }

            if (IsToHandleAdapterError())
            {
                OnAdapterError(_state.VpnProtocol, _state.Error);
            }
            else if (_state.Status == VpnStatus.Disconnected && _isToConnect)
            {
                _isToConnect = false;
                _logger.Info<ConnectLog>("A connect is pending. " +
                    "Starting connection after status changed to Disconnected.");
                PingAndConnectAsync();
                return;
            }
            else if (IsToCancelReconnection())
            {
                _isToReconnect = false;
            }
            else if (IsToReconnect(_state))
            {
                _logger.Info<ServerSwitchTriggerLog>("Trying the next server. " +
                    $"Status: '{_state.Status}', Error: '{_state.Error}'.");
                ConnectToNextEndpoint(skipCurrentIp: _state.Error == VpnError.PingTimeoutError);
                return;
            }

            OnStateChanged(FilterVpnState(_state));
        }

        private bool IsToHandleAdapterError()
        {
            return _isToDiscardProtocol && IsAdapterError(_state.Error) &&
                   (_state.Status is VpnStatus.Disconnecting or VpnStatus.Disconnected);
        }

        private void OnAdapterError(VpnProtocol protocol, VpnError vpnError)
        {
            _isToDiscardProtocol = false;

            if (vpnError == VpnError.InterfaceHasForwardingEnabled)
            {
                HandleInterfaceForwardingError();
            }
            else
            {
                if (_config.PreferredProtocols.Contains(protocol))
                {
                    DiscardProtocol(protocol);
                }
                else if (protocol != VpnProtocol.Smart)
                {
                    _logger.Warn<ConnectLog>($"Failed to find {protocol} in the preferred protocols list: " +
                        $"[{string.Join(", ", _config.PreferredProtocols)}]");
                }
            }

            if (_config.PreferredProtocols.Count == 0)
            {
                _logger.Warn<DisconnectTriggerLog>("Preferred protocols list is empty. Disconnecting.");
                Disconnect(VpnError.ServerUnreachable);
            }
            else
            {
                _logger.Info<ConnectLog>("Preferred protocols list is not empty. " +
                    "Reconnecting after discarding protocol.");
                _candidates.Reset();
                _isToConnect = true;
                _isToReconnect = true;
            }
        }

        private void HandleInterfaceForwardingError()
        {
            List<VpnProtocol> wireGuardProtocols = new[] {
                    VpnProtocol.WireGuardUdp,
                    VpnProtocol.WireGuardTcp,
                    VpnProtocol.WireGuardTls
                }
            .Where(_config.PreferredProtocols.Contains)
            .ToList();

            if (wireGuardProtocols.Count > 0)
            {
                wireGuardProtocols.ForEach(DiscardProtocol);
                _issueReporter.CaptureMessage("WireGuard protocols discarded due to interface Forwarding being enabled.");
            }
            else
            {
                _logger.Warn<ConnectLog>("Preferred protocols did not contain any WireGuard protocols.");
            }
        }

        private void DiscardProtocol(VpnProtocol protocol)
        {
            _logger.Info<ConnectLog>($"Discarding protocol '{protocol}'.");
            _config.PreferredProtocols.Remove(protocol);
        }

        private async Task PingAndConnectAsync()
        {
            _logger.Info<ConnectLog>("A connect is pending and status is Disconnected. Pinging.");
            CancellationToken cancellationToken = _cancellationHandle.Token;
            bool isResponding = false;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Info<DisconnectLog>("Disconnection has been requested. Endpoint scanning stopped.");
                    break;
                }

                VpnEndpoint endpoint = _candidates.NextIp(_config);
                if (endpoint.IsEmpty)
                {
                    _logger.Warn<ConnectLog>($"No more endpoints in the list after server {endpoint.Server.Ip} has failed to respond to the ping.");
                    break;
                }
                VpnError error = _serverValidator.Validate(endpoint.Server);
                if (error != VpnError.None)
                {
                    _logger.Error<ConnectLog>($"The server validation failed for IP '{endpoint.Server.Ip}' and Label '{endpoint.Server.Label}'.");
                    continue;
                }
                isResponding = await IsEndpointRespondingAsync(endpoint, cancellationToken);
                if (isResponding)
                {
                    _logger.Info<ConnectScanResultLog>($"The server {endpoint.Server.Ip} has responded to the ping.");
                    break;
                }

                _logger.Info<ConnectScanFailLog>($"The server {endpoint.Server.Ip} has failed to respond to the ping.");
            }

            _candidates.Reset();
            HandleEndpointResponse(isResponding, cancellationToken);
        }

        private async Task<bool> IsEndpointRespondingAsync(VpnEndpoint endpoint, CancellationToken cancellationToken)
        {
            OnStateChanged(new VpnState(VpnStatus.Pinging, VpnError.None, string.Empty, endpoint.Server.Ip, endpoint.Port,
                _config.VpnProtocol, openVpnAdapter: _config.OpenVpnAdapter, label: endpoint.Server.Label));

            VpnEndpoint bestEndpoint = await _endpointScanner.ScanForBestEndpointAsync(
                endpoint, _config.Ports, _config.PreferredProtocols, cancellationToken);
            return bestEndpoint.Port != 0;
        }

        private void HandleEndpointResponse(bool isResponding, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.Info<DisconnectLog>("Disconnection has been requested. Connection canceled.");
                return;
            }

            if (isResponding)
            {
                _logger.Info<ServerSwitchTriggerLog>("At least one server has responded to a ping. Attempting connections.");
                ConnectToNextEndpoint(skipCurrentIp: false);
            }
            else
            {
                _logger.Info<ConnectScanFailLog>("No server has responded to a ping.");
                _logger.Info<DisconnectTriggerLog>("Disconnecting due to failed scan.");
                Disconnect(VpnError.PingTimeoutError);
            }
        }

        private void ConnectToNextEndpoint(bool skipCurrentIp)
        {
            _endpoint = skipCurrentIp ? _candidates.NextIp(_config) : _candidates.NextHost(_config);
            bool isEndpointAvailableToConnect = _endpoint?.Server != null && !_endpoint.Server.IsEmpty();
            if (isEndpointAvailableToConnect)
            {
                _logger.Info<ServerSwitchSelectedLog>($"Next endpoint is {_endpoint.Server.Ip}/{_endpoint.Server.Label}. Connecting.");
                _origin.Connect(_endpoint, _credentials, _config);
            }
            else
            {
                _isToReconnect = false;
                _logger.Info<ServerSwitchFailedLog>("No more VPN endpoints to try.");
                _logger.Info<DisconnectTriggerLog>("Disconnecting since there are no more servers to connect to.");
                Disconnect(_state.Error);
            }
        }

        private bool IsToCancelReconnection()
        {
            return !_isToConnect &&
                   (_state.Status is VpnStatus.Disconnecting or VpnStatus.Disconnected) &&
                   !IsServerError(_state.Error) && !IsAdapterError(_state.Error);
        }

        private bool IsServerError(VpnError error)
        {
            return error == VpnError.ServerValidationError ||
                   error == VpnError.NetshError ||
                   error == VpnError.TlsError ||
                   error == VpnError.PingTimeoutError ||
                   error == VpnError.ServerSessionError ||
                   error == VpnError.Unknown;
        }

        private bool IsAdapterError(VpnError error)
        {
            return error == VpnError.ServerUnreachable ||
                   error == VpnError.AdapterTimeoutError ||
                   error == VpnError.InterfaceHasForwardingEnabled;
        }

        private bool IsToReconnect(VpnState state)
        {
            return _isToReconnect &&
                (state.Status == VpnStatus.Disconnecting || state.Status == VpnStatus.Disconnected) &&
                IsServerError(state.Error);
        }

        private VpnState FilterVpnState(VpnState state)
        {
            if (IsToSuppressVpnState(state))
            {
                return null;
            }

            if (ShouldBeReconnecting(state))
            {
                return CreateReconnectingVpnState(state);
            }

            return state;
        }

        private bool IsToSuppressVpnState(VpnState state)
        {
            return (_isToReconnect || _isToConnect) &&
                   (state.Status is VpnStatus.Disconnecting or VpnStatus.Disconnected);
        }

        private bool ShouldBeReconnecting(VpnState state)
        {
            return state.Status == VpnStatus.Connecting;
        }

        private VpnState CreateReconnectingVpnState(VpnState state)
        {
            return new(
                VpnStatus.Reconnecting,
                state.Error,
                state.LocalIp,
                state.RemoteIp,
                state.EndpointPort,
                state.VpnProtocol,
                openVpnAdapter: state.OpenVpnAdapter,
                label: state.Label);
        }

        private void OnStateChanged(VpnState state)
        {
            if (state != null)
            {
                StateChanged?.Invoke(this, new EventArgs<VpnState>(state));
            }
        }
    }
}