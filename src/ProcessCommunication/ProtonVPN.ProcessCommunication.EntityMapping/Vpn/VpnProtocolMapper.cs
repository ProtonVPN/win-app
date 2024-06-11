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

using ProtonVPN.Common.Core.Networking;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

public class VpnProtocolMapper : IMapper<VpnProtocol, VpnProtocolIpcEntity>
{
    public VpnProtocolIpcEntity Map(VpnProtocol leftEntity)
    {
        return leftEntity switch
        {
            VpnProtocol.OpenVpnUdp => VpnProtocolIpcEntity.OpenVpnUdp,
            VpnProtocol.OpenVpnTcp => VpnProtocolIpcEntity.OpenVpnTcp,
            VpnProtocol.WireGuardUdp => VpnProtocolIpcEntity.WireGuardUdp,
            VpnProtocol.WireGuardTcp => VpnProtocolIpcEntity.WireGuardTcp,
            VpnProtocol.WireGuardTls => VpnProtocolIpcEntity.WireGuardTls,
            VpnProtocol.Smart => VpnProtocolIpcEntity.Smart,
            _ => throw new NotImplementedException("VpnProtocol has an unknown value.")
        };
    }

    public VpnProtocol Map(VpnProtocolIpcEntity rightEntity)
    {
        return rightEntity switch
        {
            VpnProtocolIpcEntity.OpenVpnTcp => VpnProtocol.OpenVpnTcp,
            VpnProtocolIpcEntity.OpenVpnUdp => VpnProtocol.OpenVpnUdp,
            VpnProtocolIpcEntity.WireGuardUdp => VpnProtocol.WireGuardUdp,
            VpnProtocolIpcEntity.WireGuardTcp => VpnProtocol.WireGuardTcp,
            VpnProtocolIpcEntity.WireGuardTls => VpnProtocol.WireGuardTls,
            VpnProtocolIpcEntity.Smart => VpnProtocol.Smart,
            _ => throw new NotImplementedException("VpnProtocol has an unknown value."),
        };
    }
}