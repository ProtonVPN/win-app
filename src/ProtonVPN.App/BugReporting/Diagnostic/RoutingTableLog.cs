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
using ProtonVPN.Common.OS.Processes;

namespace ProtonVPN.BugReporting.Diagnostic
{
    public class RoutingTableLog : BaseLog
    {
        private readonly IOsProcesses _osProcesses;

        public RoutingTableLog(IOsProcesses osProcesses, IConfiguration config) 
            : base(config.DiagnosticsLogFolder, "RoutingTable.txt")
        {
            _osProcesses = osProcesses;
        }

        public override void Write()
        {
            File.WriteAllText(Path, Content);
        }

        private string Content
        {
            get
            {
                using IOsProcess process = _osProcesses.CommandLineProcess("/c route print");
                process.Start();
                return process.StandardOutput.ReadToEnd();
            }
        }
    }
}
