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
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Localization.Extensions;

public static class LocalizationExtensions
{
    public static string GetToggleValue(this ILocalizationProvider localizer, bool value)
    {
        return value
           ? localizer.Get("Common_States_On")
           : localizer.Get("Common_States_Off");
    }

    public static string GetFreeServerName(this ILocalizationProvider localizer, FreeServerType type)
    {
        return type switch
        {
            FreeServerType.Fastest => localizer.Get("Server_Fastest_Free"),
            FreeServerType.Random => localizer.Get("Server_Free"),
            _ => localizer.Get("Server_Fastest_Free")
        };
    }

    public static string GetCountryName(this ILocalizationProvider localizer, string countryCode)
    {
        return string.IsNullOrEmpty(countryCode)
            ? localizer.Get("Country_Fastest")
            : localizer.Get($"Country_val_{countryCode}");
    }

    public static string GetConnectionIntentTitle(this ILocalizationProvider localizer, IConnectionIntent connectionIntent)
    {
        return connectionIntent?.Location switch
        {
            CountryLocationIntent countryIntent => localizer.GetCountryName(countryIntent.CountryCode),
            GatewayLocationIntent gatewayIntent => gatewayIntent.GatewayName,
            FreeServerLocationIntent freeServerIntent => localizer.GetFreeServerName(freeServerIntent.Type),
            _ => localizer.Get("Country_Fastest")
        };
    }

    public static string GetConnectionDetailsTitle(this ILocalizationProvider localizer, ConnectionDetails connectionDetails)
    {
        return connectionDetails?.OriginalConnectionIntent.Location switch
        {
            CountryLocationIntent countryIntent => countryIntent.IsFastest
                ? localizer.Get("Country_Fastest")
                : localizer.GetCountryName(connectionDetails.ExitCountryCode),
            GatewayLocationIntent gatewayIntent => connectionDetails.GatewayName,
            FreeServerLocationIntent freeServerIntent => freeServerIntent.Type switch
            {
                FreeServerType.Random => localizer.GetCountryName(connectionDetails.ExitCountryCode),
                _ => localizer.GetFreeServerName(freeServerIntent.Type)
            },
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
            ServerLocationIntent serverIntent => localizer.GetFormat("Connection_Intent_City_Server", GetStateOrCityName(serverIntent.State, serverIntent.City), serverIntent.Number).Trim(),
            CityLocationIntent cityIntent => cityIntent.City,
            StateLocationIntent stateIntent => stateIntent.State,
            GatewayServerLocationIntent gatewayServerIntent => localizer.GetFormat("Connection_Intent_Country_Server", localizer.GetCountryName(gatewayServerIntent.CountryCode), gatewayServerIntent.Number).Trim(),
            FreeServerLocationIntent freeServerIntent => useDetailedSubtitle ? localizer.Get("Connection_Intent_AutoSelected") : string.Empty,
            CountryLocationIntent countryIntent => useDetailedSubtitle && countryIntent.IsFastest ? localizer.Get("Settings_Connection_Default_Fastest_Description") : string.Empty,
            _ => string.Empty,
        };
    }

    public static string GetConnectionDetailsSubtitle(this ILocalizationProvider localizer, ConnectionDetails? connectionDetails)
    {
        if (connectionDetails?.OriginalConnectionIntent.Feature is SecureCoreFeatureIntent secureCoreIntent &&
            connectionDetails?.OriginalConnectionIntent.Location is CountryLocationIntent locationIntent)
        {
            return locationIntent.IsFastest
                ? $"{localizer.GetCountryName(connectionDetails.ExitCountryCode)} {localizer.GetSecureCoreLabel(connectionDetails.EntryCountryCode)}"
                : localizer.GetSecureCoreLabel(connectionDetails.EntryCountryCode);
        }

        return connectionDetails?.OriginalConnectionIntent.Location switch
        {
            ServerLocationIntent serverIntent => localizer.GetFormat("Connection_Intent_City_Server", GetStateOrCityName(connectionDetails.State, connectionDetails.City), connectionDetails.ServerNumber),
            CityLocationIntent cityIntent => connectionDetails.City,
            StateLocationIntent stateIntent => connectionDetails.State,
            CountryLocationIntent countryIntent => countryIntent.IsFastest
                ? localizer.GetCountryName(connectionDetails.ExitCountryCode)
                : string.Empty,
            GatewayServerLocationIntent gatewayServerIntent => localizer.GetFormat("Connection_Intent_Country_Server", localizer.GetCountryName(connectionDetails.ExitCountryCode), connectionDetails.ServerNumber),
            FreeServerLocationIntent freeServerIntent => freeServerIntent.Type switch
            {
                FreeServerType.Fastest => localizer.GetFormat("Connection_Intent_Country_Server", localizer.GetCountryName(connectionDetails.ExitCountryCode), connectionDetails.ServerNumber),
                FreeServerType.Random => $"#{connectionDetails.ServerNumber}",
                _ => string.Empty
            },
            _ => string.Empty,
        };
    }

    public static string GetSecureCoreLabel(this ILocalizationProvider localizer, string? entryCountryCode)
    {
        return string.IsNullOrEmpty(entryCountryCode)
            ? localizer.GetFormat("Connection_Via_SecureCore", localizer.Get("Countries_SecureCore"))
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

    public static string GetFormattedShortTime(this ILocalizationProvider localizer, TimeSpan time)
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
    public static string GetVpnPlanName(this ILocalizationProvider localizer, string vpnPlan)
    {
        return string.IsNullOrEmpty(vpnPlan)
            ? localizer.Get("Account_VpnPlan_Free")
            : vpnPlan;
    }

    public static string GetVpnProtocol(this ILocalizationProvider localizer, VpnProtocol vpnProtocol)
    {
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

    public static string GetVpnError(this ILocalizationProvider localizer, VpnError vpnError, bool isPaidUser)
    {
        return vpnError switch
        {
            VpnError.WireGuardAdapterInUseError => localizer.Get("Connection_Error_WireGuardAdapterInUse"),
            VpnError.NoServers => localizer.Get("Connection_Error_NoServers"),
            VpnError.NoTapAdaptersError => localizer.Get("Connection_Error_NoTapAdapters"),
            VpnError.TapAdapterInUseError => localizer.Get("Connection_Error_TapAdapterInUse"),
            VpnError.TapRequiresUpdateError => localizer.Get("Connection_Error_TapRequiresUpdate"),
            VpnError.TlsCertificateError => localizer.Get("Connection_Error_TlsCertificate"),
            VpnError.RpcServerUnavailable => localizer.Get("Connection_Error_RpcServerUnavailable"),
            VpnError.MissingConnectionCertificate => localizer.Get("Connection_Error_MissingConnectionCertificate"),
            VpnError.SessionLimitReachedBasic or
                VpnError.SessionLimitReachedFree or
                VpnError.SessionLimitReachedPlus or
                VpnError.SessionLimitReachedPro or
                VpnError.SessionLimitReachedVisionary or
                VpnError.SessionLimitReachedUnknown => localizer.Get(isPaidUser
                    ? "Connection_Error_SessionLimitReachedPaidUser"
                    : "Connection_Error_SessionLimitReachedFreeUser"),
            _ => localizer.Get("Connection_Error_Unknown")
        };
    }

    public static string GetDisconnectErrorActionButtonTitle(this ILocalizationProvider localizer, VpnError vpnError)
    {
        return vpnError switch
        {
            VpnError.RpcServerUnavailable
                or VpnError.TapAdapterInUseError
                or VpnError.NoTapAdaptersError
                or VpnError.TapRequiresUpdateError => localizer.Get("Connection_Error_ViewPossibleSolutions"),
            _ => localizer.Get("Connection_Error_ReportAnIssue")
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