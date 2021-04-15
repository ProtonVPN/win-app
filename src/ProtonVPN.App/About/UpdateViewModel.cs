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
using ProtonVPN.BugReporting.Diagnostic;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Service.Settings;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Update;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals;
using ProtonVPN.Translations;

namespace ProtonVPN.About
{
    public class UpdateViewModel : ViewModel, IUpdateStateAware, IVpnStateAware
    {
        private readonly IDialogs _dialogs;
        private readonly IOsProcesses _osProcesses;
        private readonly IModals _modals;
        private readonly IAppSettings _appSettings;
        private readonly IVpnServiceManager _vpnServiceManager;
        private readonly ISystemState _systemState;
        private readonly ISettingsServiceClientManager _settingsServiceClientManager;

        private UpdateStateChangedEventArgs _updateStateChangedEventArgs;
        private VpnStatus _vpnStatus;

        public UpdateViewModel(
            IDialogs dialogs,
            IOsProcesses osProcesses,
            IModals modals,
            IAppSettings appSettings,
            IVpnServiceManager vpnServiceManager,
            ISystemState systemState,
            ISettingsServiceClientManager settingsServiceClientManager)
        {
            _dialogs = dialogs;
            _osProcesses = osProcesses;
            _modals = modals;
            _appSettings = appSettings;
            _vpnServiceManager = vpnServiceManager;
            _systemState = systemState;
            _settingsServiceClientManager = settingsServiceClientManager;

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

        private async void Update()
        {
            if (!CanUpdate() || !AllowToDisconnect())
            {
                return;
            }

            if (_systemState.PendingReboot())
            {
                bool? result = _modals.Show<RebootModalViewModel>();
                if (result.HasValue && result.Value)
                {
                    await UpdateInternal();
                }

                return;
            }

            await UpdateInternal();
        }

        private async Task UpdateInternal()
        {
            Updating = true;

            if (_appSettings.KillSwitchMode == KillSwitchMode.Hard)
            {
                await _settingsServiceClientManager.DisableKillSwitch();
            }

            try
            {
                _osProcesses.ElevatedProcess(
                    _updateStateChangedEventArgs.FilePath,
                    _updateStateChangedEventArgs.FileArguments).Start();

                Application.Current.Shutdown();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                if (_appSettings.KillSwitchMode == KillSwitchMode.Hard)
                {
                    await _settingsServiceClientManager.EnableHardKillSwitch();
                }

                // Privileges were not granted
                Updating = false;
            }
        }

        private bool CanUpdate() => !Updating && Ready;

        private bool AllowToDisconnect()
        {
            if (ShowConfirmationModal())
            {
                bool? result = _dialogs.ShowQuestion(Translation.Get("App_msg_UpdateConnectedConfirm"));
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

        private bool ShowConfirmationModal()
        {
            return (_vpnStatus != VpnStatus.Disconnected && _vpnStatus != VpnStatus.Disconnecting) ||
                   _appSettings.KillSwitchMode == KillSwitchMode.Hard;
        }
    }
}