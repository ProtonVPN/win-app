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

public class OpenVpnAdapterMapper : IMapper<OpenVpnAdapter, OpenVpnAdapterIpcEntity>
{
    public OpenVpnAdapterIpcEntity Map(OpenVpnAdapter leftEntity)
    {
        return leftEntity switch
        {
            OpenVpnAdapter.Tap => OpenVpnAdapterIpcEntity.Tap,
            OpenVpnAdapter.Tun => OpenVpnAdapterIpcEntity.Tun,
            _ => throw new NotImplementedException("OpenVpnAdapter has an unknown value.")
        };
    }

    public OpenVpnAdapter Map(OpenVpnAdapterIpcEntity rightEntity)
    {
        return rightEntity switch
        {
            OpenVpnAdapterIpcEntity.Tap => OpenVpnAdapter.Tap,
            OpenVpnAdapterIpcEntity.Tun => OpenVpnAdapter.Tun,
            _ => throw new NotImplementedException("OpenVpnAdapter has an unknown value.")
        };
    }
}