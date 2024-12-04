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

using ProtonVPN.Configurations.Contracts.Entities;

namespace ProtonVPN.Configurations.Entities;

public class UrlsConfiguration : IUrlsConfiguration
{
    public string ApiUrl { get; set; } = string.Empty;
    public string BfeArticleUrl { get; set; } = string.Empty;
    public string PasswordResetUrl { get; set; } = string.Empty;
    public string ForgetUsernameUrl { get; set; } = string.Empty;
    public string UpdateUrl { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string TlsReportUrl { get; set; } = string.Empty;
    public string HelpUrl { get; set; } = string.Empty;
    public string AutoLoginBaseUrl { get; set; } = string.Empty;
    public string AccountUrl { get; set; } = string.Empty;
    public string AboutSecureCoreUrl { get; set; } = string.Empty;
    public string RegisterUrl { get; set; } = string.Empty;
    public string TroubleShootingUrl { get; set; } = string.Empty;
    public string P2PStatusUrl { get; set; } = string.Empty;
    public string ProtonMailPricingUrl { get; set; } = string.Empty;
    public string PublicWifiSafetyUrl { get; set; } = string.Empty;
    public string ProtonStatusUrl { get; set; } = string.Empty;
    public string TorBrowserUrl { get; set; } = string.Empty;
    public string ProtonTwitterUrl { get; set; } = string.Empty;
    public string SupportFormUrl { get; set; } = string.Empty;
    public string AlternativeRoutingUrl { get; set; } = string.Empty;
    public string AboutKillSwitchUrl { get; set; } = string.Empty;
    public string AboutNetShieldUrl { get; set; } = string.Empty;
    public string AboutPortForwardingUrl { get; set; } = string.Empty;
    public string PortForwardingRisksUrl { get; set; } = string.Empty;
    public string AboutModerateNatUrl { get; set; } = string.Empty;
    public string StreamingUrl { get; set; } = string.Empty;
    public string SmartRoutingUrl { get; set; } = string.Empty;
    public string P2PUrl { get; set; } = string.Empty;
    public string TorUrl { get; set; } = string.Empty;
    public string InvoicesUrl { get; set; } = string.Empty;
    public string AboutSmartProtocolUrl { get; set; } = string.Empty;
    public string IncorrectSystemTimeArticleUrl { get; set; } = string.Empty;
    public string EnableVpnConnectionsUrl { get; set; } = string.Empty;
    public string LoginProblemsUrl { get; set; } = string.Empty;
    public string RebrandingUrl { get; set; } = string.Empty;
    public string RpcServerProblemUrl { get; set; } = string.Empty;
}