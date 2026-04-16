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

using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Core.Bases.Models;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;

namespace ProtonVPN.Client.Models.Features.LocationExclusion;

public class ExcludableLocationGroup : ModelBase, IExcludableLocation
{
    public ExcludedLocationType Type { get; }

    public bool IsExcluded { get; set; } = false;

    public bool IsLocationExcluded => Locations.Count > 0 && Locations.All(c => c.IsLocationExcluded);

    public string DisplayName { get; }

    public NotifyObservableCollection<IExcludableLocation> Locations { get; } = [];

    public string AbsoluteSortOrder => $"{(int)Type}";

    public string RelativeSortOrder { get; } = string.Empty;

    public ExcludedLocation ToExcludedLocation()
    {
        throw new InvalidOperationException("Cannot convert a location group to an excluded location.");
    }

    public ExcludableLocationGroup(
        ILocalizationProvider localizer,
        ExcludedLocationType type)
        : base(localizer)
    {

        Type = type;
        DisplayName = type switch
        {
            ExcludedLocationType.Country => localizer.Get("Settings_Connection_ExcludedLocations_Countries"),
            ExcludedLocationType.State => localizer.Get("Settings_Connection_ExcludedLocations_States"),
            ExcludedLocationType.City => localizer.Get("Settings_Connection_ExcludedLocations_Cities"),
            _ => throw new InvalidOperationException($"Unknown location type: {type}")
        };
    }

    public void AddLocation(IExcludableLocation location)
    {
        Locations.Add(location);
    }
}