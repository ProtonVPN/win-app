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

public class KillSwitchModeMapper : IMapper<KillSwitchMode, KillSwitchModeIpcEntity>
{
    public KillSwitchModeIpcEntity Map(KillSwitchMode leftEntity)
    {
        return leftEntity switch
        {
            KillSwitchMode.Standard => KillSwitchModeIpcEntity.Soft,
            KillSwitchMode.Advanced => KillSwitchModeIpcEntity.Hard,
            _ => throw new NotImplementedException("KillSwitchMode has an unknown value.")
        };
    }

    public KillSwitchMode Map(KillSwitchModeIpcEntity rightEntity)
    {
        return rightEntity switch
        {
            KillSwitchModeIpcEntity.Off => throw new InvalidOperationException("Disabled kill switch is now handled apart by a boolean flag"),
            KillSwitchModeIpcEntity.Soft => KillSwitchMode.Standard,
            KillSwitchModeIpcEntity.Hard => KillSwitchMode.Advanced,
            _ => throw new NotImplementedException("KillSwitchMode has an unknown value.")
        };
    }
}