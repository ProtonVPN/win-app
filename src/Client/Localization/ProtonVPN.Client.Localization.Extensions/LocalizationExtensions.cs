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
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Cities;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.FreeServers;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Gateways;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.GatewayServers;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Servers;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.States;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Extensions;
using ProtonVPN.Common.Core.Dns;
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

    public static string GetFreeServerName(this ILocalizationProvider localizer, SelectionStrategy strategy)
    {
        return strategy switch
        {
            SelectionStrategy.Fastest => localizer.Get("Server_Fastest_Free"),
            SelectionStrategy.Random => localizer.Get("Server_Free"),
            _ => localizer.Get("Server_Fastest_Free")
        };
    }

    public static string GetCountryName(this ILocalizationProvider localizer, string? countryCode, SelectionStrategy strategy = SelectionStrategy.Fastest, bool excludeMyCountry = false)
    {
        return string.IsNullOrEmpty(countryCode)
            ? strategy switch
            {
                SelectionStrategy.Fastest when excludeMyCountry => localizer.Get("Country_Fastest_ExcludingMyCountry"),
                SelectionStrategy.Random when excludeMyCountry => localizer.Get("Country_Random_ExcludingMyCountry"),
                SelectionStrategy.Fastest => localizer.Get("Country_Fastest"),
                SelectionStrategy.Random => localizer.Get("Country_Random"),
                _ => throw new NotImplementedException($"Intent strategy '{strategy}' is not supported."),
            }
            : localizer.Get($"Country_val_{countryCode}");
    }

    public static string GetGatewayName(this ILocalizationProvider localizer, string? gatewayName, SelectionStrategy strategy = SelectionStrategy.Fastest)
    {
        return string.IsNullOrEmpty(gatewayName)
            ? strategy switch
            {
                SelectionStrategy.Fastest => localizer.Get("Gateway_Fastest"),
                SelectionStrategy.Random => localizer.Get("Gateway_Random"),
                _ => throw new NotImplementedException($"Intent strategy '{strategy}' is not supported."),
            }
            : gatewayName;
    }

    public static string GetConnectionIntentTitle(this ILocalizationProvider localizer, IConnectionIntent? connectionIntent)
    {
        return connectionIntent?.Location switch
        {
            SingleCountryLocationIntent intent => localizer.GetCountryName(intent.CountryCode),
            MultiCountryLocationIntent intent => localizer.GetCountryName(string.Empty, intent.Strategy, intent.IsToExcludeMyCountry),
            StateLocationIntentBase intent => localizer.GetCountryName(intent.Country.CountryCode),
            CityLocationIntentBase intent => localizer.GetCountryName(intent.Country.CountryCode),
            ServerLocationIntentBase intent => localizer.GetCountryName(intent.Country.CountryCode),

            SingleGatewayLocationIntent intent => localizer.GetGatewayName(intent.GatewayName),
            MultiGatewayLocationIntent intent => localizer.GetGatewayName(string.Empty, intent.Strategy),
            GatewayServerLocationIntentBase intent => localizer.GetGatewayName(intent.Gateway.GatewayName),

            FreeServerLocationIntent intent => localizer.GetFreeServerName(intent.Strategy),
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
            SingleCountryLocationIntent => string.Empty,
            MultiCountryLocationIntent when !useDetailedSubtitle => string.Empty,
            MultiCountryLocationIntent intent => intent.Strategy switch
            { 
                SelectionStrategy.Fastest => localizer.Get("Settings_Connection_Default_Fastest_Description"),
                SelectionStrategy.Random => localizer.Get("Settings_Connection_Default_Random_Description"),
                _ => string.Empty
            },
            SingleStateLocationIntent intent => intent.StateName,
            MultiStateLocationIntent intent => intent.Strategy switch
            {
                SelectionStrategy.Fastest => localizer.Get("State_Fastest"),
                SelectionStrategy.Random => localizer.Get("State_Random"),
                _ => string.Empty
            },
            SingleCityLocationIntent intent => ConcatenateLocations(intent.State?.StateName, intent.CityName),
            MultiCityLocationIntent intent => intent.Strategy switch
            {
                SelectionStrategy.Fastest => localizer.Get("City_Fastest"),
                SelectionStrategy.Random => localizer.Get("City_Random"),
                _ => string.Empty
            },
            SingleServerLocationIntent intent => ConcatenateLocations(intent.State?.StateName, intent.City.CityName, intent.Server.Name),
            MultiServerLocationIntent intent => intent.Strategy switch
            {
                SelectionStrategy.Fastest => localizer.Get("Server_Fastest"),
                SelectionStrategy.Random => localizer.Get("Server_Random"),
                _ => string.Empty
            },
            SingleGatewayLocationIntent => string.Empty,
            MultiGatewayLocationIntent intent => intent.Strategy switch
            {
                SelectionStrategy.Fastest => localizer.Get("Settings_Connection_Default_Fastest_Description"),
                SelectionStrategy.Random => localizer.Get("Settings_Connection_Default_Random_Description"),
                _ => string.Empty
            },
            SingleGatewayServerLocationIntent intent => ConcatenateLocations(localizer.GetCountryName(intent.Server.CountryCode), intent.Server.Name),
            MultiGatewayServerLocationIntent intent => intent.Strategy switch
            {
                SelectionStrategy.Fastest => localizer.Get("Server_Fastest"),
                SelectionStrategy.Random => localizer.Get("Server_Random"),
                _ => string.Empty
            },
            FreeServerLocationIntent => string.Empty,
            _ => string.Empty,
        };
    }

    public static string GetConnectionProfileSubtitle(this ILocalizationProvider localizer, IConnectionProfile? profile)
    {
        string title = localizer.GetConnectionIntentTitle(profile);
        string subtitle = localizer.GetConnectionIntentSubtitle(profile);

        return profile != null && profile.Feature is SecureCoreFeatureIntent secureCoreIntent && !secureCoreIntent.IsFastest
            ? $"{title} {subtitle}".Trim()
            : ConcatenateLocations(title, subtitle);
    }

    public static string GetConnectionDetailsTitle(this ILocalizationProvider localizer, ConnectionDetails? connectionDetails)
    {
        return connectionDetails?.OriginalConnectionIntent.Location switch
        {
            SingleCountryLocationIntent or
            StateLocationIntentBase or
            CityLocationIntentBase or
            ServerLocationIntentBase => localizer.GetCountryName(connectionDetails.ExitCountryCode),

            MultiCountryLocationIntent intent => localizer.GetCountryName(string.Empty, intent.Strategy, intent.IsToExcludeMyCountry),

            SingleGatewayLocationIntent or
            GatewayServerLocationIntentBase => connectionDetails.GatewayName,

            MultiGatewayLocationIntent intent => localizer.GetGatewayName(string.Empty, intent.Strategy),

            FreeServerLocationIntent intent => intent.Strategy switch
            {
                SelectionStrategy.Random => localizer.GetCountryName(connectionDetails.ExitCountryCode, intent.Strategy),
                _ => localizer.GetFreeServerName(intent.Strategy)
            },
            _ => localizer.Get("Country_Fastest")
        };
    }

    public static string GetConnectionDetailsSubtitle(this ILocalizationProvider localizer, ConnectionDetails? connectionDetails)
    {
        if (connectionDetails?.OriginalConnectionIntent.Feature is SecureCoreFeatureIntent &&
            connectionDetails?.OriginalConnectionIntent.Location is CountryLocationIntentBase locationIntent)
        {
            return locationIntent is SingleCountryLocationIntent
                ? localizer.GetSecureCoreLabel(connectionDetails.EntryCountryCode)
                : $"{localizer.GetCountryName(connectionDetails.ExitCountryCode)} {localizer.GetSecureCoreLabel(connectionDetails.EntryCountryCode)}";
        }

        return connectionDetails?.OriginalConnectionIntent.Location switch
        {
            SingleCountryLocationIntent or
            StateLocationIntentBase or
            CityLocationIntentBase or
            ServerLocationIntentBase => ConcatenateLocations(connectionDetails.State, connectionDetails.City, connectionDetails.ServerName),

            MultiCountryLocationIntent => ConcatenateLocations(localizer.GetCountryName(connectionDetails.ExitCountryCode), connectionDetails.State, connectionDetails.City, connectionDetails.ServerName),

            SingleGatewayLocationIntent or
            GatewayServerLocationIntentBase => ConcatenateLocations(localizer.GetCountryName(connectionDetails.ExitCountryCode), connectionDetails.ServerName),

            MultiGatewayLocationIntent => ConcatenateLocations(connectionDetails.GatewayName, localizer.GetCountryName(connectionDetails.ExitCountryCode), connectionDetails.ServerName),

            FreeServerLocationIntent intent => intent.Strategy switch
            {
                SelectionStrategy.Random => connectionDetails.ServerName,
                _ => ConcatenateLocations(localizer.GetCountryName(connectionDetails.ExitCountryCode), connectionDetails.ServerName)
            },
            _ => string.Empty,
        };
    }

    public static string GetConnectionProfileDetailsSubtitle(this ILocalizationProvider localizer, ConnectionDetails? connectionDetails)
    {
        string title = localizer.GetConnectionDetailsTitle(connectionDetails);
        string subtitle = localizer.GetConnectionDetailsSubtitle(connectionDetails);

        return connectionDetails != null &&
               connectionDetails.IsSecureCore &&
               connectionDetails.OriginalConnectionIntent.Location is SingleCountryLocationIntent
            ? $"{title} {subtitle}".Trim()
            : ConcatenateLocations(title, subtitle);
    }

    private static string ConcatenateLocations(params string?[] locations)
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

    public static string GetVpnPlanName(this ILocalizationProvider localizer, VpnPlan vpnPlan)
    {
        return string.IsNullOrEmpty(vpnPlan.Title)
            ? localizer.Get("Account_VpnPlan_Free")
            : vpnPlan.Title;
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

    public static string GetFeatureDescription(this ILocalizationProvider localizer, Feature feature)
    {
        return feature switch
        {
            Feature.SecureCore => localizer.Get("Server_Feature_SecureCore_Description"),
            Feature.P2P => localizer.Get("Server_Feature_P2P_Description"),
            Feature.B2B => localizer.Get("Server_Feature_B2B_Description"),
            _ => string.Empty
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

    public static string GetNetShieldMode(this ILocalizationProvider localizer, bool isEnabled, NetShieldMode netShieldMode)
    {
        return isEnabled
            ? netShieldMode switch
            {
                NetShieldMode.BlockMalwareOnly => localizer.Get("Settings_Connection_NetShield_BlockMalwareOnly"),
                NetShieldMode.BlockAdsMalwareTrackers => localizer.Get("Settings_Connection_NetShield_BlockAdsMalwareTrackers"),
                _ => localizer.Get("Common_States_On")
            }
            : localizer.Get("Common_States_Off");
    }

    public static string GetNatType(this ILocalizationProvider localizer, NatType? natType)
    {
        return natType switch
        {
            NatType.Moderate => localizer.Get("Settings_Connection_Advanced_NatType_Moderate"),
            _ => localizer.Get("Settings_Connection_Advanced_NatType_Strict")
        };
    }

    public static string GetConnectAndGoMode(this ILocalizationProvider localizer, bool isEnabled, ConnectAndGoMode connectAndGoMode)
    {
        return isEnabled
            ? connectAndGoMode switch
            {
                ConnectAndGoMode.Website => localizer.Get("Profile_Options_ConnectAndGo_Website"),
                ConnectAndGoMode.Application => localizer.Get("Profile_Options_ConnectAndGo_Application"),
                _ => localizer.Get("Common_States_On")
            }
            : localizer.Get("Common_States_Off");
    }

    public static string? GetExitOrSignOutConfirmationMessage(this ILocalizationProvider localizer, bool isDisconnected, ISettings settings)
    {
        if (settings.IsAdvancedKillSwitchActive())
        {
            return isDisconnected
                ? localizer.Get("Common_Confirmation_KillSwitch_Message")
                : CreateBulletPoints(
                    false,
                    localizer.Get("Common_Confirmation_YouWillBeDisconnected_Message"),
                    localizer.Get("Common_Confirmation_YouWillBeDisconnectedWithKillSwitch_Message"));
        }

        return isDisconnected
            ? null
            : localizer.Get("Common_Confirmation_YouWillBeDisconnected_Message");
    }

    public static string CreateBulletPoints(bool isToAddEmptyLine, params string[] lines)
    {
        string separator = (isToAddEmptyLine ? $"{Environment.NewLine}" : null) + $"{Environment.NewLine}• ";
        return "• " + string.Join(separator, lines);
    }

    private static string GetStateOrCityName(string? state, string city)
    {
        return string.IsNullOrWhiteSpace(state)
            ? city
            : state;
    }

    public static string GetConnectionGroupName(this ILocalizationProvider localizer, ConnectionGroupType groupType, int itemsCount)
    {
        string localizationKey = groupType switch
        {
            ConnectionGroupType.Countries => "Connections_Countries",
            ConnectionGroupType.States => "Connections_States",
            ConnectionGroupType.Cities => "Connections_Cities",
            ConnectionGroupType.Servers => "Connections_Servers",
            ConnectionGroupType.FreeServers => "Connections_Free_Servers",
            ConnectionGroupType.SecureCoreCountries => "Connections_SecureCore_Countries",
            ConnectionGroupType.SecureCoreCountryPairs or
            ConnectionGroupType.SecureCoreServers => "Connections_SecureCore_Servers",
            ConnectionGroupType.P2PCountries => "Connections_P2P_Countries",
            ConnectionGroupType.P2PStates => "Connections_P2P_States",
            ConnectionGroupType.P2PCities => "Connections_P2P_Cities",
            ConnectionGroupType.P2PServers => "Connections_P2P_Servers",
            ConnectionGroupType.TorCountries => "Connections_Tor_Countries",
            ConnectionGroupType.TorServers => "Connections_Tor_Servers",
            ConnectionGroupType.Gateways => "Connections_Gateways",
            ConnectionGroupType.GatewayServers => "Connections_Gateways_Servers",
            ConnectionGroupType.PinnedRecents => "Connections_Recents_Pinned",
            ConnectionGroupType.Recents => "Connections_Recents",
            ConnectionGroupType.Profiles => "Connections_Profiles",
            _ => throw new NotSupportedException($"Group type '{groupType}' is not supported.")
        };

        bool shouldHideItemsCount = groupType
            is ConnectionGroupType.Servers
            or ConnectionGroupType.SecureCoreServers
            or ConnectionGroupType.P2PServers
            or ConnectionGroupType.TorServers
            or ConnectionGroupType.FreeServers;

        return shouldHideItemsCount
            ? localizer.GetPlural(localizationKey, itemsCount)
            : localizer.GetPluralFormat(localizationKey, itemsCount);
    }

    public static string GetKillSwitchMode(this ILocalizationProvider localizer, KillSwitchMode mode)
    {
        return mode switch
        {
            KillSwitchMode.Standard => localizer.Get("Settings_Connection_KillSwitch_Standard"),
            KillSwitchMode.Advanced => localizer.Get("Settings_Connection_KillSwitch_Advanced"),
            _ => throw new NotSupportedException($"Kill switch mode '{mode}' is not supported.")
        };
    }

    public static string GetSplitTunnelingMode(this ILocalizationProvider localizer, SplitTunnelingMode mode, bool useShortVersion = false)
    {
        return mode switch
        {
            SplitTunnelingMode.Standard => useShortVersion
                ? localizer.Get("Settings_Connection_SplitTunneling_Standard_Short")
                : localizer.Get("Settings_Connection_SplitTunneling_Standard"),
            SplitTunnelingMode.Inverse => useShortVersion
                ? localizer.Get("Settings_Connection_SplitTunneling_Inverse_Short")
                : localizer.Get("Settings_Connection_SplitTunneling_Inverse"),
            _ => throw new NotSupportedException($"Split tunneling mode '{mode}' is not supported.")
        };
    }

    public static string GetSplitTunnelingGroupName(this ILocalizationProvider localizer, SplitTunnelingGroupType groupType, int itemsCount)
    {
        return localizer.GetPluralFormat(
            groupType switch
            {
                SplitTunnelingGroupType.ProtectedApps => "Flyouts_SplitTunneling_Protected_Apps",
                SplitTunnelingGroupType.ExcludedApps => "Flyouts_SplitTunneling_Excluded_Apps",
                SplitTunnelingGroupType.ProtectedIpAddresses => "Flyouts_SplitTunneling_Protected_Ips",
                SplitTunnelingGroupType.ExcludedIpAddresses => "Flyouts_SplitTunneling_Excluded_Ips",
                _ => throw new NotSupportedException($"Group type '{groupType}' is not supported.")
            }, itemsCount);
    }

    public static string GetFormattedElapsedTime(this ILocalizationProvider localizer, DateTime utcDate)
    {
        DateTime utcNow = DateTime.UtcNow;
        TimeSpan timeDifference = utcNow - utcDate;

        DateTime localDateTime = utcDate.ToLocalTime();
        string formattedLocalDateTime = $"{localDateTime.ToShortDateString()}, {localDateTime.ToShortTimeString()}";

        return timeDifference switch
        {
            _ when timeDifference < TimeSpan.Zero => formattedLocalDateTime,
            _ when timeDifference < TimeSpan.FromMinutes(1) => localizer.GetPluralFormat("Format_Time_Seconds_Ago", timeDifference.Seconds),
            _ when timeDifference < TimeSpan.FromHours(1) => localizer.GetPluralFormat("Format_Time_Minutes_Ago", timeDifference.Minutes),
            _ when timeDifference < TimeSpan.FromDays(1) => localizer.GetPluralFormat("Format_Time_Hours_Ago", timeDifference.Hours),
            _ => formattedLocalDateTime
        };
    }
}