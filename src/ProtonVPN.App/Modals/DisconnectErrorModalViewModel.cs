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

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Modals
{
    public class DisconnectErrorModalViewModel : BaseModalViewModel
    {
        private readonly ILogger _logger;
        private readonly IActiveUrls _urlConfig;
        private readonly IAppSettings _appSettings;
        private readonly IVpnManager _vpnManager;
        private readonly IUserStorage _userStorage;

        private VpnError _error;
        private bool _networkBlocked;

        public ICommand OpenHelpArticleCommand { get; set; }
        public ICommand DisableKillSwitchCommand { get; set; }
        public ICommand GoToAccountCommand { get; set; }
        public ICommand UpgradeCommand { get; set; }

        public DisconnectErrorModalViewModel(
            ILogger logger,
            IActiveUrls urlConfig,
            IAppSettings appSettings,
            IVpnManager vpnManager,
            IUserStorage userStorage)
        {
            _logger = logger;
            _urlConfig = urlConfig;
            _appSettings = appSettings;
            _vpnManager = vpnManager;
            _userStorage = userStorage;

            OpenHelpArticleCommand = new RelayCommand(OpenHelpArticleAction);
            DisableKillSwitchCommand = new RelayCommand(DisableKillSwitch);
            GoToAccountCommand = new RelayCommand(OpenAccountPage);
            UpgradeCommand = new RelayCommand(UpgradeAction);
        }

        public VpnError Error
        {
            get => _error;
            set => Set(ref _error, value);
        }

        public bool ShowUpgrade => Error == VpnError.SessionLimitReached &&
                                   _userStorage.User().MaxTier < ServerTiers.Plus;

        public bool NetworkBlocked
        {
            get => _networkBlocked;
            set => Set(ref _networkBlocked, value);
        }

        public override void BeforeOpenModal(dynamic options)
        {
            if (options == null)
            {
                return;
            }

            NetworkBlocked = options.NetworkBlocked;
            Error = options.Error;

            _logger.Info($"Disconnected due to: {Error}. Network blocked: {NetworkBlocked}");
        }

        protected override async void OnViewReady(object view)
        {
            base.OnViewReady(view);

            switch (Error)
            {
                case VpnError.TimeoutError:
                case VpnError.UserTierTooLowError:
                case VpnError.Unpaid:
                case VpnError.SessionLimitReached:
                case VpnError.PasswordChanged:
                case VpnError.Unknown:
                    await ReconnectAsync();
                    break;
                case VpnError.ServerOffline:
                case VpnError.ServerRemoved:
                    await ReconnectWithoutLastServerAsync();
                    break;
            }
        }

        private async Task ReconnectAsync()
        {
            await CloseModalAsync();
            await _vpnManager.ReconnectAsync(new VpnReconnectionSettings() { IsToReconnectIfDisconnected = true });
        }

        private async Task CloseModalAsync()
        {
            // If TryClose() is called before any await (that actually awaits), Caliburn will throw a NullReferenceException after OnViewReady() ends
            await Task.Delay(TimeSpan.FromMilliseconds(1));
            TryClose(true);
        }

        private async Task ReconnectWithoutLastServerAsync()
        {
            await CloseModalAsync();
            await _vpnManager.ReconnectAsync(new VpnReconnectionSettings() { IsToReconnectIfDisconnected = true, IsToExcludeLastServer = true });
        }

        private void OpenHelpArticleAction()
        {
            _urlConfig.TroubleShootingUrl.Open();
        }

        private void OpenAccountPage()
        {
            _urlConfig.AccountUrl.Open();
        }

        private void DisableKillSwitch()
        {
            _appSettings.KillSwitchMode = KillSwitchMode.Off;
            TryClose();
        }

        private void UpgradeAction()
        {
            _urlConfig.AccountUrl.Open();
            TryClose();
        }
    }
}