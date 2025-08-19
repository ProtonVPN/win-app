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

using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Settings.Contracts;

public interface IUserSettings
{
    string? Username { get; set; }
    string? UserDisplayName { get; set; }
    DateTimeOffset? UserCreationDateUtc { get; set; }
    string Theme { get; set; }
    int WindowWidth { get; set; }
    int WindowHeight { get; set; }
    int? WindowXPosition { get; set; }
    int? WindowYPosition { get; set; }
    bool IsWindowMaximized { get; set; }
    bool IsNavigationPaneOpened { get; set; }
    int SidebarWidth { get; set; }
    bool IsRecentsPaneOpened { get; set; }
    bool IsConnectionDetailsPaneOpened { get; set; }
    VpnProtocol VpnProtocol { get; set; }
    OpenVpnAdapter OpenVpnAdapter { get; set; }
    VpnPlan VpnPlan { get; set; }
    int MaxDevicesAllowed { get; set; }
    ConnectionAsymmetricKeyPair? ConnectionKeyPair { get; set; }
    ConnectionCertificate? ConnectionCertificate { get; set; }
    NatType NatType { get; set; }
    bool IsVpnAcceleratorEnabled { get; set; }
    bool IsLocalAreaNetworkAccessEnabled { get; set; }
    bool IsNotificationEnabled { get; set; }
    bool IsShareStatisticsEnabled { get; set; }
    bool IsIpv6LeakProtectionEnabled { get; set; }
    bool IsIpv6Enabled { get; set; }
    bool IsCustomDnsServersEnabled { get; set; }
    List<CustomDnsServer> CustomDnsServersList { get; set; }
    bool IsAutoConnectEnabled { get; set; }
    bool IsNetShieldEnabled { get; set; }
    NetShieldMode NetShieldMode { get; set; }
    bool IsPortForwardingEnabled { get; set; }
    bool IsPortForwardingNotificationEnabled { get; set; }
    bool IsSplitTunnelingEnabled { get; set; }
    bool IsSmartReconnectEnabled { get; set; }
    bool IsUserSettingsMigrationDone { get; set; }
    SplitTunnelingMode SplitTunnelingMode { get; set; }
    List<SplitTunnelingApp> SplitTunnelingStandardAppsList { get; set; }
    List<SplitTunnelingApp> SplitTunnelingInverseAppsList { get; set; }
    List<SplitTunnelingIpAddress> SplitTunnelingStandardIpAddressesList { get; set; }
    List<SplitTunnelingIpAddress> SplitTunnelingInverseIpAddressesList { get; set; }
    List<string> Ipv6Fragments { get; set; }
    ChangeServerAttempts ChangeServerAttempts { get; set; }
    DefaultConnection DefaultConnection { get; set; }
    bool WasWelcomeOverlayDisplayed { get; set; }
    bool WasWelcomePlusOverlayDisplayed { get; set; }
    bool WasWelcomeUnlimitedOverlayDisplayed { get; set; }
    bool WasWelcomeB2BOverlayDisplayed { get; set; }
    DateTimeOffset LogicalsLastModifiedDate { get; set; }
    bool IsP2PInfoBannerDismissed { get; set; }
    bool IsSecureCoreInfoBannerDismissed { get; set; }
    bool IsTorInfoBannerDismissed { get; set; }
    bool IsGatewayInfoBannerDismissed { get; set; }
    int LastSeenWhatsNewOverlayVersion { get; set; }
}