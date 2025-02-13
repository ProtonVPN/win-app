/*
 * Copyright (c) 2025 Proton AG
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

using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.StatisticalEvents.DimensionMapping;

public class VpnProtocolMapper : DimensionMapperBase, IDimensionMapper<VpnProtocol?>
{
    public string Map(VpnProtocol? protocol)
    {
        return protocol switch
        {
            VpnProtocol.Smart => "smart", // Should never be used
            VpnProtocol.OpenVpnUdp => "openvpn_udp",
            VpnProtocol.OpenVpnTcp => "openvpn_tcp",
            VpnProtocol.WireGuardUdp => "wireguard_udp",
            VpnProtocol.WireGuardTcp => "wireguard_tcp",
            VpnProtocol.WireGuardTls => "wireguard_tls",
            _ => NOT_AVAILABLE
        };
    }
}