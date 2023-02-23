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

using ProtonVPN.Common.OS.Processes;

namespace ProtonVPN.Update.Files.Launchable
{
    /// <summary>
    /// Starts new process requesting elevation.
    /// </summary>
    public class LaunchableFile : ILaunchableFile
    {
        private readonly IOsProcesses _processes;

        public LaunchableFile(IOsProcesses processes)
        {
            _processes = processes;
        }

        public void Launch(string filename, string arguments)
        {
            _processes.ElevatedProcess(filename, arguments)
                .Start();
        }
    }
}
