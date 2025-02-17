/*
 * Copyright (c) 2024 Proton AG
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
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Settings.Pages;

public partial class AdvancedSettingsPageViewModel : SettingsPageViewModelBase,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<ProfilesChangedMessage>
{
    private readonly IUrlsBrowser _urlsBrowser;

    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;
    private readonly IProfileEditor _profileEditor;

    [ObservableProperty]
    private bool _isAlternativeRoutingEnabled;

    [ObservableProperty]
    private bool _isIpv6LeakProtectionEnabled;

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

    public AdvancedSettingsPageViewModel(
        IUrlsBrowser urlsBrowser,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IProfileEditor profileEditor)
        : base(requiredReconnectionSettings,
               mainViewNavigator,
               settingsViewNavigator,
               localizer,
               logger,
               issueReporter,
               mainWindowOverlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager)
    {
        _urlsBrowser = urlsBrowser;
        _upsellCarouselWindowActivator = upsellCarouselWindowActivator;
        _profileEditor = profileEditor;

        PageSettings =
        [
            ChangedSettingArgs.Create(() => Settings.NatType, () => CurrentNatType),
            ChangedSettingArgs.Create(() => Settings.IsAlternativeRoutingEnabled, () => IsAlternativeRoutingEnabled),
            ChangedSettingArgs.Create(() => Settings.OpenVpnAdapter, () => CurrentOpenVpnAdapter),
            ChangedSettingArgs.Create(() => Settings.IsIpv6LeakProtectionEnabled, () => IsIpv6LeakProtectionEnabled),
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
    }

    protected override void OnRetrieveSettings()
    {
        CurrentNatType = Settings.NatType;
        IsAlternativeRoutingEnabled = Settings.IsAlternativeRoutingEnabled;
        IsIpv6LeakProtectionEnabled = Settings.IsIpv6LeakProtectionEnabled;
        CurrentOpenVpnAdapter = Settings.OpenVpnAdapter;
    }

    [RelayCommand]
    private Task NavigateToCustomDnsServersPageAsync()
    {
        return IsPaidUser
            ? IsCustomDnsServersOverridden
                ? _profileEditor.TryRedirectToProfileAsync(Localizer.Get("Settings_Connection_Advanced_CustomDnsServers"), CurrentProfile!)
                : ParentViewNavigator.NavigateToCustomDnsSettingsViewAsync()
            : _upsellCarouselWindowActivator.ActivateAsync(UpsellFeatureType.AdvancedSettings);
    }

    [RelayCommand]
    private Task HandleNatTypeOverriddenByProfileAsync()
    {
        return _profileEditor.TryRedirectToProfileAsync(Localizer.Get("Settings_Connection_Advanced_NatType"), CurrentProfile!);
    }

    [RelayCommand]
    private Task TriggerNatTypeUpsellProcessAsync()
    {
        return _upsellCarouselWindowActivator.ActivateAsync(UpsellFeatureType.AdvancedSettings);
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
}