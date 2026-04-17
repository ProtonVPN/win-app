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

using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;

namespace ProtonVPN.Client.Models.Features.LocationExclusion;

public partial class ExcludableCityItem : ExcludableChildItemBase<City>
{
    public ExcludableCityItem(
        ILocalizationProvider localizer,
        ExcludableCountryItem parent,
        City city,
        bool isExcluded)
        : base(localizer, parent, ExcludedLocationType.City, city, isExcluded)
    {
        DisplayName = localizer.GetCityName(city.Name, city.CountryCode);
    }

    public override ExcludedLocation ToExcludedLocation()
    {
        return new ExcludedLocation(Location.CountryCode, Location.StateName, Location.Name);
    }
}