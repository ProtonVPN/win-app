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

using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Core;
using ProtonVPN.Core.MVVM;

namespace ProtonVPN.ErrorHandling
{
    public class ErrorWindowViewModel : ViewModel
    {
        private const string PRODUCT_NAME = "ProtonVPN";

        private readonly IConfiguration _config;
        private readonly IOsProcesses _processes;
        private readonly IAppExitInvoker _appExitInvoker;
        private readonly InstallerPath _installerPath;
        private readonly RepairLauncher _repairLauncher;
        private readonly InstalledAppRegistry _installedAppRegistry;
        private string ProductCode => _installedAppRegistry.GetAppProductCode(PRODUCT_NAME);

        public ICommand CloseCommand { get; }
        public bool RepairAvailable => _installerPath.Exists(ProductCode);

        private string _errorDescription;
        public string ErrorDescription
        {
            get => _errorDescription;
            set
            {
                _errorDescription = value;
                OnPropertyChanged(nameof(ErrorDescription));
            }
        }

        public ErrorWindowViewModel()
        {
            _appExitInvoker = new AppExitInvoker();
            _processes = new SystemProcesses(new NullLogger());
            _config = new ConfigFactory().Config();
            _installerPath = new InstallerPath(_config);
            _repairLauncher = new RepairLauncher(_processes);
            _installedAppRegistry = new InstalledAppRegistry();
            CloseCommand = new RelayCommand(CloseAction);
        }

        public void OpenDownloadUrlAction()
        {
            _processes.Open(_config.Urls.DownloadUrl);
        }

        public void Repair()
        {
            try
            {
                _repairLauncher.Repair(ProductCode);
                CloseAction();
            }
            catch (Win32Exception)
            {
                //No permission? Ignore.
            }
        }

        private void CloseAction()
        {
            _appExitInvoker.Exit();
        }
    }
}