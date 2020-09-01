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
using ProtonVPN.BugReporting;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Config.Url;
using ProtonVPN.ConnectionInfo;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Translations;
using ProtonVPN.Settings;

namespace ProtonVPN.Modals
{
    public class DisconnectErrorModalViewModel : BaseModalViewModel
    {
        private VpnError _error;
        private bool _networkBlocked;
        private readonly IActiveUrls _urlConfig;
        private readonly ConnectionErrorResolver _connectionErrorResolver;
        private readonly IVpnManager _vpnManager;
        private readonly SettingsModalViewModel _settingsModalViewModel;
        private readonly IModals _modals;
        private readonly ILogger _logger;
        private readonly ProfileManager _profileManager;
        private readonly IUserStorage _userStorage;

        public ICommand OpenHelpArticleCommand { get; set; }

        public ICommand SettingsCommand { get; set; }

        public ICommand DisableKillSwitchCommand { get; set; }

        public ICommand ReportBugCommand { get; set; }

        public ICommand GoToAccountCommand { get; set; }

        public ICommand UpgradeCommand { get; set; }

        public DisconnectErrorModalViewModel(
            ILogger logger,
            IActiveUrls urlConfig,
            ConnectionErrorResolver connectionErrorResolver,
            IVpnManager vpnManager,
            IModals modals,
            SettingsModalViewModel settingsModalViewModel,
            ProfileManager profileManager,
            IUserStorage userStorage)
        {
            _userStorage = userStorage;
            _logger = logger;
            _modals = modals;
            _settingsModalViewModel = settingsModalViewModel;
            _vpnManager = vpnManager;
            _connectionErrorResolver = connectionErrorResolver;
            _urlConfig = urlConfig;
            _profileManager = profileManager;

            OpenHelpArticleCommand = new RelayCommand(OpenHelpArticleAction);
            SettingsCommand = new RelayCommand(OpenSettings);
            DisableKillSwitchCommand = new RelayCommand(DisableKillSwitch);
            ReportBugCommand = new RelayCommand(ReportBug);
            GoToAccountCommand = new RelayCommand(OpenAccountPage);
            UpgradeCommand = new RelayCommand(UpgradeAction);
        }

        public VpnError Error
        {
            get => _error;
            set => Set(ref _error, value);
        }

        public string ErrorDescription
        {
            get
            {
                switch (Error)
                {
                    case VpnError.SessionLimitReached:
                        return _userStorage.User().MaxTier < ServerTiers.Plus ?
                            StringResources.Get("Dialogs_DisconnectError_msg_SessionLimitFreeBasic") :
                            StringResources.Get("Dialogs_DisconnectError_msg_SessionLimitPlus");
                    default:
                        return string.Empty;
                }
            }
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
                return;

            NetworkBlocked = options.NetworkBlocked;
            Error = options.Error;

            _logger.Info($"Disconnected due to: {Error}. Network blocked: {NetworkBlocked}");
        }

        protected override async void OnViewReady(object view)
        {
            base.OnViewReady(view);

            if (Error == VpnError.AuthorizationError)
            {
                var error = await _connectionErrorResolver.ResolveError();
                switch (error)
                {
                    case VpnError.PasswordChanged:
                        TryClose(true);
                        await _vpnManager.Reconnect();
                        break;
                    case VpnError.ServerOffline:
                    case VpnError.ServerRemoved:
                        TryClose(true);
                        await _vpnManager.Connect(await _profileManager.GetFastestProfile());
                        break;
                    default:
                        Error = error;
                        NotifyOfPropertyChange(nameof(ShowUpgrade));
                        NotifyOfPropertyChange(nameof(ErrorDescription));
                        break;
                }
            }
        }

        private void OpenHelpArticleAction()
        {
            _urlConfig.TroubleShootingUrl.Open();
        }

        private void OpenSettings()
        {
            TryClose();
            _settingsModalViewModel.OpenAdvancedTab();
            _modals.Show<SettingsModalViewModel>();
        }

        private void OpenAccountPage()
        {
            _urlConfig.AccountUrl.Open();
        }

        private async void DisableKillSwitch()
        {
            TryClose();
            await _vpnManager.Disconnect();
            _logger.Info("Killswitch disabled");
        }

        private void ReportBug()
        {
            TryClose();
            _modals.Show<ReportBugModalViewModel>();
        }

        private void UpgradeAction()
        {
            _urlConfig.AccountUrl.Open();
            TryClose();
        }
    }
}
