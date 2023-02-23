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

using System.Collections.Generic;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;

namespace ProtonVPN.Core.Vpn
{
    public class VpnConnectionRequest
    {
        public VpnConnectionRequest(
            IReadOnlyList<VpnHost> servers,
            VpnProtocol vpnProtocol,
            VpnConfig config,
            VpnCredentials credentials)
        {
            Servers = servers;
            VpnProtocol = vpnProtocol;
            Config = config;
            Credentials = credentials;
        }

        public IReadOnlyList<VpnHost> Servers { get; }
        public VpnProtocol VpnProtocol { get; }
        public VpnCredentials Credentials { get; }
        public VpnConfig Config { get; }
    }
}