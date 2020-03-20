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

using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Update;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Resources;

namespace ProtonVPN.About
{
    public class UpdateViewModel : ViewModel, IUpdateStateAware, IVpnStateAware
    {
        private readonly IDialogs _dialogs;
        private readonly IOsProcesses _osProcesses;
        private readonly IModals _modals;

        private VpnStatus _vpnStatus;
        private UpdateStateChangedEventArgs _updateStateChangedEventArgs;
        public UpdateViewModel(IDialogs dialogs, IOsProcesses osProcesses, IModals modals)
        {
            _dialogs = dialogs;
            _osProcesses = osProcesses;
            _modals = modals;

            OpenAboutCommand = new RelayCommand(OpenAbout);
        }

        private RelayCommand _updateCommand;
        public ICommand UpdateCommand => _updateCommand ??= new RelayCommand(Update, CanUpdate);

        public ICommand OpenAboutCommand { get; }

        private UpdateStatus _status;
        public UpdateStatus Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        private bool _available;
        public bool Available
        {
            get => _available;
            set => Set(ref _available, value);
        }

        private bool _ready;
        public bool Ready
        {
            get => _ready;
            set
            {
                Set(ref _ready, value);
                _updateCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool _updating;
        public bool Updating
        {
            get => _updating;
            set
            {
                Set(ref _updating, value);
                _updateCommand?.RaiseCanExecuteChanged();
            }
        }

        private Release _release;
        public Release Release
        {
            get => _release;
            set => Set(ref _release, value);
        }

        public void OnUpdateStateChanged(UpdateStateChangedEventArgs e)
        {
            _updateStateChangedEventArgs = e;
            Status = e.Status;
            Available = e.Available;
            Ready = e.Ready;
            Release = e.ReleaseHistory.FirstOrDefault();
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;
            return Task.CompletedTask;
        }

        private void Update()
        {
            if (!CanUpdate() || !AllowToDisconnect())
            {
                return;
            }

            Updating = true;

            try
            {
                _osProcesses.ElevatedProcess(
                    _updateStateChangedEventArgs.FilePath,
                    _updateStateChangedEventArgs.FileArguments).Start();


                Application.Current.Shutdown();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Privileges were not granted
                Updating = false;
            }
        }

        private bool CanUpdate() => !Updating && Ready;

        private bool AllowToDisconnect()
        {
            if (_vpnStatus != VpnStatus.Disconnected && _vpnStatus != VpnStatus.Disconnecting)
            {
                var result = _dialogs.ShowQuestion(StringResources.Get("App_msg_UpdateConnectedConfirm"));
                if (result.HasValue && result.Value == false)
                {
                    return false;
                }
            }

            return true;
        }

        private void OpenAbout()
        {
            dynamic options = new ExpandoObject();
            options.SkipUpdateCheck = true;
            _modals.Show<AboutModalViewModel>(options);
        }
    }
}
