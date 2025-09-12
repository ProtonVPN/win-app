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
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.States;

public class MultiStateLocationIntent : StateLocationIntentBase, IMultiLocationIntent
{
    public static MultiStateLocationIntent From(string countryCode, IEnumerable<string> stateNames, SelectionStrategy strategy)
        => new(SingleCountryLocationIntent.From(countryCode), stateNames, strategy);

    public IReadOnlyList<string> StateNames { get; }

    public SelectionStrategy Strategy { get; }

    public bool IsSelectionEmpty => StateNames.Count == 0;

    public MultiStateLocationIntent(
        SingleCountryLocationIntent country,
        IEnumerable<string> stateNames,
        SelectionStrategy strategy = SelectionStrategy.Fastest)
        : base(country)
    {
        StateNames = stateNames.Distinct().OrderBy(c => c).ToList();
        Strategy = strategy;
    }

    public MultiStateLocationIntent(
        SingleCountryLocationIntent country,
        SelectionStrategy strategy = SelectionStrategy.Fastest)
        : this(country, [], strategy)
    { }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is MultiStateLocationIntent multiStateIntent
            && Strategy == multiStateIntent.Strategy
            && StateNames.SequenceEqual(multiStateIntent.StateNames, StringComparer.OrdinalIgnoreCase);
    }

    public override bool IsSupported(Server server)
    {
        return base.IsSupported(server)
            && (IsSelectionEmpty || StateNames.Contains(server.State, StringComparer.OrdinalIgnoreCase));
    }

    public override string ToString()
    {
        return $"{base.ToString()} - {Strategy} state{(IsSelectionEmpty ? string.Empty : $" in {string.Join(", ", StateNames)}")}";
    }
}