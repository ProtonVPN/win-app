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
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.About;
using ProtonVPN.Account;
using ProtonVPN.BugReporting;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Config.Url;
using ProtonVPN.ConnectingScreen;
using ProtonVPN.Core;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Events;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Core.Windows.Popups;
using ProtonVPN.FlashNotifications;
using ProtonVPN.Map.ViewModels;
using ProtonVPN.Onboarding;
using ProtonVPN.Profiles;
using ProtonVPN.Settings;
using ProtonVPN.Translations;
using ProtonVPN.Windows.Popups.DeveloperTools;

namespace ProtonVPN.ViewModels
{
    internal class MainViewModel : LanguageAwareViewModel, IVpnStateAware, IOnboardingStepAware, IUserDataAware
    {
        private readonly IUserAuthenticator _userAuthenticator;
        private readonly IVpnManager _vpnManager;
        private readonly IActiveUrls _urls;
        private readonly IEventAggregator _eventAggregator;
        private readonly AppExitHandler _appExitHandler;
        private readonly IModals _modals;
        private readonly IDialogs _dialogs;
        private readonly IPopupWindows _popups;
        private readonly IAppSettings _appSettings;
        private readonly IUserStorage _userStorage;

        private bool _connecting;

        public MainViewModel(
            IUserAuthenticator userAuthenticator,
            IVpnManager vpnManager,
            IActiveUrls urls,
            IEventAggregator eventAggregator,
            AppExitHandler appExitHandler,
            IModals modals,
            IDialogs dialogs, 
            IPopupWindows popups,
            MapViewModel mapViewModel,
            ConnectingViewModel connectingViewModel,
            OnboardingViewModel onboardingViewModel,
            FlashNotificationViewModel flashNotificationViewModel,
            TrayNotificationViewModel trayNotificationViewModel,
            IAppSettings appSettings,
            IUserStorage userStorage)
        {
            _eventAggregator = eventAggregator;
            _vpnManager = vpnManager;
            _urls = urls;
            _userAuthenticator = userAuthenticator;
            _appExitHandler = appExitHandler;
            _modals = modals;
            _dialogs = dialogs;
            _popups = popups;
            _appSettings = appSettings;
            _userStorage = userStorage;

            Map = mapViewModel;
            Connection = connectingViewModel;
            Onboarding = onboardingViewModel;
            TrayNotification = trayNotificationViewModel;
            FlashNotification = flashNotificationViewModel;

            eventAggregator.Subscribe(this);

            AboutCommand = new RelayCommand(AboutAction);
            AccountCommand = new RelayCommand(AccountAction);
            ProfilesCommand = new RelayCommand(ProfilesAction);
            SettingsCommand = new RelayCommand(SettingsAction);
            HelpCommand = new RelayCommand(HelpAction);
            ReportBugCommand = new RelayCommand(ReportBugAction);
            DeveloperToolsCommand = new RelayCommand(DeveloperToolsAction);
            LogoutCommand = new RelayCommand(LogoutAction);
            ExitCommand = new RelayCommand(ExitAction);

            SetDeveloperToolsVisibility();
        }

        [Conditional("DEBUG")]
        private void SetDeveloperToolsVisibility()
        {
            IsToShowDeveloperTools = true;
        }

        public MapViewModel Map { get; }
        public ConnectingViewModel Connection { get; }
        public OnboardingViewModel Onboarding { get; }
        public TrayNotificationViewModel TrayNotification { get; }
        public FlashNotificationViewModel FlashNotification { get; }

        public ICommand AboutCommand { get; }
        public ICommand AccountCommand { get; }
        public ICommand ProfilesCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand HelpCommand { get; }
        public ICommand ReportBugCommand { get; }
        public ICommand DeveloperToolsCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ExitCommand { get; }

        public bool IsToShowDeveloperTools { get; private set; }

        public bool IsToShowProfilesMenuItem => !_appSettings.FeatureFreeRescopeEnabled || _userStorage.GetUser().Paid();

        public bool Connecting
        {
            get => _connecting;
            set
            {
                if (_connecting != value)
                {
                    _eventAggregator.PublishOnUIThreadAsync(new ToggleOverlay(value));
                }
                Set(ref _connecting, value);
            }
        }

        private bool _showOnboarding;
        public bool ShowOnboarding
        {
            get => _showOnboarding;
            set => Set(ref _showOnboarding, value);
        }

        private VpnStatus _vpnStatus;
        public VpnStatus VpnStatus
        {
            get => _vpnStatus;
            set => Set(ref _vpnStatus, value);
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            VpnStatus = e.State.Status;

            switch (e.State.Status)
            {
                case VpnStatus.Pinging:
                case VpnStatus.Connecting:
                case VpnStatus.Reconnecting:
                case VpnStatus.Waiting:
                case VpnStatus.Authenticating:
                case VpnStatus.RetrievingConfiguration:
                case VpnStatus.AssigningIp:
                    Connecting = true;
                    break;
                case VpnStatus.Connected:
                    Connecting = false;
                    break;
                case VpnStatus.Disconnecting:
                case VpnStatus.Disconnected:
                    Connecting = false;
                    break;
            }

            return Task.CompletedTask;
        }

        public override void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            base.OnAppSettingsChanged(e);

            if (e.PropertyName == nameof(IAppSettings.FeatureFreeRescopeEnabled))
            {
                NotifyOfPropertyChange(nameof(IsToShowProfilesMenuItem));
            }
        }

        public void OnStepChanged(int step)
        {
            ShowOnboarding = step > 0;
        }

        public void OnUserDataChanged()
        {
            NotifyOfPropertyChange(nameof(IsToShowProfilesMenuItem));
        }

        private async void AboutAction()
        {
            await _modals.ShowAsync<AboutModalViewModel>();
        }

        private async void AccountAction()
        {
            await _modals.ShowAsync<AccountModalViewModel>();
        }

        public async void ProfilesAction()
        {
            await _modals.ShowAsync<ProfileListModalViewModel>();
        }

        private async void SettingsAction()
        {
            await _modals.ShowAsync<SettingsModalViewModel>();
        }

        private void HelpAction()
        {
            _urls.HelpUrl.Open();
        }

        private async void ReportBugAction()
        {
            await _modals.ShowAsync<ReportBugModalViewModel>();
        }

        private void DeveloperToolsAction()
        {
            ShowDeveloperTools();
        }

        [Conditional("DEBUG")]
        private void ShowDeveloperTools()
        {
            _popups.Show<DeveloperToolsPopupViewModel>();
        }

        private async void LogoutAction()
        {
            if (VpnStatus != VpnStatus.Disconnected && VpnStatus != VpnStatus.Disconnecting)
            {
                bool? result = await _dialogs.ShowQuestionAsync(Translation.Get("App_msg_LogoutConnectedConfirm"));
                if (!result.HasValue || !result.Value)
                {
                    return;
                }
            }

            await _vpnManager.DisconnectAsync();
            await _userAuthenticator.LogoutAsync();
        }

        private void ExitAction()
        {
            _appExitHandler.RequestExit();
        }
    }
}