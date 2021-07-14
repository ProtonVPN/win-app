/*
 * Copyright (c) 2020 Proton Technologies AG
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
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.Connection
{
    /// <summary>
    /// Automatically reconnects in case of connection errors.
    /// A wrapper around <see cref="ISingleVpnConnection"/>.
    /// </summary>
    /// <remarks>
    /// Automatically reconnects if status changes to
    /// <see cref="VpnStatus.Disconnecting"/> or <see cref="VpnStatus.Disconnected"/>" with one of errors
    /// <see cref="VpnError.NetshError"/>, <see cref="VpnError.TlsError"/>,
    /// <see cref="VpnError.TimeoutError"/>, <see cref="VpnError.Unknown"/>.
    ///
    /// During reconnect status values <see cref="VpnStatus.Disconnecting"/> and <see cref="VpnStatus.Disconnected"/>
    /// are suppressed, status value <see cref="VpnStatus.Connecting"/> is replaced with
    /// <see cref="VpnStatus.Reconnecting"/>.
    /// </remarks>
    internal class ReconnectingWrapper : IVpnConnection
    {
        private readonly CancellationHandle _cancellationHandle = new();
        private readonly ILogger _logger;
        private readonly IVpnEndpointCandidates _candidates;
        private readonly IEndpointScanner _endpointScanner;
        private readonly ISingleVpnConnection _origin;

        private VpnState _state;
        private VpnConfig _config;
        private VpnProtocol _protocol;
        private VpnCredentials _credentials;
        private VpnEndpoint _endpoint;
        private bool _isToConnect;
        private bool _isToReconnect;

        public ReconnectingWrapper(
            ILogger logger,
            IVpnEndpointCandidates candidates,
            IEndpointScanner endpointScanner,
            ISingleVpnConnection origin)
        {
            _logger = logger;
            _candidates = candidates;
            _endpointScanner = endpointScanner;
            _origin = origin;

            _origin.StateChanged += Origin_StateChanged;
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged;

        public InOutBytes Total => _origin.Total;

        public void Connect(IReadOnlyList<VpnHost> servers, VpnConfig config, VpnProtocol protocol, VpnCredentials credentials)
        {
            _candidates.Set(servers);
            _candidates.Reset();
            _config = config;
            _protocol = protocol;
            _credentials = credentials;
            _isToConnect = true;
            _isToReconnect = true;

            _logger.Info("[ReconnectingWrapper] Requesting disconnect as first step of connection process.");
            CancelTokenAndDisconnect(VpnError.NoneKeepEnabledKillSwitch);
        }

        public void Disconnect(VpnError error = VpnError.None)
        {
            _isToConnect = false;
            _isToReconnect = false;
            CancelTokenAndDisconnect(error);
        }

        private void CancelTokenAndDisconnect(VpnError error)
        {
            _logger.Info($"[ReconnectingWrapper] A disconnect was requested with error '{error}'.");
            _cancellationHandle.Cancel();
            _origin.Disconnect(error);
        }

        private void Origin_StateChanged(object sender, EventArgs<VpnState> e)
        {
            _state = e.Data;

            if (_state.Status == VpnStatus.Disconnected && _isToConnect)
            {
                _isToConnect = false;
                PingAndConnectAsync();
            }
            else if (IsToCancelReconnection())
            {
                _isToReconnect = false;
            }
            else if (IsToReconnect())
            {
                ConnectToNextEndpoint();
            }

            OnStateChanged(FilterVpnState(_state));
        }

        private async Task PingAndConnectAsync()
        {
            _logger.Info("[ReconnectingWrapper] A connect is pending. Starting connection after status changed to Disconnected.");
            CancellationToken cancellationToken = _cancellationHandle.Token;
            bool isResponding = false;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Info("[ReconnectingWrapper] Disconnection has been requested. Endpoint scanning stopped.");
                    break;
                }

                VpnEndpoint endpoint = _candidates.NextIp(_protocol);
                if (endpoint == VpnEndpoint.EmptyEndpoint)
                {
                    _logger.Info($"[ReconnectingWrapper] No more endpoints in the list after server {endpoint.Server.Ip} has failed to respond to the ping.");
                    break;
                }

                isResponding = await IsEndpointRespondingAsync(endpoint, cancellationToken);
                if (isResponding)
                {
                    _logger.Info($"[ReconnectingWrapper] The server {endpoint.Server.Ip} has responded to the ping.");
                    break;
                }

                _logger.Info($"[ReconnectingWrapper] The server {endpoint.Server.Ip} has failed to respond to the ping.");
            }

            _candidates.Reset();
            HandleEndpointResponse(isResponding, cancellationToken);
        }

        private async Task<bool> IsEndpointRespondingAsync(VpnEndpoint endpoint, CancellationToken cancellationToken)
        {
            OnStateChanged(new VpnState(VpnStatus.Pinging, VpnError.None, string.Empty,
                endpoint.Server.Ip, endpoint.Protocol, endpoint.Server.Label));
            VpnEndpoint bestEndpoint = await _endpointScanner.ScanForBestEndpointAsync(endpoint, _config.Ports, cancellationToken);
            return bestEndpoint.Port != 0;
        }

        private void HandleEndpointResponse(bool isResponding, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.Info("[ReconnectingWrapper] Disconnection has been requested. Connection canceled.");
                return;
            }

            if (isResponding)
            {
                _logger.Info("[ReconnectingWrapper] At least one server has responded to a ping. Attempting connections.");
                ConnectToNextEndpoint();
            }
            else
            {
                _logger.Info("[ReconnectingWrapper] No server has responded to a ping. Disconnecting.");
                Disconnect(VpnError.TimeoutError);
            }
        }

        private void ConnectToNextEndpoint()
        {
            _endpoint = _candidates.NextHost(_protocol);
            bool isEndpointAvailableToConnect = _endpoint?.Server != null && !_endpoint.Server.IsEmpty();
            if (isEndpointAvailableToConnect)
            {
                _logger.Info($"[ReconnectingWrapper] Next endpoint is {_endpoint.Server.Ip}/{_endpoint.Server.Label}. Connecting.");
                _origin.Connect(_endpoint, _credentials, _config);
            }
            else
            {
                _isToReconnect = false;
                _logger.Info("[ReconnectingWrapper] No more VPN endpoints to try. Disconnecting.");
                Disconnect(_state.Error);
            }
        }

        private bool IsToCancelReconnection()
        {
            return !_isToConnect &&
                   (_state.Status == VpnStatus.Disconnecting || _state.Status == VpnStatus.Disconnected) &&
                   !IsServerError(_state.Error);
        }

        private bool IsServerError(VpnError error)
        {
            return error == VpnError.NetshError ||
                   error == VpnError.TlsError ||
                   error == VpnError.TimeoutError ||
                   error == VpnError.Unknown;
        }

        private bool IsToReconnect()
        {
            return _isToReconnect &&
                (_state.Status == VpnStatus.Disconnecting || _state.Status == VpnStatus.Disconnected) &&
                IsServerError(_state.Error);
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
            return _isToReconnect &&
                   (state.Status == VpnStatus.Disconnecting || state.Status == VpnStatus.Disconnected);
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
                state.Protocol,
                state.Label);
        }

        private void OnStateChanged(VpnState state)
        {
            if (state != null)
            {
                StateChanged?.Invoke(this, new EventArgs<VpnState>(state));
            }
        }

        public void UpdateServers(IReadOnlyList<VpnHost> servers, VpnConfig config)
        {
            _candidates.Set(servers);
            _logger.Info("[ReconnectingWrapper] VPN endpoint candidates updated.");

            if (_state.Status == VpnStatus.Disconnected ||
                _state.Status == VpnStatus.Disconnecting ||
                _candidates.Contains(_candidates.Current))
            {
                return;
            }

            _logger.Info("[ReconnectingWrapper] Current VPN endpoint is not in VPN endpoint candidates. Disconnecting.");
            Disconnect();
        }
    }
}
