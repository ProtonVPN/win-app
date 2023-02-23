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
    public class OpenVpnConfig
    {
        [Required]
        public string ExePath { get; set; }

        [Required]
        public string ConfigPath { get; set; }

        [Required]
        public string TlsVerifyExePath { get; set; }

        [Required]
        public string TlsExportCertFolder { get; set; }

        [Required]
        public string ExitEventName { get; set; }

        [Required]
        public string ManagementHost { get; set; }

        [Required]
        public byte[] OpenVpnStaticKey { get; set; }

        [Required]
        public string TapAdapterId { get; set; }

        [Required]
        public string TapAdapterDescription { get; set; }

        [Required]
        public string TunAdapterId { get; set; }

        [Required]
        public string TunAdapterName { get; set; }
    }
}