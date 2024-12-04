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

namespace ProtonVPN.Configurations.Contracts.Entities;

public interface IUrlsConfiguration
{
    string ApiUrl { get; }
    string BfeArticleUrl { get; }
    string PasswordResetUrl { get; }
    string ForgetUsernameUrl { get; }
    string UpdateUrl { get; }
    string DownloadUrl { get; }
    string TlsReportUrl { get; }
    string HelpUrl { get; }
    string AutoLoginBaseUrl { get; }
    string AccountUrl { get; }
    string AboutSecureCoreUrl { get; }
    string RegisterUrl { get; }
    string TroubleShootingUrl { get; }
    string P2PStatusUrl { get; }
    string ProtonMailPricingUrl { get; }
    string PublicWifiSafetyUrl { get; }
    string ProtonStatusUrl { get; }
    string TorBrowserUrl { get; }
    string ProtonTwitterUrl { get; }
    string SupportFormUrl { get; }
    string AlternativeRoutingUrl { get; }
    string AboutKillSwitchUrl { get; }
    string AboutNetShieldUrl { get; }
    string AboutPortForwardingUrl { get; }
    string PortForwardingRisksUrl { get; }
    string AboutModerateNatUrl { get; }
    string StreamingUrl { get; }
    string SmartRoutingUrl { get; }
    string P2PUrl { get; }
    string TorUrl { get; }
    string InvoicesUrl { get; }
    string AboutSmartProtocolUrl { get; }
    string IncorrectSystemTimeArticleUrl { get; }
    string EnableVpnConnectionsUrl { get; }
    string LoginProblemsUrl { get; }
    string RebrandingUrl { get; }
    string RpcServerProblemUrl { get; }
}