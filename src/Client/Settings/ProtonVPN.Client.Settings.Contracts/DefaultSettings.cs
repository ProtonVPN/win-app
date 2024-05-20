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

using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Helpers;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Settings.Contracts;

public static class DefaultSettings
{
    public static string Theme = "Dark";
    public static int WindowWidth = 1016;
    public static int WindowHeight = 758;
    public static string Language = "en-US";
    public static VpnProtocol VpnProtocol = VpnProtocol.Smart;
    public static NatType NatType = NatType.Strict;
    public static bool IsVpnAcceleratorEnabled = true;
    public static bool IsWindowMaximized = false;
    public static bool IsNavigationPaneOpened = true;
    public static bool IsConnectionDetailsPaneOpened = true;
    public static bool IsNotificationEnabled = true;
    public static bool IsBetaAccessEnabled = false;
    public static bool AreAutomaticUpdatesEnabled = true;
    public static bool IsShareStatisticsEnabled = true;
    public static bool IsShareCrashReportsEnabled = true;
    public static bool IsAlternativeRoutingEnabled = true;
    public static bool IsCustomDnsServersEnabled = false;
    public static List<CustomDnsServer> CustomDnsServersList = new();
    public static bool IsDoHEnabled = true;
    public static int[] WireGuardPorts = { 443, 88, 1224, 51820, 500, 4500 };
    public static int[] OpenVpnTcpPorts = { 443, 1194, 4569, 5060, 80 };
    public static int[] OpenVpnUdpPorts = { 443, 3389, 8080, 8443 };
    public static bool IsAutoLaunchEnabled = true;
    public static AutoLaunchMode AutoLaunchMode = AutoLaunchMode.MinimizeToSystemTray;
    public static bool IsAutoConnectEnabled = true;
    public static bool IsKillSwitchEnabled = false;
    public static bool IsGlobalSettingsMigrationDone = false;
    public static bool IsUserSettingsMigrationDone = false;
    public static KillSwitchMode KillSwitchMode = KillSwitchMode.Standard;
    public static bool IsPortForwardingEnabled = false;
    public static bool IsPortForwardingNotificationEnabled = true;
    public static bool IsSplitTunnelingEnabled = false;
    public static bool IsSmartReconnectEnabled = true;
    public static SplitTunnelingMode SplitTunnelingMode = SplitTunnelingMode.Standard;
    public static List<SplitTunnelingIpAddress> SplitTunnelingIpAddressesList = new();
    public static bool IsIpv6LeakProtectionEnabled = true;
    public static OpenVpnAdapter OpenVpnAdapter = OpenVpnAdapter.Tun;
    public static VpnPlan VpnPlan = VpnPlan.Default;
    public static List<FeatureFlag> FeatureFlags = new();
    public static bool IsFeatureConnectedServerCheckEnabled = true;
    public static TimeSpan ConnectedServerCheckInterval = TimeSpan.FromMinutes(30);
    public static DefaultConnection DefaultConnection = DefaultConnection.Fastest;

    public static ChangeServerSettings ChangeServerSettings = new()
    {
        AttemptsLimit = 4,
        ShortDelay = TimeSpan.FromSeconds(90),
        LongDelay = TimeSpan.FromSeconds(1200),
    };

    public static ChangeServerAttempts ChangeServerAttempts = new()
    {
        LastAttemptUtcDate = DateTimeOffset.MinValue,
        AttemptsCount = 0
    };

    public static bool IsNetShieldEnabled(bool isPaidUser)
    {
        return isPaidUser;
    }

    public static List<SplitTunnelingApp> SplitTunnelingAppsList()
    {
        return DefaultAppsHelper.GetBrowserApps().ToList();
    }
}