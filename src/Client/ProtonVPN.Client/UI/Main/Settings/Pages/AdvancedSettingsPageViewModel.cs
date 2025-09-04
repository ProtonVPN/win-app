/*
 * Copyright (c) 2025 Proton AG
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Contracts.Profiles;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Common.Core.Dns;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.UI.Main.Settings.Pages;

public partial class AdvancedSettingsPageViewModel : SettingsPageViewModelBase,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<ProfilesChangedMessage>
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IFeatureFlagsObserver _featureFlagsObserver;

    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;
    private readonly IProfileEditor _profileEditor;

    [ObservableProperty]
    private bool _isAlternativeRoutingEnabled;

    [ObservableProperty]
    private bool _isLocalAreaNetworkAccessEnabled;

    [ObservableProperty]
    private bool _isIpv6LeakProtectionEnabled;

    [ObservableProperty]
    private bool _isIpv6Enabled;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.OpenVpnAdapter))]
    [NotifyPropertyChangedFor(nameof(IsTunAdapter))]
    [NotifyPropertyChangedFor(nameof(IsTapAdapter))]
    private OpenVpnAdapter _currentOpenVpnAdapter;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.NatType))]
    [NotifyPropertyChangedFor(nameof(IsStrictNatType))]
    [NotifyPropertyChangedFor(nameof(IsModerateNatType))]
    private NatType _currentNatType;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.DnsBlockMode))]
    [NotifyPropertyChangedFor(nameof(IsNrptDnsBlockMode))]
    [NotifyPropertyChangedFor(nameof(IsCalloutDnsBlockMode))]
    [NotifyPropertyChangedFor(nameof(IsDisabledDnsBlockModeVisible))]
    private DnsBlockMode _currentDnsBlockMode;

    public IConnectionProfile? CurrentProfile => ConnectionManager.CurrentConnectionIntent as IConnectionProfile;

    public bool AreSettingsOverridden => ConnectionManager.IsConnected && CurrentProfile != null;

    public string SettingsOverriddenTagline => AreSettingsOverridden
        ? Localizer.GetFormat("Settings_OverriddenByProfile_Tagline", CurrentProfile!.Name)
        : string.Empty;

    public override string Title => Localizer.Get("Settings_Connection_AdvancedSettings");

    public bool IsPaidUser => Settings.VpnPlan.IsPaid;

    public bool IsCustomDnsServersOverridden => AreSettingsOverridden
                                             && CurrentProfile!.Settings.IsCustomDnsServersEnabled.HasValue;

    public string NatTypeSettingsState => Localizer.GetNatType(NatType);

    public string CustomDnsServersSettingsState => Localizer.GetToggleValue(IsCustomDnsServersEnabled);

    public string NatTypeLearnMoreUrl => _urlsBrowser.NatTypeLearnMore;

    public string CustomDnsLearnMoreUrl => _urlsBrowser.CustomDnsLearnMore;

    public string CustomDnsConflictInformation => IsCustomDnsServersOverridden
        ? Settings.IsCustomDnsServersEnabled
            ? Localizer.Get("Settings_Connection_CustomDns_Conflict_Information_WhenEnabled")
            : Localizer.Get("Settings_Connection_CustomDns_Conflict_Information_WhenDisabled")
        : string.Empty;

    public string Ipv6LeakProtectionLearnMoreUrl => _urlsBrowser.Ipv6LeakProtectionLearnMore;

    public bool IsLocalAreaNetworkSettingVisible => _featureFlagsObserver.IsLocalAreaNetworkAllowedForPaidUsersOnly;

    public bool IsIpv6SettingVisible => _featureFlagsObserver.IsIpv6SupportEnabled;

    public bool IsStrictNatType
    {
        get => IsNatType(NatType.Strict);
        set => SetNatType(value, NatType.Strict);
    }

    public bool IsModerateNatType
    {
        get => IsNatType(NatType.Moderate);
        set => SetNatType(value, NatType.Moderate);
    }

    public bool IsTunAdapter
    {
        get => IsOpenVpnAdapter(OpenVpnAdapter.Tun);
        set => SetOpenVpnAdapter(value, OpenVpnAdapter.Tun);
    }

    public bool IsTapAdapter
    {
        get => IsOpenVpnAdapter(OpenVpnAdapter.Tap);
        set => SetOpenVpnAdapter(value, OpenVpnAdapter.Tap);
    }

    protected NatType NatType => AreSettingsOverridden
        ? CurrentProfile!.Settings.NatType
        : CurrentNatType;

    protected bool IsCustomDnsServersEnabled => IsCustomDnsServersOverridden
        ? CurrentProfile!.Settings.IsCustomDnsServersEnabled!.Value
        : Settings.IsCustomDnsServersEnabled;

    public bool IsNrptDnsBlockMode
    {
        get => IsDnsBlockMode(DnsBlockMode.Nrpt);
        set => SetDnsBlockMode(value, DnsBlockMode.Nrpt);
    }

    public bool IsCalloutDnsBlockMode
    {
        get => IsDnsBlockMode(DnsBlockMode.Callout);
        set => SetDnsBlockMode(value, DnsBlockMode.Callout);
    }

    public bool IsDisabledDnsBlockMode
    {
        get => IsDnsBlockMode(DnsBlockMode.Disabled);
        set => SetDnsBlockMode(value, DnsBlockMode.Disabled);
    }

    public bool IsDisabledDnsBlockModeVisible => IsDisabledDnsBlockMode || Settings.DnsBlockMode == DnsBlockMode.Disabled;

    public string Recommended => Localizer.Get("Common_Tags_Recommended").ToUpperInvariant();

    public AdvancedSettingsPageViewModel(
        IUrlsBrowser urlsBrowser,
        IFeatureFlagsObserver featureFlagsObserver,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IProfileEditor profileEditor,
        IViewModelHelper viewModelHelper)
        : base(requiredReconnectionSettings,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager,
               viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;
        _featureFlagsObserver = featureFlagsObserver;
        _upsellCarouselWindowActivator = upsellCarouselWindowActivator;
        _profileEditor = profileEditor;

        PageSettings =
        [
            ChangedSettingArgs.Create(() => Settings.NatType, () => CurrentNatType),
            ChangedSettingArgs.Create(() => Settings.IsAlternativeRoutingEnabled, () => IsAlternativeRoutingEnabled),
            ChangedSettingArgs.Create(() => Settings.OpenVpnAdapter, () => CurrentOpenVpnAdapter),
            ChangedSettingArgs.Create(() => Settings.IsIpv6LeakProtectionEnabled, () => IsIpv6LeakProtectionEnabled),
            ChangedSettingArgs.Create(() => Settings.IsLocalAreaNetworkAccessEnabled, () => IsLocalAreaNetworkAccessEnabled),
            ChangedSettingArgs.Create(() => Settings.IsIpv6Enabled, () => IsIpv6Enabled),
            ChangedSettingArgs.Create(() => Settings.DnsBlockMode, () => CurrentDnsBlockMode),
        ];
    }

    public void Receive(LoggedInMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    public void Receive(ProfilesChangedMessage message)
    {
        if (IsActive && AreSettingsOverridden)
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(ISettings.IsCustomDnsServersEnabled):
                OnPropertyChanged(nameof(CustomDnsServersSettingsState));
                OnPropertyChanged(nameof(CustomDnsConflictInformation));
                break;

            case nameof(ISettings.NatType):
                OnPropertyChanged(nameof(NatTypeSettingsState));
                break;

            case nameof(ISettings.DnsBlockMode):
                OnPropertyChanged(nameof(IsDisabledDnsBlockModeVisible));
                break;
        }
    }

    protected override void OnConnectionStatusChanged(ConnectionStatus connectionStatus)
    {
        base.OnConnectionStatusChanged(connectionStatus);

        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(CustomDnsServersSettingsState));
        OnPropertyChanged(nameof(NatTypeSettingsState));
        OnPropertyChanged(nameof(SettingsOverriddenTagline));
        OnPropertyChanged(nameof(Recommended));
    }

    protected override void OnRetrieveSettings()
    {
        CurrentNatType = Settings.NatType;
        IsAlternativeRoutingEnabled = Settings.IsAlternativeRoutingEnabled;
        IsIpv6LeakProtectionEnabled = Settings.IsIpv6LeakProtectionEnabled;
        IsIpv6Enabled = Settings.IsIpv6Enabled;
        CurrentOpenVpnAdapter = Settings.OpenVpnAdapter;
        IsLocalAreaNetworkAccessEnabled = Settings.IsLocalAreaNetworkAccessEnabled;
        CurrentDnsBlockMode = Settings.DnsBlockMode;
    }

    [RelayCommand]
    private Task NavigateToCustomDnsServersPageAsync()
    {
        return IsPaidUser
            ? IsCustomDnsServersOverridden
                ? _profileEditor.TryRedirectToProfileAsync(Localizer.Get("Settings_Connection_Advanced_CustomDnsServers"), CurrentProfile!)
                : ParentViewNavigator.NavigateToCustomDnsSettingsViewAsync()
            : TriggerAdvancedSettingsUpsellProcessAsync(UpsellFeatureType.CustomDns);
    }

    [RelayCommand]
    private Task TriggerLanConnectonsUpsellProcessAsync()
    {
        return TriggerAdvancedSettingsUpsellProcessAsync(UpsellFeatureType.AllowLanConnections);
    }

    [RelayCommand]
    private Task TriggerNatTypeUpsellProcessAsync()
    {
        return TriggerAdvancedSettingsUpsellProcessAsync(UpsellFeatureType.ModerateNat);
    }

    [RelayCommand]
    private Task HandleNatTypeOverriddenByProfileAsync()
    {
        return _profileEditor.TryRedirectToProfileAsync(Localizer.Get("Settings_Connection_Advanced_NatType"), CurrentProfile!);
    }

    [RelayCommand]
    private Task TriggerDnsBlockModeUpsellProcessAsync()
    {
        return TriggerAdvancedSettingsUpsellProcessAsync(UpsellFeatureType.AllowLanConnections);
    }

    private Task TriggerAdvancedSettingsUpsellProcessAsync(UpsellFeatureType upsellFeatureType)
    {
        return _upsellCarouselWindowActivator.ActivateAsync(upsellFeatureType);
    }

    private bool IsNatType(NatType natType)
    {
        return CurrentNatType == natType;
    }

    private void SetNatType(bool value, NatType natType)
    {
        if (value)
        {
            CurrentNatType = natType;
        }
    }

    private bool IsOpenVpnAdapter(OpenVpnAdapter adapter)
    {
        return CurrentOpenVpnAdapter == adapter;
    }

    private void SetOpenVpnAdapter(bool value, OpenVpnAdapter adapter)
    {
        if (value)
        {
            CurrentOpenVpnAdapter = adapter;
        }
    }

    private bool IsDnsBlockMode(DnsBlockMode dnsBlockMode)
    {
        return CurrentDnsBlockMode == dnsBlockMode;
    }

    private void SetDnsBlockMode(bool value, DnsBlockMode dnsBlockMode)
    {
        if (value)
        {
            CurrentDnsBlockMode = dnsBlockMode;
        }
    }
}