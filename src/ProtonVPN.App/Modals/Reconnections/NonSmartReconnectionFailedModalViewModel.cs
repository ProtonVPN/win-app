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

using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Modals.Dialogs;
using ProtonVPN.Settings;

namespace ProtonVPN.Modals.Reconnections
{
    public class NonSmartReconnectionFailedModalViewModel : QuestionModalViewModel
    {
        private readonly IModals _modals;
        private readonly IVpnManager _vpnManager;
        protected readonly IActiveUrls Urls;

        public NonSmartReconnectionFailedModalViewModel(IModals modals, 
            IVpnManager vpnManager, 
            IActiveUrls urls)
        {
            _modals = modals;
            _vpnManager = vpnManager;
            Urls = urls;

            OpenSettingsCommand = new RelayCommand(OpenSettingsAction);
        }

        public ICommand OpenSettingsCommand { get; }

        private void OpenSettingsAction()
        {
            base.ContinueAction();
            _modals.Show<SettingsModalViewModel>();
        }

        protected override async void ContinueAction()
        {
            base.ContinueAction();
            await _vpnManager.QuickConnectAsync();
        }
    }
}
