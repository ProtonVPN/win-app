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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Cities;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Servers;

public class MultiServerLocationIntent : ServerLocationIntentBase, IMultiLocationIntent
{
    public static MultiServerLocationIntent From(string countryCode, string? stateName, string? cityName, IEnumerable<ServerInfo> servers, SelectionStrategy strategy)
        => string.IsNullOrEmpty(cityName)
            ? From(countryCode, servers, strategy)
            : new(SingleCityLocationIntent.From(countryCode, stateName, cityName), servers, strategy);

    public static MultiServerLocationIntent From(string countryCode, string? cityName, IEnumerable<ServerInfo> servers, SelectionStrategy strategy)
        => string.IsNullOrEmpty(cityName)
            ? From(countryCode, servers, strategy)
            : new(SingleCityLocationIntent.From(countryCode, cityName), servers, strategy);

    public static MultiServerLocationIntent From(string countryCode, IEnumerable<ServerInfo> servers, SelectionStrategy strategy)
        => new(SingleCountryLocationIntent.From(countryCode), servers, strategy);

    public IReadOnlyList<ServerInfo> Servers { get; }

    public SelectionStrategy Strategy { get; }

    public bool IsSelectionEmpty => Servers.Count == 0;

    public MultiServerLocationIntent(
        SingleCityLocationIntent city,
        IEnumerable<ServerInfo> servers,
        SelectionStrategy strategy = SelectionStrategy.Fastest)
        : base(city)
    {
        Servers = servers.Distinct().OrderBy(s => s.Id).ToList();
        Strategy = strategy;
    }

    public MultiServerLocationIntent(
        SingleCityLocationIntent city,
        SelectionStrategy strategy = SelectionStrategy.Fastest)
        : this(city, [], strategy)
    { }

    public MultiServerLocationIntent(
        SingleCountryLocationIntent country,
        IEnumerable<ServerInfo> servers,
        SelectionStrategy strategy = SelectionStrategy.Fastest)
        : base(country)
    {
        Servers = servers.Distinct().OrderBy(s => s.Id).ToList();
        Strategy = strategy;
    }

    public MultiServerLocationIntent(
        SingleCountryLocationIntent country,
        SelectionStrategy strategy = SelectionStrategy.Fastest)
        : this(country, [], strategy)
    { }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is MultiServerLocationIntent multiServerIntent
            && Strategy == multiServerIntent.Strategy
            && Servers.SequenceEqual(multiServerIntent.Servers);
    }

    public override bool IsSupported(Server server)
    {
        return base.IsSupported(server)
            && (IsSelectionEmpty || Servers.Any(s => s.Id == server.Id));
    }

    public override string ToString()
    {
        return $"{base.ToString()} - {Strategy} server{(IsSelectionEmpty ? string.Empty : $" in {string.Join(", ", Servers.Select(s => s.Name))}")}";
    }
}