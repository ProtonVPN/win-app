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

using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Core.Update;

namespace ProtonVPN.Modals
{
    public class RebootModalViewModel : BaseModalViewModel, IUpdateStateAware
    {
        private UpdateStateChangedEventArgs _updateState;
        private readonly IOsProcesses _osProcesses;

        public RebootModalViewModel(IOsProcesses osProcesses)
        {
            _osProcesses = osProcesses;
            RebootCommand = new RelayCommand(RebootAction);
            SkipRebootCommand = new RelayCommand(SkipRebootAction);
        }

        public ICommand SkipRebootCommand { get; }

        public ICommand RebootCommand { get; }

        public void OnUpdateStateChanged(UpdateStateChangedEventArgs e)
        {
            _updateState = e;
        }

        private void RebootAction()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce", true);
            key?.SetValue("", _updateState.FilePath);

            _osProcesses.CommandLineProcess("/C shutdown -f -r -t 0").Start();
        }

        private void SkipRebootAction()
        {
            TryClose(true);
        }
    }
}
