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

using System.Diagnostics;
using System.Security.Policy;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Services.Browsing;

public class UrlsBrowser : IUrlsBrowser
{
    private readonly ILogger _logger;

    public UrlsBrowser(ILogger logger)
    {
        _logger = logger;
    }

    public string ProtocolsLearnMore => "https://protonvpn.com/blog/whats-the-best-vpn-protocol";

    public string CreateAccount => "https://account.protonvpn.com/signup?ref=windows";

    public string ResetPassword => "https://account.protonvpn.com/reset-password";

    public string ForgotUsername => "https://account.protonvpn.com/forgot-username";

    public string TroubleSigningIn => "https://protonvpn.com/support/login-problems";

    public string ProtocolChangeLearnMore => "https://protonvpn.com/support/how-to-change-vpn-protocols";

    public string ServerLoadLearnMore => "https://protonvpn.com/support/server-load-percentages-and-colors-explained";

    public string InternetSpeedLearnMore => "https://protonvpn.com/support/how-latency-bandwidth-throughput-impact-internet-speed";

    public string NatTypeLearnMore => "https://protonvpn.com/support/moderate-nat"; 

    public string CustomDnsLearnMore => "https://protonvpn.com/support/custom-dns"; 
    
    public string Ipv6LeakProtectionLearnMore => "https://protonvpn.com/support/prevent-ipv6-vpn-leaks";

    public string SupportCenter => "https://protonvpn.com/support";

    public string UsageStatisticsLearnMore => "https://protonvpn.com/support/share-usage-statistics";

    public string NetShieldLearnMore => "https://protonvpn.com/support/netshield";

    public string KillSwitchLearnMore => "https://protonvpn.com/support/what-is-kill-switch";

    public string AdvancedKillSwitchLearnMore => "https://protonvpn.com/support/advanced-kill-switch";

    public string PortForwardingLearnMore => "https://protonvpn.com/support/port-forwarding";

    public string SplitTunnelingLearnMore => "https://protonvpn.com/support/protonvpn-split-tunneling";

    public string VpnAcceleratorLearnMore => "https://protonvpn.com/support/how-to-use-vpn-accelerator";

    public string SecureCoreLearnMore => "https://protonvpn.com/support/secure-core-vpn";

    public string SmartRoutingLearnMore => "https://protonvpn.com/support/how-smart-routing-works";

    public string P2PLearnMore => "https://protonvpn.com/features/p2p-support";

    public string TorLearnMore => "https://protonvpn.com/support/tor-vpn";

    public string RpcServerProblem => "https://protonvpn.com/support/rpc-server-unavailable";

    public string Troubleshooting => "https://protonvpn.com/support/windows-vpn-issues";

    public string NoLogs => "https://protonvpn.com/blog/no-logs-audit";

    public string ProtonStatusPage => "https://protonstatus.com";

    public string P2PStatusPage => "http://protonstatus.com/vpn_status";

    public string SupportForm => "https://protonvpn.com/support-form";

    public string DownloadsPage => "https://protonvpn.com/download";

    public string IpAddressLearnMore => "https://protonvpn.com/blog/what-is-an-ip-address";

    public string IspLearnMore => "https://protonvpn.com/blog/isp";

    public string IncreaseVpnSpeeds => "https://protonvpn.com/support/increase-vpn-speeds";

    public string ActiveProxyLearnMore => "https://protonvpn.com/support/turn-off-proxy-windows";

    public string EnableBaseFilteringEngine => "https://protonvpn.com/support/how-to-enable-the-base-filtering-engine";

    public string ProfileLearnMore => "https://protonvpn.com/support/connection-profiles";

    public string TrafficLearnMore => "https://protonvpn.com/support/traffic-stats";

    public void BrowseTo(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception e)
        {
            _logger.Error<AppFileAccessFailedLog>($"Could not navigate to the requested url: {url}", e);
        }
    }
}