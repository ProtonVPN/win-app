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
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.Vpn
{
    internal class ObservableConnection : IVpnConnection
    {
        private readonly IVpnConnection _origin;

        public ObservableConnection(IVpnConnection origin)
        {
            _origin = origin;

            _origin.StateChanged += Origin_StateChanged;
        }

        public event EventHandler<EventArgs<VpnState>> BeforeStateChanged;
        public event EventHandler<EventArgs<VpnState>> StateChanged;
        public event EventHandler<EventArgs<VpnState>> AfterStateChanged;

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
            BeforeStateChanged?.Invoke(this, e);
            StateChanged?.Invoke(this, e);
            AfterStateChanged?.Invoke(this, e);
        }
    }
}