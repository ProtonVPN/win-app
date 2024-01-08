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

using ProtonVPN.Common.Legacy;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

public class SplitTunnelModeMapper : IMapper<SplitTunnelMode, SplitTunnelModeIpcEntity>
{
    public SplitTunnelModeIpcEntity Map(SplitTunnelMode leftEntity)
    {
        return leftEntity switch
        {
            SplitTunnelMode.Disabled => SplitTunnelModeIpcEntity.Disabled,
            SplitTunnelMode.Block => SplitTunnelModeIpcEntity.Block,
            SplitTunnelMode.Permit => SplitTunnelModeIpcEntity.Permit,
            _ => throw new NotImplementedException("SplitTunnelMode has an unknown value.")
        };
    }

    public SplitTunnelMode Map(SplitTunnelModeIpcEntity rightEntity)
    {
        return rightEntity switch
        {
            SplitTunnelModeIpcEntity.Disabled => SplitTunnelMode.Disabled,
            SplitTunnelModeIpcEntity.Block => SplitTunnelMode.Block,
            SplitTunnelModeIpcEntity.Permit => SplitTunnelMode.Permit,
            _ => throw new NotImplementedException("SplitTunnelMode has an unknown value.")
        };
    }
}