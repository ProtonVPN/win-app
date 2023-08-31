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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Account;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Config.Url;
using ProtonVPN.Core;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals;
using ProtonVPN.Modals.Upsell;
using ProtonVPN.PortForwarding;
using ProtonVPN.PortForwarding.ActivePorts;
using ProtonVPN.Profiles;
using ProtonVPN.Resource;
using ProtonVPN.Settings.ReconnectNotification;
using ProtonVPN.Settings.SplitTunneling;
using ProtonVPN.Translations;

namespace ProtonVPN.Settings
{
    public class SettingsModalViewModel : BaseModalViewModel, IVpnStateAware, IVpnPlanAware
    {
        private readonly IUserStorage _userStorage;
        private readonly IAppSettings _appSettings;
        private readonly IVpnManager _vpnManager;
        private readonly IDialogs _dialogs;
        private readonly IModals _modals;
        private readonly IActiveUrls _urls;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly ILanguageProvider _languageProvider;
        private readonly IPortForwardingManager _portForwardingManager;
        private readonly ReconnectState _reconnectState;
        private readonly ProfileViewModelFactory _profileViewModelFactory;

        private IReadOnlyList<ProfileViewModel> _quickConnectProfiles;
        private VpnStatus _vpnStatus;

        public SettingsModalViewModel(
            IUserStorage userStorage,
            IAppSettings appSettings,
            IVpnManager vpnManager,
            IDialogs dialogs,
            IModals modals,
            IActiveUrls urls,
            ISubscriptionManager subscriptionManager,
            ILanguageProvider languageProvider,
            IPortForwardingManager portForwardingManager,
            ReconnectState reconnectState,
            ProfileViewModelFactory profileViewModelFactory,
            SplitTunnelingViewModel splitTunnelingViewModel,
            CustomDnsListViewModel customDnsListViewModel,
            PortForwardingActivePortViewModel activePortViewModel,
            PortForwardingWarningViewModel portForwardingWarningViewModel)
        {
            _userStorage = userStorage;
            _appSettings = appSettings;
            _vpnManager = vpnManager;
            _dialogs = dialogs;
            _modals = modals;
            _urls = urls;
            _subscriptionManager = subscriptionManager;
            _languageProvider = languageProvider;
            _portForwardingManager = portForwardingManager;
            _reconnectState = reconnectState;

            _profileViewModelFactory = profileViewModelFactory;
            SplitTunnelingViewModel = splitTunnelingViewModel;
            Ips = customDnsListViewModel;

            ActivePortViewModel = activePortViewModel;
            ActivePortViewModel.PropertyChanged += OnActivePortViewModelPropertyChanged;
            PortForwardingWarningViewModel = portForwardingWarningViewModel;

            ReconnectCommand = new RelayCommand(ReconnectAction);
            UpgradeCommand = new RelayCommand(UpgradeAction);
            ShowVpnAcceleratorUpgradeModalCommand = new RelayCommand(ShowUpgradeModalActionAsync<VpnAcceleratorUpsellModalViewModel>);
            ShowSplitTunnelingUpgradeModalCommand = new RelayCommand(ShowUpgradeModalActionAsync<SplitTunnelingUpsellModalViewModel>);
            ShowPortForwardingUpgradeModalCommand = new RelayCommand(() => PortForwarding = true);
            ShowCustomDnsUpgradeModalCommand = new RelayCommand(ShowUpgradeModalActionAsync<CustomDnsUpsellModalViewModel>);
            ShowModerateNatUpgradeModalCommand = new RelayCommand(ShowUpgradeModalActionAsync<ModerateNatUpsellModalViewModel>);
            LearnMoreAboutPortForwardingCommand = new RelayCommand(LearnMoreAboutPortForwardingAction);
        }

        public PortForwardingActivePortViewModel ActivePortViewModel { get; }
        public PortForwardingWarningViewModel PortForwardingWarningViewModel { get; }

        public ICommand ReconnectCommand { get; set; }
        public ICommand UpgradeCommand { get; set; }
        public ICommand ShowVpnAcceleratorUpgradeModalCommand { get; set; }
        public ICommand ShowSplitTunnelingUpgradeModalCommand { get; set; }
        public ICommand ShowPortForwardingUpgradeModalCommand { get; set; }
        public ICommand ShowCustomDnsUpgradeModalCommand { get; set; }
        public ICommand ShowModerateNatUpgradeModalCommand { get; set; }
        public ICommand LearnMoreAboutPortForwardingCommand { get; set; }

        public IpListViewModel Ips { get; }

        private bool _changesPending;

        public bool ChangesPending
        {
            get => _changesPending;
            private set => Set(ref _changesPending, value);
        }

        private bool _disconnected;

        public bool Disconnected
        {
            get => _disconnected;
            private set
            {
                Set(ref _disconnected, value);
                SplitTunnelingViewModel.Disconnected = value;
            }
        }

        public bool IsPaidUser => _userStorage.GetUser().Paid();

        public bool IsToShowPaidFeatureToggleButton => IsPaidUser || !_appSettings.FeatureFreeRescopeEnabled;

        public bool IsToShowPaidFeatureUpgradeButton => !IsToShowPaidFeatureToggleButton;

        public bool IsToShowPortForwardingSubSettings => _appSettings.FeaturePortForwardingEnabled &&
                                                         (IsPaidUser || !_appSettings.FeatureFreeRescopeEnabled);

        public bool IsToShowNetworkDriverSelection => _appSettings.GetProtocol() != VpnProtocol.WireGuard;

        public bool IsToShowPortForwardingWarningLabel => PortForwardingWarningViewModel.IsToShowPortForwardingWarningLabel;

        public int SelectedTabIndex
        {
            get => _appSettings.SettingsSelectedTabIndex;
            set
            {
                _appSettings.SettingsSelectedTabIndex = value;
                NotifyOfPropertyChange();
            }
        }

        private ProfileViewModel _quickConnect;

        public ProfileViewModel QuickConnect
        {
            get => _quickConnect;
            set
            {
                if (value == null)
                {
                    return;
                }

                Set(ref _quickConnect, value);
                _appSettings.QuickConnect = value.Id;
            }
        }

        public bool Ipv6LeakProtection
        {
            get => _appSettings.Ipv6LeakProtection;
            set
            {
                _appSettings.Ipv6LeakProtection = value;
                NotifyOfPropertyChange();
            }
        }

        public bool AllowNonStandardPorts
        {
            get => _appSettings.AllowNonStandardPorts;
            set
            {
                if (value && !IsPaidUser)
                {
                    ShowNonStandardPortsUpsellModal();
                    return;
                }

                _appSettings.AllowNonStandardPorts = value;
                NotifyOfPropertyChange();
            }
        }

        private async void ShowNonStandardPortsUpsellModal()
        {
            await _modals.ShowAsync<NonStandardPortsUpsellModalViewModel>();
        }

        public bool ShowAllowNonStandardPorts => _appSettings.ShowNonStandardPortsToFreeUsers;

        public bool ModerateNat
        {
            get => _appSettings.ModerateNat;
            set
            {
                if (value && !IsPaidUser)
                {
                    ShowModerateNatUpsellModal();
                    return;
                }

                _appSettings.ModerateNat = value;
                NotifyOfPropertyChange();
            }
        }

        private async void ShowModerateNatUpsellModal()
        {
            await _modals.ShowAsync<ModerateNatUpsellModalViewModel>();
        }

        public bool VpnAccelerator
        {
            get => _appSettings.VpnAcceleratorEnabled;
            set
            {
                _appSettings.VpnAcceleratorEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsVpnAcceleratorFeatureEnabled
        {
            get
            {
                return _appSettings.FeatureVpnAcceleratorEnabled;
            }
        }

        public bool SmartReconnect
        {
            get => _appSettings.SmartReconnectEnabled;
            set
            {
                _appSettings.SmartReconnectEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsToShowSmartReconnect
        {
            get
            {
                return _appSettings.FeatureVpnAcceleratorEnabled && _appSettings.FeatureSmartReconnectEnabled && IsPaidUser;
            }
        }

        public bool IsToShowSmartReconnectNotifications
        {
            get
            {
                return _appSettings.FeatureVpnAcceleratorEnabled && _appSettings.FeatureSmartReconnectEnabled && ShowNotifications && IsPaidUser;
            }
        }

        public bool IsSmartReconnectNotificationsEditable
        {
            get
            {
                return _appSettings.SmartReconnectEnabled && IsPaidUser;
            }
        }

        public bool SmartReconnectNotifications
        {
            get => _appSettings.SmartReconnectNotificationsEnabled;
            set
            {
                _appSettings.SmartReconnectNotificationsEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool PortForwarding
        {
            get => _appSettings.PortForwardingEnabled;
            set
            {
                if (_appSettings.PortForwardingEnabled != value)
                {
                    if (value)
                    {
                        _portForwardingManager.EnableAsync().Wait();
                    }
                    else
                    {
                        _portForwardingManager.DisableAsync().Wait();
                    }
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsToShowPortForwarding
        {
            get => _appSettings.FeaturePortForwardingEnabled;
        }

        public bool PortForwardingInQuickSettings
        {
            get => _appSettings.PortForwardingInQuickSettings;
            set
            {
                _appSettings.PortForwardingInQuickSettings = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsToShowPortForwardingNotifications
        {
            get
            {
                return _appSettings.FeaturePortForwardingEnabled && ShowNotifications;
            }
        }

        public bool PortForwardingNotifications
        {
            get => _appSettings.PortForwardingNotificationsEnabled;
            set
            {
                _appSettings.PortForwardingNotificationsEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool HasPortForwardingValue => ActivePortViewModel.HasPortForwardingValue;

        public bool DoHEnabled
        {
            get => _appSettings.DoHEnabled;
            set
            {
                _appSettings.DoHEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool CustomDnsEnabled
        {
            get => _appSettings.CustomDnsEnabled;
            set
            {
                if (value && _appSettings.IsNetShieldEnabled())
                {
                    bool? result = _dialogs.ShowQuestionAsync(
                        Translation.Get("Settings_Connection_Warning_CustomDnsServer")).Result;
                    if (result.HasValue && !result.Value)
                    {
                        return;
                    }

                    _appSettings.NetShieldEnabled = false;
                }

                _appSettings.CustomDnsEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public List<KeyValuePair<StartMinimizedMode, string>> StartMinimizedModes => new()
        {
            new(StartMinimizedMode.Disabled, Translation.Get("StartMinimizedMode_val_Disabled")),
            new(StartMinimizedMode.ToSystray, Translation.Get("StartMinimizedMode_val_ToSystray")),
            new(StartMinimizedMode.ToTaskbar, Translation.Get("StartMinimizedMode_val_ToTaskbar")),
        };

        public StartMinimizedMode StartMinimized
        {
            get => _appSettings.StartMinimized;
            set => _appSettings.StartMinimized = value;
        }

        public bool EarlyAccess
        {
            get => _appSettings.EarlyAccess;
            set => _appSettings.EarlyAccess = value;
        }

        public bool AutoUpdate
        {
            get => _appSettings.IsToAutoUpdate;
            set => _appSettings.IsToAutoUpdate = value;
        }

        public bool ConnectOnAppStart
        {
            get => _appSettings.ConnectOnAppStart;
            set => _appSettings.ConnectOnAppStart = value;
        }

        public bool ShowNotifications
        {
            get => _appSettings.ShowNotifications;
            set
            {
                _appSettings.ShowNotifications = value;
                NotifyOfPropertyChange();
            }
        }

        public bool StartOnBoot
        {
            get => _appSettings.StartOnBoot;
            set => _appSettings.StartOnBoot = value;
        }

        public bool HardwareAccelerationEnabled
        {
            get => _appSettings.HardwareAccelerationEnabled;
            set => _appSettings.HardwareAccelerationEnabled = value;
        }

        public string SelectedProtocol
        {
            get => _appSettings.OvpnProtocol;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                _appSettings.OvpnProtocol = value;
            }
        }

        public List<LanguageViewModel> Languages => GetLanguages();

        private List<LanguageViewModel> GetLanguages()
        {
            List<string> languageCodes = _languageProvider.GetAll();
            List<LanguageViewModel> languageViewModels = new();
            foreach (string languageCode in languageCodes)
            {
                string title = StringResource.Get($"Language_{languageCode}");
                if (!title.IsNullOrEmpty())
                {
                    languageViewModels.Add(new() { Code = languageCode, Title = title });
                }
            }

            return languageViewModels.OrderBy(l => l.Code).ToList();
        }

        public string SelectedLanguage
        {
            get => _appSettings.Language;
            set
            {
                if (value == null || _appSettings.Language == value)
                {
                    return;
                }

                _appSettings.Language = value;
            }
        }

        public List<KeyValuePair<string, string>> Protocols => new()
        {
            new("auto", Translation.Get("Settings_Connection_DefaultProtocol_val_Smart")),
            new("wireguard", Translation.Get("Settings_Connection_DefaultProtocol_val_WireGuard")),
            new("udp", Translation.Get("Settings_Connection_DefaultProtocol_val_Udp")),
            new("tcp", Translation.Get("Settings_Connection_DefaultProtocol_val_Tcp")),
        };

        public List<KeyValuePair<OpenVpnAdapter, string>> NetworkDrivers => new()
        {
            new(OpenVpnAdapter.Tap, Translation.Get("Settings_Advanced_lbl_OpenVpnTap")),
            new(OpenVpnAdapter.Tun, Translation.Get("Settings_Advanced_lbl_OpenVpnTun")),
        };

        public OpenVpnAdapter SelectedOpenVpnAdapter
        {
            get => _appSettings.NetworkAdapterType;
            set => _appSettings.NetworkAdapterType = value;
        }

        public IReadOnlyList<ProfileViewModel> QuickConnectProfiles
        {
            get => _quickConnectProfiles;
            set => Set(ref _quickConnectProfiles, value);
        }

        public SplitTunnelingViewModel SplitTunnelingViewModel { get; }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            SetDisconnected();
            await LoadProfiles();
            SplitTunnelingViewModel.OnActivate();
            RefreshReconnectRequiredState(string.Empty);
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            SetDisconnected();

            return Task.CompletedTask;
        }

        public void OpenConnectionTab()
        {
            SelectedTabIndex = 1;
        }

        public override async void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            base.OnAppSettingsChanged(e);

            if (e.PropertyName.Equals(nameof(IAppSettings.StartOnBoot)))
            {
                NotifyOfPropertyChange(nameof(StartOnBoot));
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.Profiles)))
            {
                await LoadProfiles();
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.Language)))
            {
                OnLanguageChanged();
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.ShowNotifications)))
            {
                OnShowNotificationsChanged();
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.FeatureNetShieldEnabled)) ||
                     e.PropertyName.Equals(nameof(IAppSettings.NetShieldMode)) ||
                     e.PropertyName.Equals(nameof(IAppSettings.NetShieldEnabled)))
            {
                if (_appSettings.IsNetShieldEnabled())
                {
                    _appSettings.CustomDnsEnabled = false;
                }
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.FeatureVpnAcceleratorEnabled)))
            {
                NotifyOfPropertyChange(() => IsVpnAcceleratorFeatureEnabled);
                NotifyOfPropertyChange(() => IsToShowSmartReconnect);
                NotifyOfPropertyChange(() => IsToShowSmartReconnectNotifications);
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.FeatureSmartReconnectEnabled)))
            {
                NotifyOfPropertyChange(() => IsToShowSmartReconnect);
                NotifyOfPropertyChange(() => IsToShowSmartReconnectNotifications);
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.FeaturePortForwardingEnabled)))
            {
                NotifyOfPropertyChange(() => IsToShowPortForwarding);
                NotifyOfPropertyChange(() => IsToShowPortForwardingNotifications);
                NotifyOfPropertyChange(() => IsToShowPortForwardingWarningLabel);
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.PortForwardingEnabled)))
            {
                NotifyOfPropertyChange(() => IsToShowPortForwardingWarningLabel);
                NotifyOfPropertyChange(() => IsToShowPortForwardingSubSettings);
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.SmartReconnectEnabled)))
            {
                NotifyOfPropertyChange(() => IsSmartReconnectNotificationsEditable);
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.OvpnProtocol)))
            {
                NotifyOfPropertyChange(() => IsToShowNetworkDriverSelection);
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.ModerateNat)))
            {
                NotifyOfPropertyChange(() => ModerateNat);
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.AllowNonStandardPorts)))
            {
                NotifyOfPropertyChange(() => AllowNonStandardPorts);
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.HardwareAccelerationEnabled)))
            {
                HardwareAccelerationManager.Set(HardwareAccelerationEnabled);
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.CustomDnsEnabled)))
            {
                NotifyOfPropertyChange(() => CustomDnsEnabled);
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.PortForwardingInQuickSettings)))
            {
                NotifyOfPropertyChange(() => PortForwardingInQuickSettings);
            }
            else if (e.PropertyName.Equals(nameof(IAppSettings.FeatureFreeRescopeEnabled)))
            {
                NotifyUserPlanRelatedSettings();
            }

            RefreshReconnectRequiredState(e.PropertyName);
        }

        private void NotifyUserPlanRelatedSettings()
        {
            NotifyOfPropertyChange(() => IsToShowPaidFeatureToggleButton);
            NotifyOfPropertyChange(() => IsToShowPaidFeatureUpgradeButton);
            NotifyOfPropertyChange(() => IsToShowPortForwardingSubSettings);
        }

        private void OnShowNotificationsChanged()
        {
            NotifyOfPropertyChange(() => ShowNotifications);
            NotifyOfPropertyChange(() => IsToShowSmartReconnectNotifications);
            NotifyOfPropertyChange(() => IsToShowPortForwardingNotifications);
        }

        public async void OnLanguageChanged()
        {
            NotifyOfPropertyChange(() => Languages);

            NotifyOfPropertyChange(() => Protocols);
            NotifyOfPropertyChange(() => SelectedProtocol);

            NotifyOfPropertyChange(() => NetworkDrivers);
            NotifyOfPropertyChange(() => SelectedOpenVpnAdapter);

            NotifyOfPropertyChange(() => StartMinimizedModes);
            NotifyOfPropertyChange(() => StartMinimized);
            
            await LoadProfiles();
        }

        public async Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            NotifyUserPlanRelatedSettings();
        }

        private void SetDisconnected()
        {
            Disconnected = _vpnStatus == VpnStatus.Disconnecting ||
                           _vpnStatus == VpnStatus.Disconnected;
        }

        private void RefreshReconnectRequiredState(string settingChanged)
        {
            ChangesPending = _reconnectState.Required(settingChanged);
        }

        private async Task LoadProfiles()
        {
            await LoadQuickConnectProfiles();

            QuickConnect = GetSelectedQuickConnectProfile();
            NotifyOfPropertyChange(() => QuickConnect);
        }

        private async Task LoadQuickConnectProfiles()
        {
            QuickConnectProfiles = await GetProfiles();
            NotifyOfPropertyChange(() => QuickConnectProfiles);
        }

        private async Task<List<ProfileViewModel>> GetProfiles()
        {
            return (await _profileViewModelFactory.GetProfiles())
                .OrderByDescending(p => p.IsPredefined)
                .ThenBy(p => p.Name)
                .ToList();
        }

        private ProfileViewModel GetSelectedQuickConnectProfile()
        {
            ProfileViewModel profile = QuickConnectProfiles.FirstOrDefault(p => p.Id == _appSettings.QuickConnect);
            if (profile != null)
            {
                return profile;
            }

            return QuickConnectProfiles.FirstOrDefault(p => p.IsPredefined && p.Id == "Fastest");
        }

        private async void ReconnectAction()
        {
            await _vpnManager.ReconnectAsync();
        }

        private void UpgradeAction()
        {
            _subscriptionManager.UpgradeAccountAsync();
        }

        private async void ShowUpgradeModalActionAsync<T>() where T : UpsellModalViewModel
        {
            await _modals.ShowAsync<T>();
        }

        private void LearnMoreAboutPortForwardingAction()
        {
            _urls.AboutPortForwardingUrl.Open();
        }

        private void OnActivePortViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyOfPropertyChange(e.PropertyName);
        }
    }
}