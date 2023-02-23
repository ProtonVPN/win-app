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

using System.ComponentModel.DataAnnotations;

namespace ProtonVPN.Common.Configuration
{
    public class UrlConfig
    {
        [Required]
        public string BfeArticleUrl { get; set; }

        [Required]
        public string PasswordResetUrl { get; set; }

        [Required]
        public string ForgetUsernameUrl { get; set; }

        [Required]
        public string OldUpdateUrl { get; set; }

        [Required]
        public string UpdateUrl { get; set; }

        [Required]
        public string DownloadUrl { get; set; }

        [Required]
        public string ApiUrl { get; set; }
        
        [Required]
        public string TlsReportUrl { get; set; }
        
        [Required]
        public string HelpUrl { get; set; }

        [Required]
        public string AccountUrl { get; set; }

        [Required]
        public string AboutSecureCoreUrl { get; set; }

        [Required]
        public string RegisterUrl { get; set; }

        [Required]
        public string TroubleShootingUrl { get; set; }

        [Required]
        public string P2PStatusUrl { get; set; }

        [Required]
        public string ProtonMailPricingUrl { get; set; }

        [Required]
        public string PublicWifiSafetyUrl { get; set; }

        [Required]
        public string ProtonStatusUrl { get; set; }

        [Required]
        public string TorBrowserUrl { get; set; }

        [Required]
        public string ProtonTwitterUrl { get; set; }

        [Required]
        public string SupportFormUrl { get; set; }

        [Required]
        public string AlternativeRoutingUrl { get; set; }

        [Required]
        public string AboutNetShieldUrl { get; set; }

        [Required]
        public string AboutKillSwitchUrl { get; set; }

        [Required]
        public string AboutPortForwardingUrl { get; set; }

        [Required]
        public string PortForwardingRisksUrl { get; set; }

        [Required]
        public string AboutModerateNatUrl { get; set; }

        [Required]
        public string InvoicesUrl { get; set; }

        [Required]
        public string StreamingUrl { get; set; }

        [Required]
        public string P2PUrl { get; set; }

        [Required]
        public string SmartRoutingUrl { get; set; }

        [Required]
        public string TorUrl { get; set; }

        [Required]
        public string AboutSmartProtocolUrl { get; set; }

        [Required]
        public string IncorrectSystemTimeArticleUrl { get; set; }

        [Required]
        public string AssignVpnConnectionsUrl { get; set; }

        [Required]
        public string NonStandardPortsUrl { get; set; }

        [Required]
        public string LoginProblemsUrl { get; set; }

        [Required]
        public string RebrandingUrl { get; set; }

        [Required]
        public string RpcServerProblemUrl { get; set; }
    }
}