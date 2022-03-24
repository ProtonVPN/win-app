/*
 * Copyright (c) 2022 Proton Technologies AG
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

using System.ComponentModel;
using System.Windows;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.OS.Processes;

namespace ProtonVPN.ErrorMessage
{
    internal class MainWindowViewModel : BaseWindowViewModel
    {
        private readonly Config _config;
        private readonly RepairLauncher _repairLauncher;
        private readonly IOsProcesses _processes;
        private readonly InstallerPath _installerPath;

        public MainWindowViewModel(
            Config config,
            RepairLauncher repairLauncher,
            IOsProcesses processes,
            InstallerPath installerPath)
        {
            _installerPath = installerPath;
            _processes = processes;
            _repairLauncher = repairLauncher;
            _config = config;
        }

        public bool RepairAvailable => _installerPath.Exists();

        public void Repair()
        {
            try
            {
                _repairLauncher.Repair();
            }
            catch (Win32Exception)
            {
                //No permission? Ignore.
            }
        }

        public void Download()
        {
            _processes.Open(_config.Urls.DownloadUrl);
            Application.Current.Shutdown();
        }
    }
}
