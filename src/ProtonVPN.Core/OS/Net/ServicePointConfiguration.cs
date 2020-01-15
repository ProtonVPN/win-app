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

using System;
using System.Net;

namespace ProtonVPN.Core.OS.Net
{
    public class ServicePointConfiguration
    {
        public void Apply()
        {
            ConfigureSSlSecurity();
        }

        /// <summary>
        /// Forces TLS 1.2 on Windows 7 SP1 and Windows Server 2008 R2.
        /// </summary>
        /// <remarks>
        /// On Windows 7 SP1 and Windows Server 2008 R2 only TLS 1.0 is enabled by default.
        /// TLS 1.1 and 1.2 are also supported, but not enabled by default.
        /// </remarks>
        private static void ConfigureSSlSecurity()
        {
            var osVer = Environment.OSVersion.Version;
            if (osVer.Major == 6 && osVer.Minor == 1)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }
        }
    }
}
