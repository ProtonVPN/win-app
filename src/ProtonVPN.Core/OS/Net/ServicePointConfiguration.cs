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

using System;
using System.Net;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ApiLogs;

namespace ProtonVPN.Core.OS.Net
{
    public class ServicePointConfiguration
    {
        private readonly ILogger _logger;

        public ServicePointConfiguration(ILogger logger)
        {
            _logger = logger;
        }

        public void Apply()
        {
            ConfigureSSlSecurity();
        }

        /// <summary>
        /// Forces TLS 1.2 on Windows 7 SP1 and Windows Server 2008 R2.
        /// </summary>
        /// <remarks>
        /// On Windows 7 SP1 and Windows Server 2008 R2 only TLS 1.0 is enabled by default, unless a specific security update is installed.
        /// TLS 1.1 and 1.2 are also supported, but not enabled by default.
        /// </remarks>
        private void ConfigureSSlSecurity()
        {
            Version osVer = Environment.OSVersion.Version;
            if (osVer.Major == 6 && osVer.Minor == 1)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                _logger.Info<ApiLog>($"Security protocol set to 'TLS 1.2' due to operative system version '{osVer}'.");
            }
            else
            {
                _logger.Info<ApiLog>($"Security protocol kept at '{ServicePointManager.SecurityProtocol}'. " +
                                     $"Operative system version '{osVer}'.");
            }
        }
    }
}
