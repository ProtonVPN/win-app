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
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers;

public class VpnProtocolDimensionMapper : DimensionMapperBase, IVpnProtocolDimensionMapper
{
    private const string SMART = "smart"; // Should never be used
    private const string IKEV2 = "ikev2"; // Not used on Windows
    private const string OPENVPN_UDP = "openvpn_udp";
    private const string OPENVPN_TCP = "openvpn_tcp";
    private const string WIREGUARD_UDP = "wireguard_udp";
    private const string WIREGUARD_TCP = "wireguard_tcp";
    private const string WIREGUARD_TLS = "wireguard_tls";

    public string Map(VpnProtocol? protocol)
    {
        return protocol switch
        {
            VpnProtocol.Smart => SMART, 
            VpnProtocol.OpenVpnUdp => OPENVPN_UDP,
            VpnProtocol.OpenVpnTcp => OPENVPN_TCP,
            VpnProtocol.WireGuardUdp => WIREGUARD_UDP,
            VpnProtocol.WireGuardTcp => WIREGUARD_TCP,
            VpnProtocol.WireGuardTls => WIREGUARD_TLS,
            _ => NOT_AVAILABLE
        };
    }
}