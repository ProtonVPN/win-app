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

using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals;
using ProtonVPN.Onboarding;
using ProtonVPN.Settings;

namespace ProtonVPN.Sidebar.QuickSettings
{
    internal class QuickSettingsViewModel : Screen,
        ISettingsAware,
        IUserDataAware,
        IVpnStateAware,
        IOnboardingStepAware
    {
        private readonly IAppSettings _appSettings;
        private readonly IUserStorage _userStorage;
        private readonly IActiveUrls _urls;
        private readonly IModals _modals;
        private readonly IVpnReconnector _vpnReconnector;

        public QuickSettingsViewModel(
            IAppSettings appSettings,
            IUserStorage userStorage,
            IActiveUrls urls,
            IModals modals,
            IVpnReconnector vpnReconnector)
        {
            _modals = modals;
            _urls = urls;
            _userStorage = userStorage;
            _appSettings = appSettings;
            _vpnReconnector = vpnReconnector;

            SecureCoreLearnMoreCommand = new RelayCommand(OpenSecureCoreArticleAction);
            NetShieldLearnMoreCommand = new RelayCommand(OpenNetShieldArticleAction);
            KillSwitchLearnMoreCommand = new RelayCommand(OpenKillSwitchArticleAction);
            PortForwardingLearnMoreCommand = new RelayCommand(OpenPortForwardingArticleAction);

            SecureCoreOffCommand = new RelayCommand(TurnOffSecureCoreActionAsync);
            SecureCoreOnCommand = new RelayCommand(TurnOnSecureCoreActionAsync);

            NetShieldOffCommand = new RelayCommand(TurnOffNetShieldActionAsync);
            NetShieldOnFirstCommand = new RelayCommand(TurnOnNetShieldFirstModeActionAsync);
            NetShieldOnSecondCommand = new RelayCommand(TurnOnNetShieldSecondModeActionAsync);

            KillSwitchOffCommand = new RelayCommand(TurnOffKillSwitchActionAsync);
            KillSwitchOnCommand = new RelayCommand(TurnOnKillSwitchActionAsync);

            PortForwardingOffCommand = new RelayCommand(TurnOffPortForwardingActionAsync);
            PortForwardingOnCommand = new RelayCommand(TurnOnPortForwardingActionAsync);

            GetPlusCommand = new RelayCommand(GetPlusAction);
        }

        public ICommand SecureCoreOffCommand { get; }
        public ICommand SecureCoreOnCommand { get; }

        public ICommand KillSwitchLearnMoreCommand { get; }
        public ICommand NetShieldLearnMoreCommand { get; }
        public ICommand SecureCoreLearnMoreCommand { get; }
        public ICommand PortForwardingLearnMoreCommand { get; }

        public ICommand KillSwitchOnCommand { get; }
        public ICommand KillSwitchOffCommand { get; }

        public ICommand NetShieldOffCommand { get; }
        public ICommand NetShieldOnFirstCommand { get; }
        public ICommand NetShieldOnSecondCommand { get; }

        public ICommand PortForwardingOnCommand { get; }
        public ICommand PortForwardingOffCommand { get; }

        public ICommand GetPlusCommand { get; }

        public bool IsSecureCoreOnButtonOn => _appSettings.SecureCore;
        public bool IsSecureCoreOffButtonOn => !_appSettings.SecureCore;

        public bool IsNetShieldOffButtonOn => !_appSettings.NetShieldEnabled;
        public bool IsNetShieldFirstButtonOn => _appSettings.NetShieldEnabled && _appSettings.NetShieldMode == 1;
        public bool IsNetShieldSecondButtonOn => _appSettings.NetShieldEnabled && _appSettings.NetShieldMode == 2;
        public int NetShieldMode => _appSettings.NetShieldMode;
        public bool IsNetShieldOn => _appSettings.NetShieldEnabled;
        public bool IsNetShieldVisible => _appSettings.FeatureNetShieldEnabled;

        public bool IsPortForwardingOnButtonOn => _appSettings.PortForwardingEnabled;
        public bool IsPortForwardingOffButtonOn => !_appSettings.PortForwardingEnabled;
        public bool IsPortForwardingVisible => _appSettings.FeaturePortForwardingEnabled;

        public int KillSwitchButtonNumber => _appSettings.FeatureNetShieldEnabled ? 2 : 1;
        public int PortForwardingButtonNumber => KillSwitchButtonNumber + 1;
        public int TotalButtons => 2 + (_appSettings.FeatureNetShieldEnabled ? 1 : 0) + (_appSettings.FeaturePortForwardingEnabled ? 1 : 0);

        public bool IsKillSwitchOnButtonOn => _appSettings.KillSwitch;
        public bool IsKillSwitchOffButtonOn => !_appSettings.KillSwitch;

        public bool IsFreeUser => _userStorage.User().TrialStatus() == PlanStatus.Free;

        public bool IsUserTierPlusOrHigher => _userStorage.User().MaxTier >= ServerTiers.Plus;

        private bool _showSecureCorePopup;

        public bool ShowSecureCorePopup
        {
            get => _showSecureCorePopup;
            set => Set(ref _showSecureCorePopup, value);
        }

        private bool _showNetShieldPopup;

        public bool ShowNetShieldPopup
        {
            get => _showNetShieldPopup;
            set => Set(ref _showNetShieldPopup, value);
        }

        private bool _showKillSwitchPopup;

        public bool ShowKillSwitchPopup
        {
            get => _showKillSwitchPopup;
            set => Set(ref _showKillSwitchPopup, value);
        }

        private bool _showPortForwardingPopup;

        public bool ShowPortForwardingPopup
        {
            get => _showPortForwardingPopup;
            set => Set(ref _showPortForwardingPopup, value);
        }

        private bool _showOnboardingStep;

        public bool ShowOnboardingStep
        {
            get => _showOnboardingStep;
            set => Set(ref _showOnboardingStep, value);
        }

        public void OnUserDataChanged()
        {
            if (IsFreeUser && _appSettings.NetShieldEnabled)
            {
                _appSettings.NetShieldEnabled = false;

                NotifyOfPropertyChange(nameof(IsNetShieldOn));
                NotifyOfPropertyChange(nameof(IsNetShieldOffButtonOn));
                NotifyOfPropertyChange(nameof(IsNetShieldFirstButtonOn));
                NotifyOfPropertyChange(nameof(IsNetShieldSecondButtonOn));
            }

            NotifyOfPropertyChange(nameof(IsFreeUser));
            NotifyOfPropertyChange(nameof(IsUserTierPlusOrHigher));
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IAppSettings.KillSwitch):
                    NotifyOfPropertyChange(nameof(IsKillSwitchOnButtonOn));
                    NotifyOfPropertyChange(nameof(IsKillSwitchOffButtonOn));
                    break;
                case nameof(IAppSettings.NetShieldEnabled):
                case nameof(IAppSettings.NetShieldMode):
                    NotifyOfPropertyChange(nameof(IsNetShieldOn));
                    NotifyOfPropertyChange(nameof(NetShieldMode));
                    NotifyOfPropertyChange(nameof(IsNetShieldOffButtonOn));
                    NotifyOfPropertyChange(nameof(IsNetShieldFirstButtonOn));
                    NotifyOfPropertyChange(nameof(IsNetShieldSecondButtonOn));
                    break;
                case nameof(IAppSettings.SecureCore):
                    NotifyOfPropertyChange(nameof(IsSecureCoreOffButtonOn));
                    NotifyOfPropertyChange(nameof(IsSecureCoreOnButtonOn));
                    break;
                case nameof(IAppSettings.FeatureNetShieldEnabled):
                    NotifyOfPropertyChange(nameof(IsNetShieldVisible));
                    NotifyOfPropertyChange(nameof(KillSwitchButtonNumber));
                    NotifyOfPropertyChange(nameof(PortForwardingButtonNumber));
                    NotifyOfPropertyChange(nameof(TotalButtons));
                    break;
                case nameof(IAppSettings.FeaturePortForwardingEnabled):
                    NotifyOfPropertyChange(nameof(IsPortForwardingVisible));
                    NotifyOfPropertyChange(nameof(TotalButtons));
                    break;
                case nameof(IAppSettings.PortForwardingEnabled):
                    NotifyOfPropertyChange(nameof(IsPortForwardingOnButtonOn));
                    NotifyOfPropertyChange(nameof(IsPortForwardingOffButtonOn));
                    break;
            }
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (e.State.Status == VpnStatus.Connecting)
            {
                ShowSecureCorePopup = false;
                ShowNetShieldPopup = false;
                ShowKillSwitchPopup = false;
                ShowPortForwardingPopup = false;
            }

            return Task.CompletedTask;
        }

        public void OnStepChanged(int step)
        {
            ShowOnboardingStep = step == 4;
        }

        private async void TurnOffSecureCoreActionAsync()
        {
            _appSettings.SecureCore = false;
            await ReconnectAsync();
        }

        private async Task ReconnectAsync()
        {
            await _vpnReconnector.ReconnectAsync();
        }

        private async void TurnOnSecureCoreActionAsync()
        {
            if (IsUserTierPlusOrHigher)
            {
                _appSettings.PortForwardingEnabled = false;
                _appSettings.SecureCore = true;
                await ReconnectAsync();
            }
            else
            {
                _urls.AccountUrl.Open();
            }
        }

        private async void TurnOffKillSwitchActionAsync()
        {
            _appSettings.KillSwitch = false;
            await ReconnectAsync();
        }

        private async void TurnOnKillSwitchActionAsync()
        {
            _appSettings.KillSwitch = true;
            await ReconnectAsync();
        }

        private async void TurnOnPortForwardingActionAsync()
        {
            if (IsUserTierPlusOrHigher)
            {
                await TurnOnPortForwardingAsync();
            }
            else
            {
                _urls.AccountUrl.Open();
            }
        }

        private async Task TurnOnPortForwardingAsync()
        {
            if (_appSettings.PortForwardingEnabled)
            {
                return;
            }

            if (_appSettings.DoNotShowPortForwardingConfirmationDialog)
            {
                await EnablePortForwardingAsync();
            }
            else
            {
                bool? result = _modals.Show<PortForwardingConfirmationModalViewModel>();

                if (result.HasValue && result.Value)
                {
                    await EnablePortForwardingAsync();
                }
            }
        }

        private async Task EnablePortForwardingAsync()
        {
            _appSettings.SecureCore = false;
            _appSettings.PortForwardingEnabled = true;
            await ReconnectAsync();
        }

        private async void TurnOffPortForwardingActionAsync()
        {
            _appSettings.PortForwardingEnabled = false;
            await ReconnectAsync();
        }

        private async void TurnOffNetShieldActionAsync()
        {
            _appSettings.NetShieldEnabled = false;
            await ReconnectAsync();
        }

        private async void TurnOnNetShieldFirstModeActionAsync()
        {
            if (IsFreeUser)
            {
                _urls.AccountUrl.Open();
            }
            else
            {
                _appSettings.NetShieldEnabled = true;
                _appSettings.NetShieldMode = 1;
                await ReconnectAsync();
            }
        }

        private async void TurnOnNetShieldSecondModeActionAsync()
        {
            if (IsFreeUser)
            {
                _urls.AccountUrl.Open();
            }
            else
            {
                _appSettings.NetShieldEnabled = true;
                _appSettings.NetShieldMode = 2;
                await ReconnectAsync();
            }
        }

        private void OpenSecureCoreArticleAction()
        {
            _urls.AboutSecureCoreUrl.Open();
        }

        private void OpenNetShieldArticleAction()
        {
            _urls.AboutNetShieldUrl.Open();
        }

        private void OpenKillSwitchArticleAction()
        {
            _urls.AboutKillSwitchUrl.Open();
        }

        private void OpenPortForwardingArticleAction()
        {
            _urls.AboutPortForwardingUrl.Open();
        }

        private void GetPlusAction()
        {
            _urls.AccountUrl.Open();
        }
    }
}