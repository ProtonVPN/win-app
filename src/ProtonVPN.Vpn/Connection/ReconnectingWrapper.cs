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
        private readonly ILogger _logger;
        private readonly ITaskQueue _taskQueue;
        private readonly IVpnEndpointCandidates _candidates;
        private readonly IEndpointScanner _endpointScanner;
        private readonly ISingleVpnConnection _origin;
        private readonly CancellationHandle _cancellationHandle = new();

        private bool _reconnectPending;
        private bool _reconnecting;
        private bool _disconnecting;
        private bool _connecting;
        private bool _connectPending;
        private bool _isToAttemptReconnection;
        private object _connectLock = new();
        private VpnProtocol _protocol;
        private VpnCredentials _credentials;
        private VpnEndpoint _endpoint;
        private VpnConfig _config;
        private VpnState _state = new(VpnStatus.Disconnected);

        public ReconnectingWrapper(
            ILogger logger,
            ITaskQueue taskQueue,
            IVpnEndpointCandidates candidates,
            IEndpointScanner endpointScanner,
            ISingleVpnConnection origin)
        {
            _logger = logger;
            _taskQueue = taskQueue;
            _candidates = candidates;
            _endpointScanner = endpointScanner;
            _origin = origin;

            _origin.StateChanged += Origin_StateChanged;
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged;

        public InOutBytes Total => _origin.Total;

        public async void Connect(IReadOnlyList<VpnHost> servers, VpnConfig config, VpnProtocol protocol, VpnCredentials credentials)
        {
            _connecting = true;
            _isToAttemptReconnection = true;
            lock(_connectLock)
            {
                _connectPending = true;
            }
            DisconnectIfNotDisconnected();
            SetFieldsOnConnect(servers, config, protocol, credentials);
            await PingAndConnectIfPendingAsync();
        }

        private async Task PingAndConnectIfPendingAsync()
        {
            bool isToConnect = false;
            lock(_connectLock)
            {
                if (_connectPending && _state.Status == VpnStatus.Disconnected)
                {
                    _connectPending = false;
                    isToConnect = true;
                }
            }

            if (isToConnect)
            {
                _logger.Info("[ReconnectingWrapper] A connect is pending. Calling connect after status changed to Disconnected.");
                await PingAndConnectAsync();
            }
        }

        private async Task PingAndConnectAsync()
        {
            int numOfEndpoints = _candidates.Count();
            if (numOfEndpoints > 1)
            {
                _logger.Info($"Multiple VPN endpoints ({numOfEndpoints} endpoints). Scanning for availability.");
                await PingServersBeforeAttemptingConnectionsAsync();
            }
            else
            {
                _logger.Info("Single VPN endpoint. Attempting connection.");
                ConnectToNextEndpoint();
            }
        }

        private void DisconnectIfNotDisconnected()
        {
            _cancellationHandle.Cancel();
            _reconnectPending = false;
            _reconnecting = false;
            _disconnecting = true;

            HandlingRequestsWrapper handlingRequestsWrapper = (HandlingRequestsWrapper)_origin;
            handlingRequestsWrapper.PassThroughDisconnectIfNotDisconnected(VpnError.None);
        }

        private void SetFieldsOnConnect(IReadOnlyList<VpnHost> servers, VpnConfig config, VpnProtocol protocol, VpnCredentials credentials)
        {
            _protocol = protocol;
            _credentials = credentials;
            _config = config;
            _candidates.Set(servers);
            _candidates.Reset();
        }

        private async Task PingServersBeforeAttemptingConnectionsAsync()
        {
            CancellationToken cancellationToken = _cancellationHandle.Token;
            bool isResponding = false;
            _candidates.Reset();
            
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    DisconnectDueToCancel();
                    return;
                }
                VpnEndpoint endpoint = _candidates.NextIp(_protocol);
                if (endpoint == VpnEndpoint.EmptyEndpoint)
                {
                    _logger.Info($"No more endpoints in the list after server {endpoint.Server.Ip} has failed to respond to the ping.");
                    break;
                }

                OnStateChanged(new VpnState(VpnStatus.Reconnecting, VpnError.None, string.Empty, 
                    endpoint.Server.Ip, endpoint.Protocol, endpoint.Server.Label));
                VpnEndpoint bestEndpoint = await _endpointScanner.ScanForBestEndpointAsync(endpoint, _config.Ports, cancellationToken);
                isResponding = bestEndpoint.Port != 0;
                if (isResponding)
                {
                    _logger.Info($"The server {endpoint.Server.Ip} has responded to the ping.");
                    break;
                }
                    
                _logger.Info($"The server {endpoint.Server.Ip} has failed to respond to the ping.");
            }

            HandlePingResponse(isResponding, cancellationToken);
        }

        private void DisconnectDueToCancel()
        {
            _logger.Info("Disconnection has been requested. Endpoint scanning stopped.");
            Disconnect();
        }

        private void HandlePingResponse(bool isResponding, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                DisconnectDueToCancel();
                return;
            }

            if (isResponding)
            {
                _logger.Info("At least one server has responded to a ping. Attempting connections.");
                _candidates.Reset();
                ConnectToNextEndpoint();
            }
            else
            {
                _logger.Info("No server has responded to a ping. Disconnecting.");
                Disconnect(VpnError.TimeoutError);
            }
        }

        private void ConnectToNextEndpoint()
        {
            _endpoint = _candidates.NextHost(_protocol);
            if (_endpoint?.Server == null || _endpoint.Server.IsEmpty())
            {
                _logger.Info("No more VPN endpoints to try, disconnecting");
                Disconnect(_state.Error);
            }
            else
            {
                _origin.Connect(_endpoint, _credentials, _config);
            }
        }

        public void Disconnect(VpnError error = VpnError.None)
        {
            _cancellationHandle.Cancel();
            _reconnectPending = false;
            _reconnecting = false;
            _disconnecting = true;
            _connecting = false;
            _isToAttemptReconnection = false;
            _origin.Disconnect(error);
        }

        public void UpdateServers(IReadOnlyList<VpnHost> servers, VpnConfig config)
        {
            _candidates.Set(servers);
            _logger.Info("VPN endpoint candidates updated");

            if (_reconnectPending ||
                _state.Status == VpnStatus.Disconnected ||
                _state.Status == VpnStatus.Disconnecting ||
                _candidates.Contains(_candidates.Current))
            {
                return;
            }

            _logger.Info("Current VPN endpoint is not in VPN endpoint candidates, disconnecting");
            _origin.Disconnect(VpnError.Unknown);
        }

        private async void Origin_StateChanged(object sender, EventArgs<VpnState> e)
        {
            _state = e.Data;
            
            if (_disconnecting && _state.Status == VpnStatus.Disconnected)
            {
                _disconnecting = false;
            }

            if (_connecting && _state.Status == VpnStatus.Connected)
            {
                _connecting = false;
            }

            if (_reconnecting && !Reconnecting(_state))
            {
                _reconnecting = false;
            }

            if (IsReconnectRequired(_state))
            {
                _logger.Info($"Disconnecting because of {_state.Error}, scheduling reconnect");

                _reconnectPending = true;
                Queued(Reconnect);
            }

            await PingAndConnectIfPendingAsync();

            if (_state.Status == VpnStatus.Reconnecting)
            {
                _reconnecting = true;
            }

            OnStateChanged(Filtered(_state));
        }

        private bool Reconnecting(VpnState state)
        {
            return state.Status == VpnStatus.Reconnecting ||
                   state.Status == VpnStatus.Connecting;
        }

        private bool IsReconnectRequired(VpnState state)
        {
            bool isReconnectRequired;
            lock (_connectLock)
            {
                isReconnectRequired =
                    _isToAttemptReconnection &&
                    !_connectPending &&
                    !_reconnectPending &&
                    !_disconnecting &&
                    (state.Status == VpnStatus.Disconnecting ||
                     state.Status == VpnStatus.Disconnected) &&
                    (state.Error == VpnError.NetshError ||
                     state.Error == VpnError.TlsError ||
                     state.Error == VpnError.TimeoutError ||
                     state.Error == VpnError.Unknown);
            }
            return isReconnectRequired;
        }

        private void Reconnect()
        {
            if (_reconnectPending)
            {
                _reconnectPending = false;
                _reconnecting = true;
                ConnectToNextEndpoint();
            }
        }

        private void Queued(Action action)
        {
            _taskQueue.Enqueue(action);
        }

        private VpnState Filtered(VpnState state)
        {
            if (IsToSuppressVpnState(state))
            {
                return null;
            }

            if (ShouldBeReconnecting(state))
            {
                return CreateVpnState(state, VpnStatus.Reconnecting, VpnProtocol.Auto);
            }

            return CreateVpnState(state);
        }

        private VpnState CreateVpnState(VpnState state, VpnStatus? status = null, VpnProtocol? protocol = null)
        {
            return new(
                status ?? state.Status,
                state.Error,
                state.LocalIp,
                state.RemoteIp,
                protocol ?? state.Protocol,
                state.Label);
        }

        private bool IsToSuppressVpnState(VpnState state)
        {
            bool isToSuppress; 
            lock (_connectLock)
            {
                isToSuppress = (_isToAttemptReconnection || _reconnectPending || _connectPending || _connecting) && 
                               (state.Status == VpnStatus.Disconnecting || state.Status == VpnStatus.Disconnected);
            }
            return isToSuppress;
        }

        private bool ShouldBeReconnecting(VpnState state)
        {
            return _reconnecting && 
                   state.Status == VpnStatus.Connecting;
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
