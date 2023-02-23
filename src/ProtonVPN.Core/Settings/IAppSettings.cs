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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Common;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Native.Structures;
using ProtonVPN.Core.Profiles.Cached;
using ProtonVPN.Core.Settings.Contracts;
using ProtonVPN.Dns.Contracts;

namespace ProtonVPN.Core.Settings
{
    public interface IAppSettings
    {
        event PropertyChangedEventHandler PropertyChanged;
        CachedProfileDataContract Profiles { get; set; }
        DateTime ProfileChangesSyncedAt { get; set; }
        string OvpnProtocol { get; set; }
        string Language { get; set; }
        bool AppFirstRun { get; set; }
        bool ShowNotifications { get; set; }
        WindowPlacement WindowPlacement { get; set; }
        WindowPlacement SidebarWindowPlacement { get; set; }
        double Width { get; set; }
        string QuickConnect { get; set; }
        string LastEventId { get; set; }
        bool StartOnBoot { get; set; }
        StartMinimizedMode StartMinimized { get; set; }
        bool EarlyAccess { get; set; }
        bool SecureCore { get; set; }
        string LastUpdate { get; set; }
        KillSwitchMode KillSwitchMode { get; set; }
        bool Ipv6LeakProtection { get; set; }
        bool CustomDnsEnabled { get; set; }
        bool SidebarMode { get; set; }
        bool WelcomeModalShown { get; set; }
        bool ModerateNat { get; set; }
        int OnboardingStep { get; set; }
        int AppStartCounter { get; set; }
        int SidebarTab { get; set; }
        SplitTunnelingApp[] SplitTunnelingBlockApps { get; set; }
        SplitTunnelingApp[] SplitTunnelingAllowApps { get; set; }
        IpContract[] SplitTunnelExcludeIps { get; set; }
        IpContract[] SplitTunnelIncludeIps { get; set; }
        IpContract[] CustomDnsIps { get; set; }
        int SettingsSelectedTabIndex { get; set; }
        bool SplitTunnelingEnabled { get; set; }
        SplitTunnelMode SplitTunnelMode { get; set; }
        string[] GetSplitTunnelApps();
        bool NetShieldEnabled { get; set; }
        int NetShieldMode { get; set; }
        DateTime LastPrimaryApiFailDateUtc { get; set; }
        StringCollection AlternativeApiBaseUrls { set; get; }
        string ActiveAlternativeApiBaseUrl { set; get; }
        bool DoHEnabled { get; set; }
        bool PortForwardingEnabled { get; set; }
        bool FeaturePortForwardingEnabled { get; set; }
        bool DoNotShowPortForwardingConfirmationDialog { get; set; }
        bool PortForwardingNotificationsEnabled { get; set; }
        bool PortForwardingInQuickSettings { get; set; }
        bool DoNotShowKillSwitchConfirmationDialog { get; set; }
        bool DoNotShowEnableSmartProtocolDialog { get; set; }
        bool DoNotShowDiscourageSecureCoreDialog { get; set; }
        bool FeatureSmartProtocolWireGuardEnabled { get; set; }
        IReadOnlyList<Announcement> Announcements { get; set; }
        List<IssueCategoryResponse> ReportAnIssueFormData { get; set; }
        int[] OpenVpnTcpPorts { get; set; }
        int[] OpenVpnUdpPorts { get; set; }
        int[] WireGuardPorts { get; set; }
        bool FeatureNetShieldEnabled { get; set; }
        bool FeatureMaintenanceTrackerEnabled { get; set; }
        bool FeaturePollNotificationApiEnabled { get; set; }
        TimeSpan MaintenanceCheckInterval { get; set; }
        OpenVpnAdapter NetworkAdapterType { get; set; }
        bool VpnAcceleratorEnabled { get; set; }
        bool FeatureVpnAcceleratorEnabled { get; set; }
        bool FeatureStreamingServicesLogosEnabled { get; set; }
        bool FeaturePromoCodeEnabled { get; set; }
        bool ConnectOnAppStart { get; set; }
        bool FeatureSmartReconnectEnabled { get; set; }
        bool ShowNonStandardPortsToFreeUsers { get; set; }
        bool SmartReconnectEnabled { get; set; }
        bool SmartReconnectNotificationsEnabled { get; set; }
        bool AllowNonStandardPorts { get; set; }
        string AuthenticationPublicKey { get; set; }
        string AuthenticationSecretKey { get; set; }
        string AuthenticationCertificatePem { get; set; }
        DateTimeOffset? AuthenticationCertificateExpirationUtcDate { get; set; }
        DateTimeOffset? AuthenticationCertificateRefreshUtcDate { get; set; }
        DateTimeOffset? AuthenticationCertificateRequestUtcDate { get; set; }
        string CertificationServerPublicKey { get; set; }
        string AccessToken { get; set; }
        string RefreshToken { get; set; }
        string Uid { get; set; }
        bool HardwareAccelerationEnabled { get; set; }
        bool IsToShowRebrandingPopup { get; set; }
        ConcurrentDictionary<string, DnsResponse> DnsCache { get; set; }
        bool IsNetShieldEnabled();
        bool IsPortForwardingEnabled();
        bool IsVpnAcceleratorEnabled();
        bool IsSmartReconnectEnabled();
        bool IsSmartReconnectNotificationsEnabled();
        VpnProtocol GetProtocol();
    }
}