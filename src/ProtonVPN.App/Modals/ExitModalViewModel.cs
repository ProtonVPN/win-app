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
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Modals
{
    public class ExitModalViewModel : BaseModalViewModel, IVpnStateAware, IServiceSettingsStateAware
    {
        private readonly IAppSettings _appSettings;

        private bool _networkBlocked;
        private bool _disconnected;

        public ExitModalViewModel(IAppSettings appSettings)
        {
            _appSettings = appSettings;
            ExitCommand = new RelayCommand(TryCloseWithSuccess);
            CancelCommand = new RelayCommand(TryCloseWithFailure);
        }

        public ICommand ExitCommand { get; }
        public ICommand CancelCommand { get; }

        public KillSwitchMode KillSwitchMode => _appSettings.KillSwitchMode;

        public bool NetworkBlocked
        {
            get => _networkBlocked;
            set => Set(ref _networkBlocked, value);
        }
        public bool Disconnected
        {
            get => _disconnected;
            set => Set(ref _disconnected, value);
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            NetworkBlocked = e.NetworkBlocked;
            Disconnected = e.State.Status == VpnStatus.Disconnected;

            return Task.CompletedTask;
        }

        public void OnServiceSettingsStateChanged(ServiceSettingsStateChangedEventArgs e)
        {
            NetworkBlocked = e.IsNetworkBlocked;
        }
    }
}