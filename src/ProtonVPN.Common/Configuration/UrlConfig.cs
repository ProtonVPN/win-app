/*
 * Copyright (c) 2020 Proton Technologies AG
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
        public string BfeArticleUrl { get; internal set; }

        [Required]
        public string PasswordResetUrl { get; internal set; }

        [Required]
        public string ForgetUsernameUrl { get; internal set; }
        
        [Required]
        public string UpdateUrl { get; internal set; }

        [Required]
        public string DownloadUrl { get; internal set; }

        [Required]
        public string ApiUrl { get; internal set; }
        
        [Required]
        public string TlsReportUrl { get; internal set; }
        
        [Required]
        public string HelpUrl { get; internal set; }

        [Required]
        public string AccountUrl { get; internal set; }

        [Required]
        public string AboutSecureCoreUrl { get; internal set; }

        [Required]
        public string RegisterUrl { get; internal set; }

        [Required]
        public string TroubleShootingUrl { get; internal set; }

        [Required]
        public string P2PStatusUrl { get; internal set; }

        [Required]
        public string ProtonMailPricingUrl { get; internal set; }

        [Required]
        public string PublicWifiSafetyUrl { get; internal set; }

        [Required]
        public string ProtonStatusUrl { get; internal set; }

        [Required]
        public string TorBrowserUrl { get; internal set; }

        [Required]
        public string ProtonTwitterUrl { get; internal set; }

        [Required]
        public string SupportFormUrl { get; internal set; }

        [Required]
        public string AlternativeRoutingUrl { get; internal set; }

        [Required]
        public string AboutNetShieldUrl { get; internal set; }

        [Required]
        public string AboutKillSwitchUrl { get; internal set; }

        [Required]
        public string AboutPortForwardingUrl { get; internal set; }

        [Required]
        public string PortForwardingRisksUrl { get; internal set; }

        [Required]
        public string AboutDelinquencyUrl { get; internal set; }

        [Required]
        public string InvoicesUrl { get; internal set; }
    }
}
