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

namespace ProtonVPN.Client.Models.Urls;

public class Urls : IUrls
{
    public string ProtocolsLearnMore => "https://protonvpn.com/blog/whats-the-best-vpn-protocol/";

    public string CreateAccount => "https://account.protonvpn.com/signup";

    public string ResetPassword => "https://account.protonvpn.com/reset-password";

    public string ForgotUsername => "https://account.protonvpn.com/forgot-username";

    public string TroubleSigningIn => "https://protonvpn.com/support/login-problems";

    public string ProtocolChangeLearnMore => "https://protonvpn.com/support/how-to-change-vpn-protocols/";

    public string ServerLoadLearnMore => "https://protonvpn.com/support/server-load-percentages-and-colors-explained/";

    public string InternetSpeedLearnMore => "https://protonvpn.com/support/how-latency-bandwidth-throughput-impact-internet-speed/";

    public string NatTypeLearnMore => "https://protonvpn.com/support/moderate-nat/";

    public string SupportCenter => "https://protonvpn.com/support/";

    public string UsageStatisticsLearnMore => "https://protonvpn.com/support/share-usage-statistics/";

    public void NavigateTo(string url)
    {
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }
}