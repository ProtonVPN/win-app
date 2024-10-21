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

namespace ProtonVPN.Client.Services.Browsing;

public interface IUrls
{
    string ProtocolsLearnMore { get; }
    string CreateAccount { get; }
    string ResetPassword { get; }
    string ForgotUsername { get; }
    string TroubleSigningIn { get; }
    string ProtocolChangeLearnMore { get; }
    string ServerLoadLearnMore { get; }
    string InternetSpeedLearnMore { get; }
    string NatTypeLearnMore { get; }
    string SupportCenter { get; }
    string UsageStatisticsLearnMore { get; }
    string NetShieldLearnMore { get; }
    string KillSwitchLearnMore { get; }
    string PortForwardingLearnMore { get; }
    string SplitTunnelingLearnMore { get; }
    string VpnAcceleratorLearnMore { get; }
    string SecureCoreLearnMore { get; }
    string SmartRoutingLearnMore { get; }
    string P2PLearnMore { get; }
    string TorLearnMore { get; }
    string RpcServerProblem { get; }
    string Troubleshooting { get; }
    string ProtonStatusPage { get; }
    string SupportForm { get; }
    string NoLogs { get; }
    string DownloadsPage { get; }

    void NavigateTo(string url);
}