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

using ProtonVPN.Common.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Localization.Contracts;

namespace ProtonVPN.Client.Helpers;

public static class LocalizationExtensions
{
    public static string GetConnectionIntentTitle(this ILocalizationProvider localizer, IConnectionIntent connectionIntent)
    {
        if (connectionIntent?.Location is not CountryLocationIntent countryIntent || countryIntent.IsFastest)
        {
            return localizer.Get("Country_Fastest");
        }

        return localizer.Get($"Country_val_{countryIntent.CountryCode}");
    }

    public static string GetConnectionIntentSubtitle(this ILocalizationProvider localizer, IConnectionIntent connectionIntent)
    {
        if (connectionIntent?.Feature is SecureCoreFeatureIntent secureCoreIntent && !secureCoreIntent.IsFastest)
        {
            return localizer.GetFormat("Connection_Via_SecureCore", localizer.Get($"Country_val_{secureCoreIntent.EntryCountryCode}"));
        }

        return connectionIntent?.Location switch
        {
            FreeServerLocationIntent freeServerIntent => localizer.GetFormat("Connection_Intent_Free_Server", freeServerIntent.ServerNumber),
            ServerLocationIntent serverIntent => localizer.GetFormat("Connection_Intent_City_Server", serverIntent.CityState, serverIntent.ServerNumber),
            CityStateLocationIntent cityStateIntent => cityStateIntent.CityState,
            _ => string.Empty,
        };
    }
}