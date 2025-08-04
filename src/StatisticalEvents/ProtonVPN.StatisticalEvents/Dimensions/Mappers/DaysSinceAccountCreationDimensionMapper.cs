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

using System;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers;

public class DaysSinceAccountCreationDimensionMapper : DimensionMapperBase, IDaysSinceAccountCreationDimensionMapper
{
    private const string ZERO_DAY = "0";
    private const string ONE_TO_THREE_DAYS = "1-3";
    private const string FOUR_TO_SEVEN_DAYS = "4-7";
    private const string EIGHT_TO_FOURTEEN_DAYS = "8-14";
    private const string MORE_THAN_FOURTEEN_DAYS = ">14";

    public string Map(DateTimeOffset? accountCreationDateUtc)
    {
        if (accountCreationDateUtc is null)
        {
            return NOT_AVAILABLE;
        }

        long daysSinceAccountCreation = (long)(DateTime.UtcNow - accountCreationDateUtc.Value).TotalDays;
        return daysSinceAccountCreation switch
        {
            <= 0 => ZERO_DAY,
            <= 3 => ONE_TO_THREE_DAYS,
            <= 7 => FOUR_TO_SEVEN_DAYS,
            <= 14 => EIGHT_TO_FOURTEEN_DAYS,
            _ => MORE_THAN_FOURTEEN_DAYS
        };
    }
}