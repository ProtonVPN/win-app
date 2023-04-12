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
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.NetShield;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.NetShield;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals;
using ProtonVPN.Modals.Upsell;
using ProtonVPN.Onboarding;
using ProtonVPN.PortForwarding;
using ProtonVPN.PortForwarding.ActivePorts;
using ProtonVPN.Settings;
using ProtonVPN.Translations;
using static ProtonVPN.Common.Extensions.ExponentialExtensions;

namespace ProtonVPN.Sidebar.QuickSettings
{
    internal class QuickSettingsViewModel : Screen,
        ISettingsAware,
        IUserDataAware,
        IVpnStateAware,
        IOnboardingStepAware,
        INetShieldStatisticAware
    {
        private const int AVG_AD_SIZE_IN_BYTES = 200000;
        private const int AVG_TRACKER_SIZE_IN_BYTES = 50000;
        private const int AVG_MALWARE_SIZE_IN_BYTES = 750000;
        private const string NETSHIELD_ADS_BLOCKED_TRANSLATION = "QuickSettings_lbl_NetShieldStatsAds";
        private const string NETSHIELD_TRACKERS_STOPPED_TRANSLATION = "QuickSettings_lbl_NetShieldStatsTrackers";

        private readonly IAppSettings _appSettings;
        private readonly IUserStorage _userStorage;
        private readonly IActiveUrls _urls;
        private readonly IModals _modals;
        private readonly IVpnManager _vpnManager;
        private readonly IPortForwardingManager _portForwardingManager;

        private bool _isConnected;
        private NetShieldStatistic _lastStats;

        public PortForwardingActivePortViewModel ActivePortViewModel { get; }
        public PortForwardingWarningViewModel PortForwardingWarningViewModel { get; }

        public ICommand SecureCoreOffCommand { get; }
        public ICommand SecureCoreOnCommand { get; }

        public ICommand KillSwitchLearnMoreCommand { get; }
        public ICommand NetShieldLearnMoreCommand { get; }
        public ICommand SecureCoreLearnMoreCommand { get; }
        public ICommand PortForwardingLearnMoreCommand { get; }

        public ICommand EnableSoftKillSwitchCommand { get; }
        public ICommand EnableHardKillSwitchCommand { get; }
        public ICommand DisableKillSwitchCommand { get; }

        public ICommand NetShieldOffCommand { get; }
        public ICommand NetShieldOnFirstCommand { get; }
        public ICommand NetShieldOnSecondCommand { get; }

        public ICommand PortForwardingOnCommand { get; }
        public ICommand PortForwardingOffCommand { get; }

        public ICommand ShowSecureCoreUpsellModalCommand { get; }
        public ICommand ShowNetshieldUpsellModalCommand { get; }
        public ICommand ShowPortForwardingUpsellModalCommand { get; }

        public bool IsSecureCoreOnButtonOn => _appSettings.SecureCore;
        public bool IsSecureCoreOffButtonOn => !_appSettings.SecureCore;

        public bool IsNetShieldOffButtonOn => !_appSettings.NetShieldEnabled;
        public bool IsNetShieldFirstButtonOn => _appSettings.NetShieldEnabled && _appSettings.NetShieldMode == 1;
        public bool IsNetShieldSecondButtonOn => _appSettings.NetShieldEnabled && _appSettings.NetShieldMode == 2;
        public int NetShieldMode => _appSettings.NetShieldMode;
        public bool IsNetShieldOn => _appSettings.NetShieldEnabled;
        public bool IsNetShieldVisible => _appSettings.FeatureNetShieldEnabled;
        public bool HasNetShieldStats => _appSettings.IsNetShieldEnabled() && _appSettings.NetShieldMode == 2 && _isConnected && HasNetShieldStatsContainer;
        public bool HasNetShieldStatsContainer => _userStorage.GetUser().Paid() && _appSettings.FeatureNetShieldStatsEnabled;

        public bool IsSoftKillSwitchEnabled => _appSettings.KillSwitchMode == Common.KillSwitch.KillSwitchMode.Soft;
        public bool IsHardKillSwitchEnabled => _appSettings.KillSwitchMode == Common.KillSwitch.KillSwitchMode.Hard;
        public bool IsKillSwitchDisabled => _appSettings.KillSwitchMode == Common.KillSwitch.KillSwitchMode.Off;
        public bool IsKillSwitchEnabled => IsSoftKillSwitchEnabled || IsHardKillSwitchEnabled;
        public int KillSwitchMode => (int)_appSettings.KillSwitchMode;

        public bool IsPortForwardingOnButtonOn => _appSettings.PortForwardingEnabled;
        public bool IsPortForwardingOffButtonOn => !_appSettings.PortForwardingEnabled;
        public bool IsPortForwardingVisible => _appSettings.PortForwardingInQuickSettings && _appSettings.FeaturePortForwardingEnabled;

        public int KillSwitchButtonNumber => _appSettings.FeatureNetShieldEnabled ? 2 : 1;
        public int PortForwardingButtonNumber => KillSwitchButtonNumber + 1;

        public int TotalButtons => 2 + (IsNetShieldVisible ? 1 : 0) + (IsPortForwardingVisible ? 1 : 0);

        public bool IsFreeUser => !_userStorage.GetUser().Paid();
        public bool IsUserTierPlusOrHigher => _userStorage.GetUser().IsTierPlusOrHigher();
        public bool IsPaidUser => _userStorage.GetUser().Paid();

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

        private string _netShieldBadgeValue = "0";
        public string NetShieldBadgeValue
        {
            get => _netShieldBadgeValue;
            set => Set(ref _netShieldBadgeValue, value);
        }

        private string _netShieldStatsNumOfAdvertisementUrlsBlocked = "0";
        public string NetShieldStatsNumOfAdvertisementUrlsBlocked
        {
            get => _netShieldStatsNumOfAdvertisementUrlsBlocked;
            set => Set(ref _netShieldStatsNumOfAdvertisementUrlsBlocked, value);
        }

        private string _netShieldStatsLabelOfAdvertisementUrlsBlocked;
        public string NetShieldStatsLabelOfAdvertisementUrlsBlocked
        {
            get => _netShieldStatsLabelOfAdvertisementUrlsBlocked;
            set => Set(ref _netShieldStatsLabelOfAdvertisementUrlsBlocked, value);
        }

        private string _netShieldStatsNumOfTrackingUrlsBlocked = "0";
        public string NetShieldStatsNumOfTrackingUrlsBlocked
        {
            get => _netShieldStatsNumOfTrackingUrlsBlocked;
            set => Set(ref _netShieldStatsNumOfTrackingUrlsBlocked, value);
        }
        
        private string _netShieldStatsLabelOfTrackingUrlsBlocked;
        public string NetShieldStatsLabelOfTrackingUrlsBlocked
        {
            get => _netShieldStatsLabelOfTrackingUrlsBlocked;
            set => Set(ref _netShieldStatsLabelOfTrackingUrlsBlocked, value);
        }

        private string _netShieldStatsDataSaved = "0";
        public string NetShieldStatsDataSaved
        {
            get => _netShieldStatsDataSaved;
            set => Set(ref _netShieldStatsDataSaved, value);
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

        public bool HasPortForwardingValue => ActivePortViewModel.HasPortForwardingValue;

        private bool _showOnboardingStep;
        public bool ShowOnboardingStep
        {
            get => _showOnboardingStep;
            set => Set(ref _showOnboardingStep, value);
        }

        public bool ShowPortForwardingWarningLabel => PortForwardingWarningViewModel.IsToShowPortForwardingWarningLabel;

        public QuickSettingsViewModel(
            IAppSettings appSettings,
            IUserStorage userStorage,
            IActiveUrls urls,
            IModals modals,
            IVpnManager vpnManager,
            IPortForwardingManager portForwardingManager,
            PortForwardingActivePortViewModel activePortViewModel,
            PortForwardingWarningViewModel portForwardingWarningViewModel)
        {
            _modals = modals;
            _urls = urls;
            _userStorage = userStorage;
            _appSettings = appSettings;
            _vpnManager = vpnManager;
            _portForwardingManager = portForwardingManager;

            ActivePortViewModel = activePortViewModel;
            ActivePortViewModel.PropertyChanged += OnActivePortViewModelPropertyChanged;
            PortForwardingWarningViewModel = portForwardingWarningViewModel;

            SecureCoreLearnMoreCommand = new RelayCommand(OpenSecureCoreArticleAction);
            NetShieldLearnMoreCommand = new RelayCommand(OpenNetShieldArticleAction);
            KillSwitchLearnMoreCommand = new RelayCommand(OpenKillSwitchArticleAction);
            PortForwardingLearnMoreCommand = new RelayCommand(OpenPortForwardingArticleAction);

            SecureCoreOffCommand = new RelayCommand(TurnOffSecureCoreActionAsync);
            SecureCoreOnCommand = new RelayCommand(TurnOnSecureCoreActionAsync);

            NetShieldOffCommand = new RelayCommand(TurnOffNetShieldAction);
            NetShieldOnFirstCommand = new RelayCommand(TurnOnNetShieldFirstModeActionAsync);
            NetShieldOnSecondCommand = new RelayCommand(TurnOnNetShieldSecondModeActionAsync);

            DisableKillSwitchCommand = new RelayCommand(DisableKillSwitchAction);
            EnableSoftKillSwitchCommand = new RelayCommand(EnableSoftKillSwitchActionAsync);
            EnableHardKillSwitchCommand = new RelayCommand(EnableHardKillSwitchActionAsync);

            PortForwardingOffCommand = new RelayCommand(TurnOffPortForwardingActionAsync);
            PortForwardingOnCommand = new RelayCommand(TurnOnPortForwardingActionAsync);

            ShowSecureCoreUpsellModalCommand = new RelayCommand(ShowSecureCoreUpsellModalAction);
            ShowNetshieldUpsellModalCommand = new RelayCommand(ShowNetshieldUpsellModalAction);
            ShowPortForwardingUpsellModalCommand = new RelayCommand(ShowPortForwardingUpsellModalAction);
        }

        public void OnUserDataChanged()
        {
            if (!IsUserTierPlusOrHigher && _appSettings.SecureCore)
            {
                DisableSecureCore();
            }

            if (!IsPaidUser && _appSettings.PortForwardingEnabled)
            {
                DisablePortForwarding();
            }

            if (IsFreeUser && _appSettings.NetShieldEnabled)
            {
                DisableNetShield();
            }

            NotifyOfPropertyChange(nameof(IsFreeUser));
            NotifyOfPropertyChange(nameof(IsPaidUser));
            NotifyOfPropertyChange(nameof(IsUserTierPlusOrHigher));
            NotifyOfPropertyChange(nameof(HasNetShieldStatsContainer));
            NotifyOfPropertyChange(nameof(HasNetShieldStats));
        }

        private void DisableSecureCore()
        {
            _appSettings.SecureCore = false;

            NotifyOfPropertyChange(nameof(IsSecureCoreOnButtonOn));
            NotifyOfPropertyChange(nameof(IsSecureCoreOffButtonOn));
        }

        private void DisablePortForwarding()
        {
            _appSettings.PortForwardingEnabled = false;

            NotifyOfPropertyChange(nameof(IsPortForwardingOnButtonOn));
            NotifyOfPropertyChange(nameof(IsPortForwardingOffButtonOn));
        }

        private void DisableNetShield()
        {
            _appSettings.NetShieldEnabled = false;

            NotifyOfPropertyChange(nameof(IsNetShieldOn));
            NotifyOfPropertyChange(nameof(IsNetShieldOffButtonOn));
            NotifyOfPropertyChange(nameof(IsNetShieldFirstButtonOn));
            NotifyOfPropertyChange(nameof(IsNetShieldSecondButtonOn));
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IAppSettings.KillSwitchMode):
                    NotifyOfPropertyChange(nameof(IsKillSwitchEnabled));
                    NotifyOfPropertyChange(nameof(IsSoftKillSwitchEnabled));
                    NotifyOfPropertyChange(nameof(IsKillSwitchDisabled));
                    NotifyOfPropertyChange(nameof(IsHardKillSwitchEnabled));
                    NotifyOfPropertyChange(nameof(KillSwitchMode));
                    break;
                case nameof(IAppSettings.NetShieldEnabled):
                case nameof(IAppSettings.NetShieldMode):
                    NotifyOfPropertyChange(nameof(IsNetShieldOn));
                    NotifyOfPropertyChange(nameof(NetShieldMode));
                    NotifyOfPropertyChange(nameof(IsNetShieldOffButtonOn));
                    NotifyOfPropertyChange(nameof(IsNetShieldFirstButtonOn));
                    NotifyOfPropertyChange(nameof(IsNetShieldSecondButtonOn));
                    NotifyOfPropertyChange(nameof(HasNetShieldStats));
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
                    NotifyOfPropertyChange(nameof(HasNetShieldStats));
                    break;
                case nameof(IAppSettings.FeaturePortForwardingEnabled):
                case nameof(IAppSettings.PortForwardingInQuickSettings):
                    NotifyOfPropertyChange(nameof(IsPortForwardingVisible));
                    NotifyOfPropertyChange(nameof(TotalButtons));
                    break;
                case nameof(IAppSettings.PortForwardingEnabled):
                    NotifyOfPropertyChange(nameof(IsPortForwardingOnButtonOn));
                    NotifyOfPropertyChange(nameof(IsPortForwardingOffButtonOn));
                    NotifyOfPropertyChange(nameof(ShowPortForwardingWarningLabel));
                    break;
                case nameof(IAppSettings.FeatureNetShieldStatsEnabled):
                    NotifyOfPropertyChange(nameof(HasNetShieldStats));
                    NotifyOfPropertyChange(nameof(HasNetShieldStatsContainer));
                    break;
                case nameof(IAppSettings.Language):
                    SetNetShieldStatistic(_lastStats);
                    break;
            }
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(ShowPortForwardingWarningLabel));

            if (e.State.Status == VpnStatus.Pinging ||
                e.State.Status == VpnStatus.Connecting ||
                e.State.Status == VpnStatus.Reconnecting)
            {
                ShowSecureCorePopup = false;
                ShowNetShieldPopup = false;
                ShowKillSwitchPopup = false;
                ShowPortForwardingPopup = false;
            }

            bool newIsConnected = e.State.Status == VpnStatus.Connected;
            if (_isConnected != newIsConnected)
            {
                _isConnected = newIsConnected;
                NotifyOfPropertyChange(nameof(HasNetShieldStats));
            }
            if (!newIsConnected)
            {
                ClearNetShieldStatistic();
            }
        }

        public void OnStepChanged(int step)
        {
            ShowOnboardingStep = step == 4;
        }

        private async void TurnOffSecureCoreActionAsync()
        {
            HideSecureCorePopup();
            DisableSecureCore();
            await ReconnectAsync();
        }

        private async Task ReconnectAsync()
        {
            await _vpnManager.ReconnectAsync();
        }

        private void HideSecureCorePopup()
        {
            ShowSecureCorePopup = false;
        }

        private async void TurnOnSecureCoreActionAsync()
        {
            HideSecureCorePopup();

            if (IsUserTierPlusOrHigher)
            {
                if (!_appSettings.DoNotShowDiscourageSecureCoreDialog)
                {
                    bool? result = await _modals.ShowAsync<DiscourageSecureCoreModalViewModel>();
                    if (result.HasValue && !result.Value)
                    {
                        return;
                    }
                }

                DisablePortForwarding();
                _appSettings.SecureCore = true;
                await ReconnectAsync();
            }
            else
            {
                ShowSecureCoreUpsellModalAction();
            }
        }

        private void HideKillSwitchPopup()
        {
            ShowKillSwitchPopup = false;
        }

        private void DisableKillSwitchAction()
        {
            HideKillSwitchPopup();
            _appSettings.KillSwitchMode = Common.KillSwitch.KillSwitchMode.Off;
        }

        private bool WasSplitTunnelDisabled()
        {
            if (_appSettings.SplitTunnelingEnabled)
            {
                _appSettings.SplitTunnelingEnabled = false;
                return true;
            }

            return false;
        }

        private async void EnableSoftKillSwitchActionAsync()
        {
            HideKillSwitchPopup();
            bool splitTunnelDisabled = WasSplitTunnelDisabled();
            _appSettings.KillSwitchMode = Common.KillSwitch.KillSwitchMode.Soft;
            if (splitTunnelDisabled)
            {
                await ReconnectAsync();
            }
        }

        private async void EnableHardKillSwitchActionAsync()
        {
            HideKillSwitchPopup();

            if (!_appSettings.DoNotShowKillSwitchConfirmationDialog)
            {
                bool? result = await _modals.ShowAsync<KillSwitchConfirmationModalViewModel>();
                if (result.HasValue && !result.Value)
                {
                    return;
                }
            }

            bool splitTunnelDisabled = WasSplitTunnelDisabled();
            _appSettings.KillSwitchMode = Common.KillSwitch.KillSwitchMode.Hard;
            if (splitTunnelDisabled)
            {
                await ReconnectAsync();
            }
        }

        private async void TurnOnPortForwardingActionAsync()
        {
            HidePortForwardingPopup();
            await _portForwardingManager.EnableAsync();
        }

        private void HidePortForwardingPopup()
        {
            ShowPortForwardingPopup = false;
        }

        private async void TurnOffPortForwardingActionAsync()
        {
            HidePortForwardingPopup();
            await _portForwardingManager.DisableAsync();
        }

        private void HideNetShieldPopup()
        {
            ShowNetShieldPopup = false;
        }

        private void TurnOffNetShieldAction()
        {
            HideNetShieldPopup();
            DisableNetShield();
        }

        private async void TurnOnNetShieldFirstModeActionAsync()
        {
            HideNetShieldPopup();
            await EnableNetShieldMode(1);
        }

        private async void TurnOnNetShieldSecondModeActionAsync()
        {
            HideNetShieldPopup();
            await EnableNetShieldMode(2);
        }

        private async Task EnableNetShieldMode(int mode)
        {
            if (IsFreeUser)
            {
                ShowNetshieldUpsellModalAction();
            }
            else
            {
                bool isCustomDnsOn = _appSettings.CustomDnsEnabled;
                _appSettings.NetShieldEnabled = true;
                _appSettings.NetShieldMode = mode;
                if (isCustomDnsOn)
                {
                    await ReconnectAsync();
                }
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

        private async void ShowSecureCoreUpsellModalAction()
        {
            await _modals.ShowAsync<SecureCoreUpsellModalViewModel>();
        }

        private async void ShowNetshieldUpsellModalAction()
        {
            await _modals.ShowAsync<NetshieldUpsellModalViewModel>();
        }

        private async void ShowPortForwardingUpsellModalAction()
        {
            await _modals.ShowAsync<PortForwardingUpsellModalViewModel>();
        }

        private void OnActivePortViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyOfPropertyChange(e.PropertyName);
        }

        public void OnNetShieldStatisticChanged(NetShieldStatistic stats)
        {
            if (_isConnected)
            {
                SetNetShieldStatistic(stats);
            }
            else
            {
                ClearNetShieldStatistic();
            }
        }

        private void ClearNetShieldStatistic()
        {
            SetNetShieldStatistic();
        }

        private void SetNetShieldStatistic(NetShieldStatistic stats = null)
        {
            _lastStats = stats;
            if (stats is null)
            {
                NetShieldBadgeValue = "0";
                NetShieldStatsNumOfAdvertisementUrlsBlocked = "0";
                NetShieldStatsLabelOfAdvertisementUrlsBlocked = Translation.GetPlural(NETSHIELD_ADS_BLOCKED_TRANSLATION, 0);
                NetShieldStatsNumOfTrackingUrlsBlocked = "0";
                NetShieldStatsLabelOfTrackingUrlsBlocked = Translation.GetPlural(NETSHIELD_TRACKERS_STOPPED_TRANSLATION, 0);
                NetShieldStatsDataSaved = "0 B";
            }
            else
            {
                long badgeSum = stats.NumOfAdvertisementUrlsBlocked + stats.NumOfTrackingUrlsBlocked;
                NetShieldBadgeValue = badgeSum > 99 ? "99+" : $"{badgeSum}";

                NetShieldStatsNumOfAdvertisementUrlsBlocked = $"{stats.NumOfAdvertisementUrlsBlocked}";
                NetShieldStatsLabelOfAdvertisementUrlsBlocked = Translation.GetPlural(
                    NETSHIELD_ADS_BLOCKED_TRANSLATION, stats.NumOfAdvertisementUrlsBlocked);

                NetShieldStatsNumOfTrackingUrlsBlocked = $"{stats.NumOfTrackingUrlsBlocked}";
                NetShieldStatsLabelOfTrackingUrlsBlocked = Translation.GetPlural(
                    NETSHIELD_TRACKERS_STOPPED_TRANSLATION, stats.NumOfTrackingUrlsBlocked);

                long adDataSavedInBytes = stats.NumOfAdvertisementUrlsBlocked * AVG_AD_SIZE_IN_BYTES;
                long trackerDataSavedInBytes = stats.NumOfTrackingUrlsBlocked * AVG_TRACKER_SIZE_IN_BYTES;
                long malwareDataSavedInBytes = stats.NumOfMaliciousUrlsBlocked * AVG_MALWARE_SIZE_IN_BYTES;
                long dataSavedInBytes = adDataSavedInBytes + trackerDataSavedInBytes + malwareDataSavedInBytes;
                NetShieldStatsDataSaved = dataSavedInBytes.ToShortString("B", UnitSpacing.WithSpace);
            }
        }
    }
}