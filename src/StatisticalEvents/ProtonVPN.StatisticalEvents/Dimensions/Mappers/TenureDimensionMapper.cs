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

public class TenureDimensionMapper : DimensionMapperBase, ITenureDimensionMapper
{
    private const string UNKNOWN = "unknown";
    private const string DAY_0 = "0";
    private const string DAY_1 = "1";
    private const string DAY_2 = "2";
    private const string DAYS_3_TO_7 = "3-7";
    private const string DAYS_8_TO_30 = "8-30";
    private const string DAYS_31_TO_90 = "31-90";
    private const string DAYS_91_TO_365 = "91-365";
    private const string DAYS_366_AND_MORE = ">366";

    public string Map(DateTimeOffset? accountCreationDateUtc)
    {
        if (accountCreationDateUtc is null)
        {
            return UNKNOWN;
        }

        long daysSinceAccountCreation = (long)(DateTime.UtcNow - accountCreationDateUtc.Value).TotalDays;

        if (daysSinceAccountCreation <= 0)
        {
            return DAY_0;
        }

        return daysSinceAccountCreation switch
        {
            1 => DAY_1,
            2 => DAY_2,
            >= 3 and <= 7 => DAYS_3_TO_7,
            >= 8 and <= 30 => DAYS_8_TO_30,
            >= 31 and <= 90 => DAYS_31_TO_90,
            >= 91 and <= 365 => DAYS_91_TO_365,
            _ => DAYS_366_AND_MORE
        };
    }
}
