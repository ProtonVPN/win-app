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

using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.CommandWpf;
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
    internal class MainViewModel : LanguageAwareViewModel, IVpnStateAware, IOnboardingStepAware
    {
        private readonly IUserAuthenticator _userAuthenticator;
        private readonly IVpnManager _vpnManager;
        private readonly IActiveUrls _urls;
        private readonly IEventAggregator _eventAggregator;
        private readonly AppExitHandler _appExitHandler;
        private readonly IModals _modals;
        private readonly IDialogs _dialogs;
        private readonly IPopupWindows _popups;

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
            TrayNotificationViewModel trayNotificationViewModel)
        {
            _eventAggregator = eventAggregator;
            _vpnManager = vpnManager;
            _urls = urls;
            _userAuthenticator = userAuthenticator;
            _appExitHandler = appExitHandler;
            _modals = modals;
            _dialogs = dialogs;
            _popups = popups;

            Map = mapViewModel;
            Connection = connectingViewModel;
            Onboarding = onboardingViewModel;
            TrayNotification = trayNotificationViewModel;
            FlashNotification = flashNotificationViewModel;

            eventAggregator.Subscribe(this);

            AboutCommand = new RelayCommand(AboutAction, CanClick);
            AccountCommand = new RelayCommand(AccountAction, CanClick);
            ProfilesCommand = new RelayCommand(ProfilesAction, CanClick);
            SettingsCommand = new RelayCommand(SettingsAction, CanClick);
            HelpCommand = new RelayCommand(HelpAction);
            ReportBugCommand = new RelayCommand(ReportBugAction, CanClick);
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

        public bool Connecting
        {
            get => _connecting;
            set
            {
                if (_connecting != value)
                {
                    _eventAggregator.PublishOnUIThread(new ToggleOverlay(value));
                }
                Set(ref _connecting, value);
                CommandManager.InvalidateRequerySuggested();
                SetupMenuItems();
            }
        }

        private bool _blockMenu;
        public bool BlockMenu
        {
            get => _blockMenu;
            set => Set(ref _blockMenu, value);
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

        public void Load()
        {
            SetupMenuItems();
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
                    SetupMenuItems();
                    break;
            }

            return Task.CompletedTask;
        }

        public void OnStepChanged(int step)
        {
            ShowOnboarding = step > 0;
        }

        private void SetupMenuItems()
        {
            BlockMenu = Connecting;
        }

        private bool CanClick()
        {
            return !Connecting;
        }

        private void AboutAction()
        {
            _modals.Show<AboutModalViewModel>();
        }

        private void AccountAction()
        {
            _modals.Show<AccountModalViewModel>();
        }

        public void ProfilesAction()
        {
            _modals.Show<ProfileListModalViewModel>();
        }

        private void SettingsAction()
        {
            _modals.Show<SettingsModalViewModel>();
        }

        private void HelpAction()
        {
            _urls.HelpUrl.Open();
        }

        private void ReportBugAction()
        {
            _modals.Show<ReportBugModalViewModel>();
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
                bool? result = _dialogs.ShowQuestion(Translation.Get("App_msg_LogoutConnectedConfirm"));
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