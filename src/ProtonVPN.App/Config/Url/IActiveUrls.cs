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

namespace ProtonVPN.Config.Url
{
    public interface IActiveUrls
    {
        IActiveUrl AccountUrl { get; }
        IActiveUrl AboutSecureCoreUrl { get; }
        IActiveUrl BfeArticleUrl { get; }
        IActiveUrl ApiUrl { get; }
        IActiveUrl TlsReportUrl { get; }
        IActiveUrl ForgetUsernameUrl { get; }
        IActiveUrl HelpUrl { get; }
        IActiveUrl P2PStatusUrl { get; }
        IActiveUrl PasswordResetUrl { get; }
        IActiveUrl ProtonMailPricingUrl { get; }
        IActiveUrl RegisterUrl { get; }
        IActiveUrl TroubleShootingUrl { get; }
        IActiveUrl DownloadUrl { get; }
        IActiveUrl PublicWifiSafetyUrl { get; }
        IActiveUrl ProtonStatusUrl { get; }
        IActiveUrl TorBrowserUrl { get; }
        IActiveUrl ProtonTwitterUrl { get; }
        IActiveUrl SupportFormUrl { get; }
        IActiveUrl AlternativeRoutingUrl { get; }
        IActiveUrl AboutNetShieldUrl { get; }
        IActiveUrl AboutKillSwitchUrl { get; }
        IActiveUrl AboutPortForwardingUrl { get; }
        IActiveUrl PortForwardingRisksUrl { get; }
        IActiveUrl AboutModerateNatUrl { get; }
        IActiveUrl InvoicesUrl { get; }
        IActiveUrl SmartRoutingUrl { get; }
        IActiveUrl StreamingUrl { get; }
        IActiveUrl P2PUrl { get; }
        IActiveUrl TorUrl { get; }
        IActiveUrl AboutSmartProtocolUrl { get; }
        IActiveUrl IncorrectSystemTimeArticleUrl { get; }
        IActiveUrl AssignVpnConnectionsUrl { get; }
        IActiveUrl NonStandardPortsUrl { get; }
        IActiveUrl LoginProblemsUrl { get; }
        IActiveUrl RebrandingUrl { get; }
        IActiveUrl RpcServerProblemUrl { get; }
    }
}