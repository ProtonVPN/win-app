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
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.Connection
{
    /// <summary>
    /// Logs state changed events.
    /// A wrapper around <see cref="ISingleVpnConnection"/>.
    /// </summary>
    internal class LoggingWrapper : IVpnConnection
    {
        private readonly ILogger _logger;
        private readonly IVpnConnection _origin;

        public LoggingWrapper(
            ILogger logger,
            IVpnConnection origin)
        {
            _logger = logger;
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
            _origin.Connect(servers, config, credentials);
        }

        public void ResetConnection()
        {
            _origin.ResetConnection();
        }

        public void Disconnect(VpnError error)
        {
            _origin.Disconnect(error);
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

        private void Origin_StateChanged(object sender, EventArgs<VpnState> e)
        {
            VpnState state = e.Data;
            _logger.Info<ConnectionStateChangeLog>($"VPN state changed: {state.Status}, Error: {state.Error}, " +
                         $"LocalIP: {state.LocalIp}, RemoteIP: {state.RemoteIp}, Label: {state.Label}");

            OnStateChanged(state);
        }

        private void OnStateChanged(VpnState state)
        {
            StateChanged?.Invoke(this, new EventArgs<VpnState>(state));
        }
    }
}