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
using ProtonVPN.Common;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.Vpn
{
    internal class VpnConnectionEntry : IVpnConnection
    {
        private readonly IVpnConnection _openVpnConnection;
        private readonly IVpnConnection _wireguardConnection;

        private IReadOnlyList<VpnHost> _servers;
        private VpnConfig _config;
        private VpnCredentials _credentials;

        private VpnState _lastState = new(VpnStatus.Disconnected, default);
        private bool _pendingConnect;

        public VpnConnectionEntry(IVpnConnection openVpnConnection, IVpnConnection wireguardConnection)
        {
            _openVpnConnection = openVpnConnection;
            _wireguardConnection = wireguardConnection;
            openVpnConnection.StateChanged += (_, e) =>
            {
                InvokeStateChange(e);
            };

            wireguardConnection.StateChanged += (_, e) =>
            {
                InvokeStateChange(e);
            };
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged;

        public InOutBytes Total => _config != null ? VpnConnection.Total : new InOutBytes(0, 0);

        public void Connect(IReadOnlyList<VpnHost> servers, VpnConfig config, VpnCredentials credentials)
        {
            if (_lastState.Status == VpnStatus.Connected && _lastState.VpnProtocol != config.VpnProtocol)
            {
                IVpnConnection previousConnection = VpnConnection;
                _pendingConnect = true;
                _servers = servers;
                _config = config;
                _credentials = credentials;
                previousConnection.Disconnect();
                return;
            }

            _config = config;
            VpnConnection.Connect(servers, config, credentials);
        }

        public void Disconnect(VpnError error = VpnError.None)
        {
            if (_config != null)
            {
                VpnConnection.Disconnect(error);
            }
        }

        public void UpdateServers(IReadOnlyList<VpnHost> servers)
        {
            if (_config != null)
            {
                VpnConnection.UpdateServers(servers);
            }
        }

        public void UpdateAuthCertificate(string certificate)
        {
            if (_config != null)
            {
                VpnConnection.UpdateAuthCertificate(certificate);
            }
        }

        public void SetFeatures(VpnFeatures vpnFeatures)
        {
            if (_config != null)
            {
                VpnConnection.SetFeatures(vpnFeatures);
            }
        }

        private void InvokeStateChange(EventArgs<VpnState> e)
        {
            _lastState = e.Data;

            if (_pendingConnect)
            {
                if (e.Data.Status == VpnStatus.Disconnected)
                {
                    _pendingConnect = false;
                    Connect(_servers, _config, _credentials);
                }

                return;
            }

            StateChanged?.Invoke(this, e);
        }

        private IVpnConnection VpnConnection =>
            _config.VpnProtocol == VpnProtocol.WireGuard ? _wireguardConnection : _openVpnConnection;
    }
}