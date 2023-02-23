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
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.OS.Net.NetworkInterface;

namespace ProtonVPN.Common.OS.Net
{
    public class NetworkInterfaceLoader : INetworkInterfaceLoader
    {
        private readonly IConfiguration _config;
        private readonly INetworkInterfaces _networkInterfaces;

        public NetworkInterfaceLoader(IConfiguration config, INetworkInterfaces networkInterfaces)
        {
            _networkInterfaces = networkInterfaces;
            _config = config;
        }

        public INetworkInterface GetOpenVpnTapInterface()
        {
            return _networkInterfaces.GetByDescription(_config.OpenVpn.TapAdapterDescription);
        }

        public INetworkInterface GetOpenVpnTunInterface()
        {
            return _networkInterfaces.GetByName(_config.OpenVpn.TunAdapterName);
        }

        public INetworkInterface GetWireGuardTunInterface()
        {
            INetworkInterface networkInterface = _networkInterfaces.GetById(Guid.Parse(_config.WireGuard.TunAdapterGuid));
            return networkInterface ?? _networkInterfaces.GetByName(_config.WireGuard.TunAdapterName);
        }

        public INetworkInterface GetByVpnProtocol(VpnProtocol vpnProtocol, OpenVpnAdapter? openVpnAdapter)
        {
            return vpnProtocol == VpnProtocol.WireGuard
                ? GetWireGuardTunInterface()
                : GetByOpenVpnAdapter(openVpnAdapter);
        }

        public INetworkInterface GetByOpenVpnAdapter(OpenVpnAdapter? openVpnAdapter)
        {
            return openVpnAdapter switch
            {
                OpenVpnAdapter.Tap => GetOpenVpnTapInterface(),
                OpenVpnAdapter.Tun => GetOpenVpnTunInterface(),
                _ => new NullNetworkInterface()
            };
        }
    }
}