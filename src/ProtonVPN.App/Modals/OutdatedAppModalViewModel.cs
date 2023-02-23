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

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.About;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Update;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Modals
{
    public class OutdatedAppModalViewModel : BaseModalViewModel, IVpnStateAware
    {
        private readonly UpdateService _appUpdater;
        private readonly IActiveUrls _urls;

        public OutdatedAppModalViewModel(UpdateViewModel updateViewModel, UpdateService appUpdater, IActiveUrls urls)
        {
            HideWindowControls = true;
            Update = updateViewModel;
            UpdateManuallyCommand = new RelayCommand(UpdateManuallyAction);
            _appUpdater = appUpdater;
            _urls = urls;
        }

        public UpdateViewModel Update { get; }

        public ICommand UpdateManuallyCommand { get; }

        private bool _killSwitchEnabled;
        public bool KillSwitchEnabled
        {
            get => _killSwitchEnabled;
            set => Set(ref _killSwitchEnabled, value);
        }

        public override void BeforeOpenModal(dynamic options)
        {
            _appUpdater.StartCheckingForUpdate();
        }

        public override void CloseAction()
        {
            Application.Current.Shutdown();
        }

        protected override void OnDeactivate(bool close)
        {
            CloseAction();
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            KillSwitchEnabled = e.NetworkBlocked;
            return Task.CompletedTask;
        }

        private void UpdateManuallyAction()
        {
            _urls.DownloadUrl.Open();
            CloseAction();
        }
    }
}
