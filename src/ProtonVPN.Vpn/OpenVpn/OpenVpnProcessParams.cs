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

using ProtonVPN.Vpn.Common;
using System.Collections.Generic;
using ProtonVPN.Common;

namespace ProtonVPN.Vpn.OpenVpn
{
    public class OpenVpnProcessParams
    {
        public OpenVpnProcessParams(
            VpnEndpoint endpoint,
            int managementPort,
            string password,
            IReadOnlyCollection<string> customDns,
            SplitTunnelMode splitTunnelMode,
            IReadOnlyCollection<string> splitTunnelIPs,
            bool useTunAdapter,
            string interfaceGuid)
        {
            Endpoint = endpoint;
            ManagementPort = managementPort;
            Password = password;
            CustomDns = customDns;
            SplitTunnelMode = splitTunnelMode;
            SplitTunnelIPs = splitTunnelIPs;
            UseTunAdapter = useTunAdapter;
            InterfaceGuid = interfaceGuid;
        }

        public VpnEndpoint Endpoint { get; }

        public int ManagementPort { get; }

        public string Password { get; }

        public IReadOnlyCollection<string> CustomDns { get; }

        public SplitTunnelMode SplitTunnelMode { get; }

        public IReadOnlyCollection<string> SplitTunnelIPs { get; }

        public bool UseTunAdapter { get; }

        public string InterfaceGuid { get; }
    }
}