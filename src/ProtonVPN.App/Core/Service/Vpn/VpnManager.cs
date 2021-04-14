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
using System.Threading.Tasks;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnManager : IVpnManager
    {
        private readonly ITaskQueue _taskQueue = new SerialTaskQueue();
        private readonly IVpnServiceManager _vpnServiceManager;
        private readonly IVpnReconnector _vpnReconnector;
        private readonly IVpnConnector _vpnConnector; 

        public VpnManager(
            IVpnServiceManager vpnServiceManager,
            IVpnReconnector vpnReconnector,
            IVpnConnector vpnConnector)
        {
            _vpnServiceManager = vpnServiceManager;
            _vpnReconnector = vpnReconnector;
            _vpnConnector = vpnConnector;
            _vpnConnector.VpnStateChanged += OnConnectorVpnStateChanged;
        }

        public event EventHandler<VpnStateChangedEventArgs> VpnStateChanged;

        private void OnConnectorVpnStateChanged(object sender, VpnStateChangedEventArgs e)
        {
            VpnStateChanged?.Invoke(this, e);
        }

        public void OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnReconnector.OnVpnStateChanged(e);
            _vpnConnector.OnVpnStateChanged(e);
        }

        public async Task ConnectAsync(Profile profile, Profile fallbackProfile = null)
        {
            await Queued(() => _vpnConnector.ConnectToBestProfileAsync(profile, fallbackProfile));
        }

        public async Task QuickConnectAsync()
        {
            await Queued(() => _vpnConnector.QuickConnectAsync());
        }

        public async Task ReconnectAsync(VpnReconnectionSettings settings = null)
        {
            await Queued(() => _vpnReconnector.ReconnectAsync(
                _vpnConnector.LastServer, _vpnConnector.LastProfile, settings));
        }

        public async Task DisconnectAsync(VpnError vpnError = VpnError.None)
        {
            _vpnReconnector.OnDisconnectionRequest();
            await Queued(() => _vpnServiceManager.Disconnect(vpnError));
        }

        public async Task GetStateAsync()
        {
            await _vpnServiceManager.RepeatState();
        }

        private Task Queued(Func<Task> function)
        {
            return _taskQueue.Enqueue(function);
        }
    }
}
