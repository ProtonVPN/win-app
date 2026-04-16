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

using System.Collections.Generic;
using System.Linq;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers.Settings;

public class ExcludedLocationsCountDimensionMapper : DimensionMapperBase, IExcludedLocationsCountDimensionMapper
{
    private const string ZERO = "0";
    private const string ONE = "1";
    private const string TWO_TO_FIVE = "2-5";
    private const string SIX_TO_TEN = "6-10";
    private const string ELEVEN_TO_TWENTY = "11-20";
    private const string MORE_THAN_TWENTY = ">=21";

    private string Map(int count)
    {
        return count switch
        {
            0 => ZERO,
            1 => ONE,
            <= 5 => TWO_TO_FIVE,
            <= 10 => SIX_TO_TEN,
            <= 20 => ELEVEN_TO_TWENTY,
            _ => MORE_THAN_TWENTY
        };
    }

    public string MapCountries(List<ExcludedLocation> locations)
    {
        int count = locations?.Count(l => l.Type == ExcludedLocationType.Country) ?? 0;
        return Map(count);
    }

    public string MapCitiesAndStates(List<ExcludedLocation> locations)
    {
        int count = locations?.Count(l => l.Type == ExcludedLocationType.City || l.Type == ExcludedLocationType.State) ?? 0;
        return Map(count);
    }
}