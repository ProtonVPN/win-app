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
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Localization.Extensions;

public static class LocalizationExtensions
{
    private const string NULL_VALUE_PLACEHOLDER = "-";

    public static string GetToggleValue(this ILocalizationProvider localizer, bool value)
    {
        return value
           ? localizer.Get("Common_States_On")
           : localizer.Get("Common_States_Off");
    }

    public static string GetFreeServerName(this ILocalizationProvider localizer, ConnectionIntentKind kind)
    {
        return kind switch
        {
            ConnectionIntentKind.Fastest => localizer.Get("Server_Fastest_Free"),
            ConnectionIntentKind.Random => localizer.Get("Server_Free"),
            _ => localizer.Get("Server_Fastest_Free")
        };
    }

    public static string GetCountryName(this ILocalizationProvider localizer, string? countryCode, ConnectionIntentKind intentKind = ConnectionIntentKind.Fastest, bool excludeMyCountry = false)
    {
        return string.IsNullOrEmpty(countryCode)
            ? intentKind switch
            {
                ConnectionIntentKind.Fastest when excludeMyCountry => localizer.Get("Country_Fastest_ExcludingMyCountry"),
                ConnectionIntentKind.Random when excludeMyCountry => localizer.Get("Country_Random_ExcludingMyCountry"),
                ConnectionIntentKind.Fastest => localizer.Get("Country_Fastest"),
                ConnectionIntentKind.Random => localizer.Get("Country_Random"),                
                _ => throw new NotImplementedException($"Intent kind '{intentKind}' is not supported."),
            }
            : localizer.Get($"Country_val_{countryCode}");
    }

    public static string GetConnectionIntentTitle(this ILocalizationProvider localizer, IConnectionIntent connectionIntent)
    {
        return connectionIntent?.Location switch
        {
            CountryLocationIntent countryIntent => localizer.GetCountryName(countryIntent.CountryCode, countryIntent.Kind, countryIntent.IsToExcludeMyCountry),
            GatewayLocationIntent gatewayIntent => gatewayIntent.GatewayName,
            FreeServerLocationIntent freeServerIntent => localizer.GetFreeServerName(freeServerIntent.Kind),
            _ => localizer.Get("Country_Fastest")
        };
    }

    public static string GetConnectionIntentSubtitle(this ILocalizationProvider localizer, IConnectionIntent? connectionIntent, bool useDetailedSubtitle = false)
    {
        if (connectionIntent?.Feature is SecureCoreFeatureIntent secureCoreIntent)
        {
            return localizer.GetSecureCoreLabel(secureCoreIntent.EntryCountryCode);
        }

        return connectionIntent?.Location switch
        {
            ServerLocationIntent serverIntent => ConcatenateLocations(GetStateOrCityName(serverIntent.State, serverIntent.City), serverIntent.Name),
            CityLocationIntent cityIntent => cityIntent.City,
            StateLocationIntent stateIntent => stateIntent.State,
            GatewayServerLocationIntent gatewayServerIntent => ConcatenateLocations(localizer.GetCountryName(gatewayServerIntent.CountryCode), gatewayServerIntent.Name),
            CountryLocationIntent countryIntent => useDetailedSubtitle && countryIntent.IsFastestCountry
                ? localizer.Get("Settings_Connection_Default_Fastest_Description")
                : string.Empty,
            _ => string.Empty,
        };
    }

    public static string GetConnectionProfileSubtitle(this ILocalizationProvider localizer, IConnectionProfile profile)
    {
        string title = localizer.GetConnectionIntentTitle(profile);
        string subtitle = localizer.GetConnectionIntentSubtitle(profile);

        return profile.Feature is SecureCoreFeatureIntent secureCoreIntent && secureCoreIntent.IsFastest
            ? ConcatenateLocations(title, subtitle)
            : $"{title} {subtitle}".Trim();
    }

    public static string GetConnectionDetailsTitle(this ILocalizationProvider localizer, ConnectionDetails connectionDetails)
    {
        return connectionDetails?.OriginalConnectionIntent.Location switch
        {
            CountryLocationIntent countryIntent => countryIntent.IsSpecificCountry
                ? localizer.GetCountryName(connectionDetails.ExitCountryCode)
                : localizer.GetCountryName(countryCode: string.Empty, countryIntent.Kind, countryIntent.IsToExcludeMyCountry),
            GatewayLocationIntent gatewayIntent => connectionDetails.GatewayName,
            FreeServerLocationIntent freeServerIntent => freeServerIntent.Kind switch
            {
                ConnectionIntentKind.Random => localizer.GetCountryName(connectionDetails.ExitCountryCode, freeServerIntent.Kind),
                _ => localizer.GetFreeServerName(freeServerIntent.Kind)
            },
            _ => localizer.Get("Country_Fastest")
        };
    }

    public static string GetConnectionDetailsSubtitle(this ILocalizationProvider localizer, ConnectionDetails? connectionDetails)
    {
        if (connectionDetails?.OriginalConnectionIntent.Feature is SecureCoreFeatureIntent secureCoreIntent &&
            connectionDetails?.OriginalConnectionIntent.Location is CountryLocationIntent locationIntent)
        {
            return locationIntent.IsSpecificCountry
                ? localizer.GetSecureCoreLabel(connectionDetails.EntryCountryCode)
                : $"{localizer.GetCountryName(connectionDetails.ExitCountryCode, locationIntent.Kind, locationIntent.IsToExcludeMyCountry)} {localizer.GetSecureCoreLabel(connectionDetails.EntryCountryCode)}";
        }

        return connectionDetails?.OriginalConnectionIntent.Location switch
        {
            CountryLocationIntent countryIntent => countryIntent.IsSpecificCountry
                ? ConcatenateLocations(GetStateOrCityName(connectionDetails.State, connectionDetails.City), connectionDetails.ServerName)
                : ConcatenateLocations(localizer.GetCountryName(connectionDetails.ExitCountryCode), GetStateOrCityName(connectionDetails.State, connectionDetails.City), connectionDetails.ServerName),
            GatewayLocationIntent => ConcatenateLocations(localizer.GetCountryName(connectionDetails.ExitCountryCode), connectionDetails.ServerName),
            FreeServerLocationIntent freeServerIntent => freeServerIntent.Kind switch
            {
                ConnectionIntentKind.Fastest => ConcatenateLocations(localizer.GetCountryName(connectionDetails.ExitCountryCode, freeServerIntent.Kind), connectionDetails.ServerName),
                ConnectionIntentKind.Random => connectionDetails.ServerName,
                _ => string.Empty
            },
            _ => string.Empty,
        };
    }

    public static string GetConnectionProfileDetailsSubtitle(this ILocalizationProvider localizer, ConnectionDetails connectionDetails)
    {
        string title = localizer.GetConnectionDetailsTitle(connectionDetails);
        string subtitle = localizer.GetConnectionDetailsSubtitle(connectionDetails);

        return connectionDetails.IsSecureCore && connectionDetails.OriginalConnectionIntent.Location is CountryLocationIntent countryIntent && countryIntent.IsSpecificCountry
            ? $"{title} {subtitle}".Trim()
            : ConcatenateLocations(title, subtitle); 
    }

    private static string ConcatenateLocations(params string[] locations)
    {
        return string.Join(" - ", locations.Where(s => !string.IsNullOrEmpty(s))).Trim();
    }

    public static string GetSecureCoreLabel(this ILocalizationProvider localizer, string? entryCountryCode)
    {
        return string.IsNullOrEmpty(entryCountryCode)
            ? localizer.Get("Countries_SecureCore")
            : localizer.GetFormat("Connection_Via_SecureCore", localizer.GetCountryName(entryCountryCode));
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
            _ => time.Hours == 0
                ? localizer.GetPluralFormat("Format_Time_Day", time.Days)
                : string.Format(localizer.GetPlural("Format_Time_DayHour", time.Days), time.Days, time.Hours),
        };
    }

    public static string? GetFormattedShortTime(this ILocalizationProvider localizer, TimeSpan time)
    {
        try
        {
            return time switch
            {
                TimeSpan when time < TimeSpan.Zero => null,
                TimeSpan when time < TimeSpan.FromHours(1) => time.ToString(localizer.Get("Format_Time_MinutesSeconds_Short")),
                TimeSpan when time < TimeSpan.FromDays(1) => time.ToString(localizer.Get("Format_Time_HoursMinutesSeconds_Short")),
                _ => time.ToString(),
            };
        }
        catch (FormatException)
        {
            return time.ToString();
        }
    }

    public static string GetFormattedSize(this ILocalizationProvider localizer, long? sizeInBytes)
    {
        if (!sizeInBytes.HasValue)
        {
            return NULL_VALUE_PLACEHOLDER;
        }

        (double size, ByteMetrics metric) result = ByteConversionHelper.CalculateSize(sizeInBytes.Value);

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

    public static string GetFormattedSpeed(this ILocalizationProvider localizer, long? sizeInBytes)
    {
        if (!sizeInBytes.HasValue)
        {
            return NULL_VALUE_PLACEHOLDER;
        }

        (double size, ByteMetrics metric) result = ByteConversionHelper.CalculateSize(sizeInBytes.Value);

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

    public static string GetSpeedUnit(this ILocalizationProvider localizer, ByteMetrics metric)
    {
        return metric switch
        {
            ByteMetrics.Bytes => localizer.GetFormat("Unit_BytesPerSecond"),
            ByteMetrics.Kilobytes => localizer.GetFormat("Unit_KilobytesPerSecond"),
            ByteMetrics.Megabytes => localizer.GetFormat("Unit_MegabytesPerSecond"),
            ByteMetrics.Gigabytes => localizer.GetFormat("Unit_GigabytesPerSecond"),
            ByteMetrics.Terabytes => localizer.GetFormat("Unit_TerabytesPerSecond"),
            ByteMetrics.Petabytes => localizer.GetFormat("Unit_PetabytesPerSecond"),
            ByteMetrics.Exabytes => localizer.GetFormat("Unit_ExabytesPerSecond"),
            _ => string.Empty,
        };
    }

    public static string GetVpnPlanName(this ILocalizationProvider localizer, string vpnPlan)
    {
        return string.IsNullOrEmpty(vpnPlan)
            ? localizer.Get("Account_VpnPlan_Free")
            : vpnPlan;
    }

    public static string GetFeatureName(this ILocalizationProvider localizer, Feature feature)
    {
        return feature switch
        {
            Feature.SecureCore => localizer.Get("Server_Feature_SecureCore"),
            Feature.Tor => localizer.Get("Server_Feature_Tor"),
            Feature.P2P => localizer.Get("Server_Feature_P2P"),
            Feature.B2B => localizer.Get("Server_Feature_B2B"),
            _ => localizer.Get("Server_Feature_None")
        };
    }

    public static string GetVpnProtocol(this ILocalizationProvider localizer, VpnProtocol? vpnProtocol)
    {
        if (vpnProtocol == null)
        {
            return NULL_VALUE_PLACEHOLDER;
        }

        return vpnProtocol switch
        {
            VpnProtocol.Smart => localizer.Get("VpnProtocol_Smart"),
            VpnProtocol.OpenVpnTcp => localizer.Get("VpnProtocol_OpenVPN_Tcp"),
            VpnProtocol.OpenVpnUdp => localizer.Get("VpnProtocol_OpenVPN_Udp"),
            VpnProtocol.WireGuardUdp => localizer.Get("VpnProtocol_WireGuard_Udp"),
            VpnProtocol.WireGuardTcp => localizer.Get("VpnProtocol_WireGuard_Tcp"),
            VpnProtocol.WireGuardTls => localizer.Get("VpnProtocol_WireGuard_Tls"),
            _ => string.Empty
        };
    }

    public static string GetVpnProtocolDescription(this ILocalizationProvider localizer, VpnProtocol? vpnProtocol)
    {
        return vpnProtocol switch
        {
            VpnProtocol.OpenVpnTcp => localizer.Get("VpnProtocol_OpenVPN_Tcp_Description"),
            VpnProtocol.OpenVpnUdp => localizer.Get("VpnProtocol_OpenVPN_Udp_Description"),
            VpnProtocol.WireGuardUdp => localizer.Get("VpnProtocol_WireGuard_Udp_Description"),
            VpnProtocol.WireGuardTcp => localizer.Get("VpnProtocol_WireGuard_Tcp_Description"),
            VpnProtocol.WireGuardTls => localizer.Get("VpnProtocol_WireGuard_Tls_Description"),
            _ => string.Empty
        };
    }

    public static string? GetExitOrSignOutConfirmationMessage(this ILocalizationProvider localizer, bool isDisconnected, ISettings settings)
    {
        bool isAdvancedKillSwitchActive = settings.IsKillSwitchEnabled &&
                                          settings.KillSwitchMode == KillSwitchMode.Advanced;
        if (!isDisconnected)
        {
            if (isAdvancedKillSwitchActive)
            {
                return CreateBulletPoints(true,
                    localizer.Get("Common_Confirmation_YouWillBeDisconnected_Message"),
                    localizer.Get("Common_Confirmation_YouWillBeDisconnectedWithKillSwitch_Message"));
            }

            return localizer.Get("Common_Confirmation_YouWillBeDisconnected_Message");
        }

        return isAdvancedKillSwitchActive
            ? localizer.Get("Common_Confirmation_KillSwitch_Message")
            : null;
    }

    public static string CreateBulletPoints(bool isToAddEmptyLine, params string[] lines)
    {
        string separator = (isToAddEmptyLine ? $"{Environment.NewLine}" : null) + $"{Environment.NewLine}• ";
        return "• " + string.Join(separator, lines);
    }

    private static string GetStateOrCityName(string state, string city)
    {
        return string.IsNullOrWhiteSpace(state)
            ? city
            : state;
    }
}