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

using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.Logic.Searches.Contracts;

public static class SearchMatcher
{
    public static string? NormalizeInput(this string? input)
    {
        return input?.Trim().RemoveDiacritics();
    }

    public static bool MatchesCountry(Country country, string localizedCountryName, string normalizedInput)
    {
        return Equals(country.Code, normalizedInput)
            || Matches(localizedCountryName, normalizedInput);
    }

    public static bool MatchesState(string localizedStateName, string normalizedInput)
    {
        return Matches(localizedStateName, normalizedInput);
    }

    public static bool MatchesCity(string localizedCityName, string normalizedInput)
    {
        return Matches(localizedCityName, normalizedInput);
    }

    public static bool MatchesServer(Server server, string normalizedInput)
    {
        return StartsWith(server.Name, normalizedInput)
            || StartsWith(server.Name.Replace("#", ""), normalizedInput);
    }

    public static bool Matches(string? candidateName, string normalizedInput)
    {
        return normalizedInput.Length < SearchConfiguration.MIN_CONTAINS_LENGTH
            ? StartsWith(candidateName, normalizedInput)
            : Contains(candidateName, normalizedInput);
    }

    public static bool Equals(string? candidateName, string normalizedInput)
    {
        return candidateName.NormalizeInput()?.Equals(normalizedInput, SearchConfiguration.STRING_COMPARISON) ?? false;
    }

    public static bool StartsWith(string? candidateName, string normalizedInput)
    {
        return candidateName.NormalizeInput()?.StartsWith(normalizedInput, SearchConfiguration.STRING_COMPARISON) ?? false;
    }

    public static bool Contains(string? candidateName, string normalizedInput)
    {
        return candidateName.NormalizeInput()?.Contains(normalizedInput, SearchConfiguration.STRING_COMPARISON) ?? false;
    }
}
