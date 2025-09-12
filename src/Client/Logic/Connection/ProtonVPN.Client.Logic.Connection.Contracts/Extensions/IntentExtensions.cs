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
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Cities;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.FreeServers;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Gateways;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.GatewayServers;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Servers;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.States;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Extensions;

public static class IntentExtensions
{
    public static string? GetCountryCode(this ILocationIntent locationIntent)
    {
        return locationIntent switch
        {
            SingleCountryLocationIntent countryIntent => countryIntent.CountryCode,
            CityLocationIntentBase cityIntent => cityIntent.Country.CountryCode,
            StateLocationIntentBase stateIntent => stateIntent.Country.CountryCode,
            ServerLocationIntentBase serverIntent => serverIntent.Country.CountryCode,
            SingleGatewayServerLocationIntent b2bServerIntent => b2bServerIntent.Server.CountryCode,
            _ => string.Empty
        };
    }

    public static FlagType GetFlagType(this ILocationIntent? locationIntent, bool isConnected = false)
    {
        return locationIntent switch
        {
            SingleCountryLocationIntent or
            CityLocationIntentBase or
            StateLocationIntentBase or
            ServerLocationIntentBase => FlagType.Country,

            SingleGatewayLocationIntent or
            GatewayServerLocationIntentBase => FlagType.Gateway,

            FreeServerLocationIntent freeServerIntent => freeServerIntent.Strategy switch
            {
                SelectionStrategy.Random when isConnected => FlagType.Country,
                SelectionStrategy.Random => FlagType.Random,
                _ => FlagType.Fastest,
            },

            IMultiLocationIntent multiLocationIntent => multiLocationIntent.Strategy switch
            {
                SelectionStrategy.Random => FlagType.Random,
                _ => FlagType.Fastest,
            },

            _ => FlagType.Fastest,
        };
    }

    public static FlagType GetFlagType(this IConnectionIntent? connectionIntent)
    {
        return GetFlagType(connectionIntent?.Location);
    }

    public static SelectionStrategy GetSelectionStrategy(this ILocationIntent locationIntent)
    {
        return locationIntent is IMultiLocationIntent multiLocationIntent
            ? multiLocationIntent.Strategy
            : SelectionStrategy.Fastest;
    }

    public static IEnumerable<ILocationIntent> GetIntentHierarchy(this ILocationIntent? locationIntent)
    {
        while (locationIntent != null)
        {
            yield return locationIntent;
            locationIntent = locationIntent.GetBaseLocationIntent();
        }
    }

    public static IEnumerable<IFeatureIntent> GetIntentHierarchy(this IFeatureIntent? featureIntent)
    {
        while (featureIntent != null)
        {
            yield return featureIntent;
            featureIntent = featureIntent.GetBaseFeatureIntent();
        }
    }

    public static ILocationIntent? GetBaseLocationIntent(this ILocationIntent locationIntent)
    {
        return locationIntent switch
        {
            // Server -> City -> (State) -> Single country -> Multi country
            ServerLocationIntentBase serverIntent => serverIntent.City,
            CityLocationIntentBase cityIntent when cityIntent.State != null => cityIntent.State,
            CityLocationIntentBase cityIntent => cityIntent.Country,
            StateLocationIntentBase stateIntent => stateIntent.Country,
            SingleCountryLocationIntent => MultiCountryLocationIntent.Default,

            // Multi country with selection -> Multi country (all available)
            MultiCountryLocationIntent countryIntent when !countryIntent.IsSelectionEmpty => countryIntent.Strategy switch
            {
                SelectionStrategy.Fastest => MultiCountryLocationIntent.Fastest,
                SelectionStrategy.Random => MultiCountryLocationIntent.Random,
                _ => MultiCountryLocationIntent.Default,
            },

            // Multi country -> Multi country excluding my country
            MultiCountryLocationIntent countryIntent when !countryIntent.IsToExcludeMyCountry => countryIntent.Strategy switch
            {
                SelectionStrategy.Fastest => MultiCountryLocationIntent.FastestExcludingMyCountry,
                SelectionStrategy.Random => MultiCountryLocationIntent.RandomExcludingMyCountry,
                _ => MultiCountryLocationIntent.Default,
            },

            // Fastest country -> Random country
            MultiCountryLocationIntent countryIntent when countryIntent.Strategy == SelectionStrategy.Fastest => MultiCountryLocationIntent.Random,

            // Gateway server -> Single gateway -> Multi gateway (all available)
            GatewayServerLocationIntentBase gatewayServerIntent => gatewayServerIntent.Gateway,
            SingleGatewayLocationIntent => MultiGatewayLocationIntent.Default,

            // Multi gateway with selection -> Multi gateway (all available)
            MultiGatewayLocationIntent gatewayIntent when !gatewayIntent.IsSelectionEmpty => gatewayIntent.Strategy switch
            {
                SelectionStrategy.Fastest => MultiGatewayLocationIntent.Fastest,
                SelectionStrategy.Random => MultiGatewayLocationIntent.Random,
                _ => MultiGatewayLocationIntent.Default,
            },

            // Fastest gateway -> Random gateway
            MultiGatewayLocationIntent gatewayIntent when gatewayIntent.Strategy == SelectionStrategy.Fastest => MultiGatewayLocationIntent.Random,

            // Free server with exclusion -> Free server
            FreeServerLocationIntent freeServerIntent when freeServerIntent.ServerToExclude != null => FreeServerLocationIntent.Default,

            _ => null,
        };
    }

    public static IFeatureIntent? GetBaseFeatureIntent(this IFeatureIntent featureIntent)
    {
        return featureIntent switch
        {
            // Secure core with entry country -> Secure core
            SecureCoreFeatureIntent secureCoreIntent when !secureCoreIntent.IsFastest => SecureCoreFeatureIntent.Default,

            _ => null,
        };
    }
}