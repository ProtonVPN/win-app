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
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;

public class MultiCountryLocationIntent : CountryLocationIntentBase, IMultiLocationIntent
{
    public static MultiCountryLocationIntent Default => Fastest;
    public static MultiCountryLocationIntent Fastest => new(SelectionStrategy.Fastest);
    public static MultiCountryLocationIntent Random => new(SelectionStrategy.Random);
    public static MultiCountryLocationIntent FastestExcludingMyCountry => ExcludingMyCountry(SelectionStrategy.Fastest);
    public static MultiCountryLocationIntent RandomExcludingMyCountry => ExcludingMyCountry(SelectionStrategy.Random);
    public static MultiCountryLocationIntent FastestFrom(IEnumerable<string> countryCodes) => From(countryCodes, SelectionStrategy.Fastest);
    public static MultiCountryLocationIntent RandomFrom(IEnumerable<string> countryCodes) => From(countryCodes, SelectionStrategy.Random);
    public static MultiCountryLocationIntent From(SelectionStrategy strategy) => new(strategy);
    public static MultiCountryLocationIntent From(IEnumerable<string> countryCodes, SelectionStrategy strategy) => new(countryCodes, strategy);
    public static MultiCountryLocationIntent From(SelectionStrategy strategy, bool isToExcludeMyCountry) => new(strategy, isToExcludeMyCountry);
    public static MultiCountryLocationIntent ExcludingMyCountry(SelectionStrategy strategy) => new(strategy, true);

    public IReadOnlyList<string> CountryCodes { get; }

    public bool IsToExcludeMyCountry { get; }

    public SelectionStrategy Strategy { get; }

    public bool IsSelectionEmpty => CountryCodes.Count == 0;

    public MultiCountryLocationIntent(
        IEnumerable<string> countryCodes,
        SelectionStrategy strategy = SelectionStrategy.Fastest)
    {
        CountryCodes = countryCodes.Distinct().OrderBy(c => c).ToList();
        Strategy = strategy;
    }

    public MultiCountryLocationIntent(
        SelectionStrategy strategy = SelectionStrategy.Fastest,
        bool isToExcludeMyCountry = false)
        : this([], strategy)
    {
        IsToExcludeMyCountry = isToExcludeMyCountry;
    }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is MultiCountryLocationIntent multiCountryIntent
            && Strategy == multiCountryIntent.Strategy
            && IsToExcludeMyCountry == multiCountryIntent.IsToExcludeMyCountry
            && CountryCodes.SequenceEqual(multiCountryIntent.CountryCodes, StringComparer.OrdinalIgnoreCase);
    }

    public override bool IsSupported(Server server)
    {
        return IsSelectionEmpty || CountryCodes.Contains(server.ExitCountry, StringComparer.OrdinalIgnoreCase);
    }

    public override string ToString()
    {
        string intent = $"{Strategy} country";

        if (!IsSelectionEmpty)
        {
            intent += $" in {string.Join(", ", CountryCodes)}";
        }

        if (IsToExcludeMyCountry)
        {
            intent += " (excluding my country)";
        }

        return intent;
    }
}