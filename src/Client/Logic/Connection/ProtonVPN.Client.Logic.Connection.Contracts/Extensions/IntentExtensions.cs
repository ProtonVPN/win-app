/*
 * Copyright (c) 2023 Proton AG
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

using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Common.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Extensions;

public static class IntentExtensions
{
    public static string? GetCountryCode(this ILocationIntent locationIntent)
    {
        return locationIntent switch
        {
            CountryLocationIntent countryIntent => countryIntent.CountryCode,
            GatewayServerLocationIntent b2bServerIntent => b2bServerIntent.CountryCode,
            _ => string.Empty
        };
    }

    public static int GetServerNumber(this ServerLocationIntent serverIntent)
    {
        return serverIntent.Name.GetServerNumber();
    }

    public static int GetServerNumber(this GatewayServerLocationIntent serverIntent)
    {
        return serverIntent.Name.GetServerNumber();
    }

    public static FlagType GetFlagType(this ILocationIntent? locationIntent, bool isConnected = false)
    {
        return locationIntent switch
        {
            GatewayLocationIntent gatewayIntent => FlagType.Gateway,
            CountryLocationIntent countryIntent when countryIntent.IsSpecificCountry => FlagType.Country,
            CountryLocationIntent countryIntent when countryIntent.IsGenericRandomIntent() => FlagType.Random,
            FreeServerLocationIntent freeServerIntent when freeServerIntent.IsGenericRandomIntent() => isConnected ? FlagType.Country : FlagType.Random,
            _ => FlagType.Fastest,
        };
    }

    public static FlagType GetFlagType(this IConnectionIntent? connectionIntent)
    {
        return GetFlagType(connectionIntent?.Location);
    }
}