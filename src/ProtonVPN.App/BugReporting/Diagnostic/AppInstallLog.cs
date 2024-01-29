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

using System.IO;
using ProtonVPN.Common.Configuration;

namespace ProtonVPN.BugReporting.Diagnostic
{
    public class AppInstallLog : BaseLog
    {
        private readonly IConfiguration _configuration;

        public AppInstallLog(IConfiguration configuration) : base(configuration.DiagnosticsLogFolder, "Install.log.txt")
        {
            _configuration = configuration;
        }

        public override void Write()
        {
            if (File.Exists(_configuration.AppInstallLogPath))
            {
                File.Copy(_configuration.AppInstallLogPath, Path, true);
            }
        }
    }
}