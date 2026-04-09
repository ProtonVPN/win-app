/*
 * Copyright (c) 2026 Proton AG
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
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers.Settings;

public class KillSwitchModeDimensionMapper : DimensionMapperBase, IKillSwitchModeDimensionMapper
{
    private const string KILL_SWITCH_OFF = "off";
    private const string KILL_SWITCH_STANDARD = "standard";
    private const string KILL_SWITCH_ADVANCED = "advanced";

    public string Map(bool isKillSwitchEnabled, KillSwitchMode killSwitchMode)
    {
        if (!isKillSwitchEnabled)
        {
            return KILL_SWITCH_OFF;
        }

        return killSwitchMode switch
        {
            KillSwitchMode.Standard => KILL_SWITCH_STANDARD,
            KillSwitchMode.Advanced => KILL_SWITCH_ADVANCED,
            _ => NOT_AVAILABLE
        };
    }
}
