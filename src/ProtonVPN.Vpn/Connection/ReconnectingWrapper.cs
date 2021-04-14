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
        private readonly ISingleVpnConnection _origin;

        private bool _reconnectPending;
        private bool _reconnecting;
        private bool _disconnecting;
        private VpnProtocol _protocol;
        private VpnCredentials _credentials;
        private VpnEndpoint _endpoint;
        private VpnConfig _config;
        private VpnState _state = new VpnState(VpnStatus.Disconnected);

        public ReconnectingWrapper(
            ILogger logger,
            ITaskQueue taskQueue,
            IVpnEndpointCandidates candidates,
            ISingleVpnConnection origin)
        {
            _logger = logger;
            _taskQueue = taskQueue;
            _candidates = candidates;
            _origin = origin;

            _origin.StateChanged += Origin_StateChanged;
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged;

        public InOutBytes Total => _origin.Total;

        public void Connect(IReadOnlyList<VpnHost> servers, VpnConfig config, VpnProtocol protocol, VpnCredentials credentials)
        {
            _reconnectPending = false;
            _reconnecting = false;
            _disconnecting = false;
            _protocol = protocol;
            _credentials = credentials;
            _config = config;

            _candidates.Set(servers);
            _candidates.Reset();

            ConnectToNextEndpoint();
        }

        private void ConnectToNextEndpoint()
        {
            _endpoint = _candidates.Next(_protocol);
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
            _reconnectPending = false;
            _reconnecting = false;
            _disconnecting = true;
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

        private void Origin_StateChanged(object sender, EventArgs<VpnState> e)
        {
            _state = e.Data;

            if (_reconnecting && !Reconnecting(_state))
                _reconnecting = false;

            if (ReconnectRequired(_state))
            {
                _logger.Info($"Disconnecting because of {_state.Error}, scheduling reconnect");

                _reconnectPending = true;
                Queued(Reconnect);
            }

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

        private bool ReconnectRequired(VpnState state)
        {
            return !_reconnectPending &&
                   !_disconnecting &&
                   (state.Status == VpnStatus.Disconnecting ||
                    state.Status == VpnStatus.Disconnected) &&
                   (state.Error == VpnError.NetshError ||
                    state.Error == VpnError.TlsError ||
                    state.Error == VpnError.TimeoutError ||
                    state.Error == VpnError.Unknown);
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
            if (ShouldSuppress(state))
            {
                return null;
            }

            if (ShouldBeReconnecting(state))
            {
                return new VpnState(
                    VpnStatus.Reconnecting,
                    state.Error,
                    state.LocalIp,
                    state.RemoteIp);
            }

            return state;
        }

        private bool ShouldSuppress(VpnState state)
        {
            return _reconnectPending &&
                   (state.Status == VpnStatus.Disconnecting ||
                    state.Status == VpnStatus.Disconnected);
        }

        private bool ShouldBeReconnecting(VpnState state)
        {
            return _reconnecting && 
                   state.Status == VpnStatus.Connecting;
        }

        private void OnStateChanged(VpnState state)
        {
            if (state == null)
                return;

            StateChanged?.Invoke(this, new EventArgs<VpnState>(state));
        }
    }
}
