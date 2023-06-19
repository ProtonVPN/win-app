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
using ProtonVPN.Client.Common.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;

namespace ProtonVPN.Client.Helpers;

public static class LocalizationExtensions
{
    public static string GetCountryName(this ILocalizationProvider localizer, string? countryCode)
    {
        return string.IsNullOrEmpty(countryCode)
            ? localizer.Get("Country_Fastest")
            : localizer.Get($"Country_val_{countryCode}");
    }

    public static string GetConnectionIntentTitle(this ILocalizationProvider localizer, IConnectionIntent connectionIntent)
    {
        if (connectionIntent?.Location is not CountryLocationIntent countryIntent || countryIntent.IsFastest)
        {
            return localizer.Get("Country_Fastest");
        }

        return localizer.GetCountryName(countryIntent.CountryCode);
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

    public static string? GetFormattedTime(this ILocalizationProvider localizer, TimeSpan time)
    {
        return time switch
        {
            TimeSpan when time < TimeSpan.Zero => null,
            TimeSpan when time < TimeSpan.FromMinutes(1) => localizer.GetFormat("Format_Time_Seconds", time.Seconds),
            TimeSpan when time < TimeSpan.FromHours(1) => time.Seconds == 0
                                ? localizer.GetFormat("Format_Time_Minutes", time.Minutes)
                                : localizer.GetFormat("Format_Time_MinutesSeconds", time.Minutes, time.Seconds),
            TimeSpan when time < TimeSpan.FromDays(1) => time.Minutes == 0
                                ? localizer.GetFormat("Format_Time_Hours", time.Hours)
                                : localizer.GetFormat("Format_Time_HoursMinutes", time.Hours, time.Minutes),
            TimeSpan when time < TimeSpan.FromDays(2) => time.Hours == 0
                                ? localizer.GetFormat("Format_Time_Day", time.Days)
                                : localizer.GetFormat("Format_Time_DayHours", time.Days, time.Hours),
            _ => time.Hours == 0
                                ? localizer.GetFormat("Format_Time_Days", time.Days)
                                : localizer.GetFormat("Format_Time_DaysHours", time.Days, time.Hours),
        };
    }

    public static string GetFormattedSize(this ILocalizationProvider localizer, long sizeInBytes)
    {
        (double size, ByteMetrics metric) result = ByteConversionHelper.CalculateSize(sizeInBytes);

        return result.metric switch
        {
            ByteMetrics.Bytes => localizer.GetFormat("Format_Size_Bytes", result.size),
            ByteMetrics.Kilobytes => localizer.GetFormat("Format_Size_Kilobytes", result.size),
            ByteMetrics.Megabytes => localizer.GetFormat("Format_Size_Megabytes", result.size),
            ByteMetrics.Gigabytes => localizer.GetFormat("Format_Size_Gigabytes", result.size),
            ByteMetrics.Terabytes => localizer.GetFormat("Format_Size_Terabytes", result.size),
            ByteMetrics.Petabytes => localizer.GetFormat("Format_Size_Petabytes", result.size),
            ByteMetrics.Exabytes => localizer.GetFormat("Format_Size_Exabytes", result.size),
            _ => string.Empty,
        };
    }

    public static string GetFormattedSpeed(this ILocalizationProvider localizer, long sizeInBytes)
    {
        (double size, ByteMetrics metric) result = ByteConversionHelper.CalculateSize(sizeInBytes);

        return result.metric switch
        {
            ByteMetrics.Bytes => localizer.GetFormat("Format_Speed_BytesPerSecond", result.size),
            ByteMetrics.Kilobytes => localizer.GetFormat("Format_Speed_KilobytesPerSecond", result.size),
            ByteMetrics.Megabytes => localizer.GetFormat("Format_Speed_MegabytesPerSecond", result.size),
            ByteMetrics.Gigabytes => localizer.GetFormat("Format_Speed_GigabytesPerSecond", result.size),
            ByteMetrics.Terabytes => localizer.GetFormat("Format_Speed_TerabytesPerSecond", result.size),
            ByteMetrics.Petabytes => localizer.GetFormat("Format_Speed_PetabytesPerSecond", result.size),
            ByteMetrics.Exabytes => localizer.GetFormat("Format_Speed_ExabytesPerSecond", result.size),
            _ => string.Empty,
        };
    }
}