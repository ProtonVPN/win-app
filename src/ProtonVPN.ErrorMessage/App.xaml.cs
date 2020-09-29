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

using System.Windows;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.OS.Processes;

namespace ProtonVPN.ErrorMessage
{
    public partial class App
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            var config = new ConfigFactory().Config();
            var osProcesses = new SystemProcesses(new NullLogger());
            var productCode = GetProductCode();
            var installerPath = new InstallerPath(productCode, config);
            var repairLauncher = new RepairLauncher(osProcesses, productCode);
            var vm = new MainWindowViewModel(config, repairLauncher, osProcesses, installerPath);
            var window = new MainWindow(vm);
            window.Show();
        }

        private string GetProductCode()
        {
            var productCode = new ProductCode("ProtonVPN");
            return productCode.Value();
        }
    }
}
