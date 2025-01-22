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
using ProtonVPN.Configurations.Entities;

namespace ProtonVPN.Configurations.Defaults;

public static class DefaultUrlsConfigurationFactory
{
    public static IUrlsConfiguration Create()
    {
        return new UrlsConfiguration()
        {
            ApiUrl = "https://vpn-api.proton.me",
            BfeArticleUrl = "https://protonvpn.com/support/how-to-enable-the-base-filtering-engine",
            PasswordResetUrl = "https://account.protonvpn.com/reset-password",
            ForgetUsernameUrl = "https://account.protonvpn.com/forgot-username",
            UpdateUrl = "https://protonvpn.com/download/windows/{0}/v1/version.json",
            DownloadUrl = "https://protonvpn.com/download",
            TlsReportUrl = "https://reports.protonmail.ch/reports/tls",
            HelpUrl = "https://protonvpn.com/support/",
            AutoLoginBaseUrl = "https://account.proton.me/lite",
            AccountUrl = "https://account.protonvpn.com/account",
            AboutSecureCoreUrl = "https://protonvpn.com/support/secure-core-vpn",
            RegisterUrl = "https://account.protonvpn.com/signup",
            TroubleShootingUrl = "https://protonvpn.com/support/windows-vpn-issues",
            P2PStatusUrl = "http://protonstatus.com/vpn_status",
            ProtonMailPricingUrl = "https://protonmail.com/pricing",
            PublicWifiSafetyUrl = "https://protonvpn.com/blog/public-wifi-safety",
            ProtonStatusUrl = "https://protonstatus.com",
            TorBrowserUrl = "https://www.torproject.org",
            ProtonTwitterUrl = "https://twitter.com/ProtonVPN",
            SupportFormUrl = "https://protonvpn.com/support-form",
            AlternativeRoutingUrl = "https://protonmail.com/blog/anti-censorship-alternative-routing",
            AboutKillSwitchUrl = "https://protonvpn.com/support/what-is-kill-switch",
            AboutNetShieldUrl = "https://protonvpn.com/support/netshield",
            AboutPortForwardingUrl = "https://protonvpn.com/support/port-forwarding",
            PortForwardingRisksUrl = "https://protonvpn.com/support/port-forwarding-risks",
            AboutModerateNatUrl = "https://protonvpn.com/support/moderate-nat",
            StreamingUrl = "https://protonvpn.com/support/streaming-guide/",
            SmartRoutingUrl = "https://protonvpn.com/support/smart-routing",
            P2PUrl = "https://protonvpn.com/support/bittorrent-vpn/",
            TorUrl = "https://protonvpn.com/support/tor-vpn/",
            InvoicesUrl = "https://account.protonvpn.com/payments#invoices",
            AboutSmartProtocolUrl = "https://protonvpn.com/support/how-to-change-vpn-protocols",
            IncorrectSystemTimeArticleUrl = "https://protonvpn.com/support/update-windows-clock",
            EnableVpnConnectionsUrl = "https://protonvpn.com/support/enable-vpn-connection",
            LoginProblemsUrl = "https://protonvpn.com/support/login-problems",
            RebrandingUrl = "https://protonvpn.com/blog/updated-proton-vpn",
            RpcServerProblemUrl = "https://protonvpn.com/support/rpc-server-unavailable",
        };
    }
}