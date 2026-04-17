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
using ProtonVPN.Client.Logic.Connection.Contracts.Preferences;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Features.LocationExclusion;
using ProtonVPN.Client.Settings.Contracts.Enums;

namespace ProtonVPN.Client.Services.LocationExclusion;

public class ExcludeLocationsManager : IExcludeLocationsManager
{
    private readonly IServersLoader _serversLoader;
    private readonly ILocalizationProvider _localizer;
    private readonly IExclusionChecker _exclusionChecker;

    public ExcludeLocationsManager(
        IServersLoader serversLoader,
        ILocalizationProvider localizer,
        IExclusionChecker exclusionChecker)
    {
        _serversLoader = serversLoader;
        _localizer = localizer;
        _exclusionChecker = exclusionChecker;
    }

    public List<IExcludableLocation> GetExcludableLocations()
    {
        ExcludableLocationGroup countriesGroup = new(_localizer, ExcludedLocationType.Country);
        ExcludableLocationGroup statesGroup = new(_localizer, ExcludedLocationType.State);
        ExcludableLocationGroup citiesGroup = new(_localizer, ExcludedLocationType.City);

        List<IExcludableLocation> excludableLocations =
        [
            countriesGroup,
            statesGroup,
            citiesGroup
        ];

        IEnumerable<Country> countries = _serversLoader.GetCountries();
        foreach (Country country in countries)
        {
            bool isCountryExcluded = _exclusionChecker.IsCountryExcluded(country);
            ExcludableCountryItem countryItem = new(_localizer, country, isCountryExcluded);

            countriesGroup.AddLocation(countryItem);
            excludableLocations.Add(countryItem);

            IEnumerable<State> states = _serversLoader.GetStatesByCountryCode(country.Code);
            if (states.Any())
            {
                foreach (State state in states)
                {
                    bool isStateExcluded = _exclusionChecker.IsStateExcluded(state);
                    ExcludableStateItem stateItem = new ExcludableStateItem(_localizer, countryItem, state, isStateExcluded);

                    countryItem.AddChild(stateItem);
                    statesGroup.AddLocation(stateItem);
                    excludableLocations.Add(stateItem);
                }

                continue;
            }

            IEnumerable<City> cities = _serversLoader.GetCitiesByCountryCode(country.Code);
            foreach (City city in cities)
            {
                bool isCityExcluded = _exclusionChecker.IsCityExcluded(city);
                ExcludableCityItem cityItem = new ExcludableCityItem(_localizer, countryItem, city, isCityExcluded);

                countryItem.AddChild(cityItem);
                citiesGroup.AddLocation(cityItem);
                excludableLocations.Add(cityItem);
            }
        }

        return excludableLocations;
    }
}

