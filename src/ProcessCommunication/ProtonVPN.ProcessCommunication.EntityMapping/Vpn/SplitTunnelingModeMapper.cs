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

using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

public class SplitTunnelingModeMapper : IMapper<SplitTunnelingMode, SplitTunnelModeIpcEntity>
{
    public SplitTunnelModeIpcEntity Map(SplitTunnelingMode leftEntity)
    {
        return leftEntity switch
        {
            SplitTunnelingMode.Standard => SplitTunnelModeIpcEntity.Block,
            SplitTunnelingMode.Inverse => SplitTunnelModeIpcEntity.Permit,
            _ => throw new NotImplementedException("SplitTunnelMode has an unknown value.")
        };
    }

    public SplitTunnelingMode Map(SplitTunnelModeIpcEntity rightEntity)
    {
        return rightEntity switch
        {
            SplitTunnelModeIpcEntity.Disabled => throw new InvalidOperationException("Disabled split tunnel is now handled apart by a boolean flag"),
            SplitTunnelModeIpcEntity.Block => SplitTunnelingMode.Standard,
            SplitTunnelModeIpcEntity.Permit => SplitTunnelingMode.Inverse,
            _ => throw new NotImplementedException("SplitTunnelMode has an unknown value.")
        };
    }
}