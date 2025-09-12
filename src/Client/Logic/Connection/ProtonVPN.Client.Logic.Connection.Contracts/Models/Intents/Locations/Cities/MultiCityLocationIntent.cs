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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.States;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Cities;

public class MultiCityLocationIntent : CityLocationIntentBase, IMultiLocationIntent
{
    public static MultiCityLocationIntent From(string countryCode, string stateName, IEnumerable<string> cityNames, SelectionStrategy strategy)
        => string.IsNullOrEmpty(stateName)
            ? From(countryCode, cityNames, strategy)
            : new(SingleStateLocationIntent.From(countryCode, stateName), cityNames, strategy);

    public static MultiCityLocationIntent From(string countryCode, IEnumerable<string> cityNames, SelectionStrategy strategy)
        => new(SingleCountryLocationIntent.From(countryCode), cityNames, strategy);

    public IReadOnlyList<string> CityNames { get; }

    public SelectionStrategy Strategy { get; }

    public bool IsSelectionEmpty => CityNames.Count == 0;

    public MultiCityLocationIntent(
        SingleCountryLocationIntent country,
        IEnumerable<string> cityNames,
        SelectionStrategy strategy = SelectionStrategy.Fastest)
        : base(country)
    {
        CityNames = cityNames.Distinct().OrderBy(c => c).ToList();
        Strategy = strategy;
    }

    public MultiCityLocationIntent(
        SingleCountryLocationIntent country,
        SelectionStrategy strategy = SelectionStrategy.Fastest)
        : this(country, [], strategy)
    { }

    public MultiCityLocationIntent(
        SingleStateLocationIntent state,
        IEnumerable<string> cityNames,
        SelectionStrategy strategy = SelectionStrategy.Fastest)
        : base(state)
    {
        CityNames = cityNames.Distinct().OrderBy(c => c).ToList();
        Strategy = strategy;
    }

    public MultiCityLocationIntent(
        SingleStateLocationIntent state,
        SelectionStrategy strategy = SelectionStrategy.Fastest)
        : this(state, [], strategy)
    { }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is MultiCityLocationIntent multiCityIntent
            && Strategy == multiCityIntent.Strategy
            && CityNames.SequenceEqual(multiCityIntent.CityNames, StringComparer.OrdinalIgnoreCase);
    }

    public override bool IsSupported(Server server)
    {
        return base.IsSupported(server)
            && (IsSelectionEmpty || CityNames.Contains(server.City, StringComparer.OrdinalIgnoreCase));
    }

    public override string ToString()
    {
        return $"{base.ToString()} - {Strategy} City{(IsSelectionEmpty ? string.Empty : $" in {string.Join(", ", CityNames)}")}";
    }
}