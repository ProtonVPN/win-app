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

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Account;
using ProtonVPN.BugReporting;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.DisconnectLogs;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Modals
{
    public class DisconnectErrorModalViewModel : BaseModalViewModel
    {
        private readonly ILogger _logger;
        private readonly IActiveUrls _urlConfig;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IAppSettings _appSettings;
        private readonly IVpnManager _vpnManager;
        private readonly IUserStorage _userStorage;
        private readonly IModals _modals;

        private VpnError _error;
        private bool _networkBlocked;

        public ICommand OpenHelpArticleCommand { get; set; }
        public ICommand DisableKillSwitchCommand { get; set; }
        public ICommand UpgradeCommand { get; set; }
        public ICommand ReportBugCommand { get; set; }
        public ICommand OpenRpcServerUrlCommand { get; }

        public DisconnectErrorModalViewModel(
            ILogger logger,
            IActiveUrls urlConfig,
            ISubscriptionManager subscriptionManager,
            IAppSettings appSettings,
            IVpnManager vpnManager,
            IUserStorage userStorage, 
            IModals modals)
        {
            _logger = logger;
            _urlConfig = urlConfig;
            _subscriptionManager = subscriptionManager;
            _appSettings = appSettings;
            _vpnManager = vpnManager;
            _userStorage = userStorage;
            _modals = modals;

            OpenHelpArticleCommand = new RelayCommand(OpenHelpArticleAction);
            DisableKillSwitchCommand = new RelayCommand(DisableKillSwitch);
            UpgradeCommand = new RelayCommand(UpgradeAction);
            ReportBugCommand = new RelayCommand(ReportBugAction);
            OpenRpcServerUrlCommand = new RelayCommand(OpenRpcServerProblemUrl);
        }

        public VpnError Error
        {
            get => _error;
            set => Set(ref _error, value);
        }

        public bool ShowUpgrade => Error == VpnError.SessionLimitReached &&
                                   _userStorage.GetUser().MaxTier < ServerTiers.Plus;

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

            HandleError(options.Error);

            _logger.Info<DisconnectLog>($"Disconnected due to: {Error}. Network blocked: {NetworkBlocked}");
        }

        private void HandleError(VpnError error)
        {
            if (error is VpnError.TlsError or VpnError.TlsCertificateError)
            {
                _logger.Error<DisconnectLog>($"The error '{error}' was handled by the app.");
            }
        }

        protected override async void OnViewReady(object view)
        {
            base.OnViewReady(view);

            switch (Error)
            {
                case VpnError.TlsError:
                case VpnError.PingTimeoutError:
                case VpnError.AdapterTimeoutError:
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
            await _vpnManager.ReconnectAsync(new VpnReconnectionSettings
            {
                IsToReconnectIfDisconnected = true,
                IsToShowReconnectionPopup = true
            });
        }

        private async Task CloseModalAsync()
        {
            // If TryClose() is called before any await (that actually awaits), Caliburn will throw a
            // NullReferenceException after OnViewReady() ends.
            await Task.Delay(TimeSpan.FromMilliseconds(1));
            TryClose(true);
        }

        private async Task ReconnectWithoutLastServerAsync()
        {
            await CloseModalAsync();
            await _vpnManager.ReconnectAsync(new VpnReconnectionSettings
            {
                IsToReconnectIfDisconnected = true,
                IsToExcludeLastServer = true,
                IsToShowReconnectionPopup = true
            });
        }

        private void OpenHelpArticleAction()
        {
            _urlConfig.TroubleShootingUrl.Open();
        }

        private void DisableKillSwitch()
        {
            _appSettings.KillSwitchMode = KillSwitchMode.Off;
            TryClose();
        }

        private void UpgradeAction()
        {
            _subscriptionManager.UpgradeAccountAsync();
            TryClose();
        }

        private void ReportBugAction()
        {
            _modals.Show<ReportBugModalViewModel>();
        }

        private void OpenRpcServerProblemUrl()
        {
            _urlConfig.RpcServerProblemUrl.Open();
        }
    }
}