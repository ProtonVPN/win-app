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

using ProtonVPN.Common.Core.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

public class VpnServerAddressMapper : IMapper<IpAddressInfo, VpnServerAddressIpcEntity>
{
    public VpnServerAddressIpcEntity Map(IpAddressInfo leftEntity)
    {
        return new()
        {
            Ipv4Address = leftEntity.Ipv4Address,
            Ipv6Address = leftEntity.Ipv6Address,
        };
    }

    public IpAddressInfo Map(VpnServerAddressIpcEntity rightEntity)
    {
        return new()
        {
            Ipv4Address = rightEntity.Ipv4Address,
            Ipv6Address = rightEntity.Ipv6Address,
        };
    }
}