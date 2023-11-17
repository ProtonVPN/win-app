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

using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Settings.Contracts;

public interface IUserSettings
{
    string Theme { get; set; }
    int? WindowWidth { get; set; }
    int? WindowHeight { get; set; }
    int? WindowXPosition { get; set; }
    int? WindowYPosition { get; set; }
    bool IsWindowMaximized { get; set; }
    VpnProtocol VpnProtocol { get; set; }
    string? VpnPlanTitle { get; set; }
    string? AuthenticationPublicKey { get; set; }
    string? AuthenticationSecretKey { get; set; }
    string? AuthenticationCertificatePem { get; set; }
    string? CertificationServerPublicKey { get; set; }
    DateTimeOffset? AuthenticationCertificateRequestUtcDate { get; set; }
    DateTimeOffset? AuthenticationCertificateExpirationUtcDate { get; set; }
    DateTimeOffset? AuthenticationCertificateRefreshUtcDate { get; set; }
    NatType NatType { get; set; }
    bool IsVpnAcceleratorEnabled { get; set; }
    bool IsNotificationEnabled { get; set; }
    bool IsBetaAccessEnabled { get; set; }
    bool IsShareStatisticsEnabled { get; set; }
    bool IsShareCrashReportsEnabled { get; set; }
    bool IsAlternativeRoutingEnabled { get; set; }
    bool IsCustomDnsServersEnabled { get; set; }
    List<CustomDnsServer> CustomDnsServersList { get; set; }
    bool IsAutoConnectEnabled { get; set; }
    AutoConnectMode AutoConnectMode { get; set; }
    bool IsNetShieldEnabled { get; set; }
    bool IsPortForwardingEnabled { get; set; }
    bool IsPortForwardingNotificationEnabled { get; set; }
    bool IsSplitTunnelingEnabled { get; set; }
    SplitTunnelingMode SplitTunnelingMode { get; set; }
    List<SplitTunnelingApp> SplitTunnelingStandardAppsList { get; set; }
    List<SplitTunnelingApp> SplitTunnelingInverseAppsList { get; set; }
    List<SplitTunnelingIpAddress> SplitTunnelingStandardIpAddressesList { get; set; }
    List<SplitTunnelingIpAddress> SplitTunnelingInverseIpAddressesList { get; set; }
}