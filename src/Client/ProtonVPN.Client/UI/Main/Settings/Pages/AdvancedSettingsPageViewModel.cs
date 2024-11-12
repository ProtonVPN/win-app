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
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Settings.Pages.Entities;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Settings.Pages;

public partial class AdvancedSettingsPageViewModel : SettingsPageViewModelBase,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly IUrls _urls;

    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;
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

    public override string Title => Localizer.Get("Settings_Connection_AdvancedSettings");

    public bool IsPaidUser => Settings.VpnPlan.IsPaid;

    public string CustomDnsServersSettingsState => Localizer.GetToggleValue(Settings.IsCustomDnsServersEnabled);

    public string NatTypeLearnMoreUrl => _urls.NatTypeLearnMore;

    public string Ipv6LeakProtectionLearnMoreUrl => _urls.Ipv6LeakProtectionLearnMore;

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

    public AdvancedSettingsPageViewModel(
        IUrls urls,
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
        IConnectionManager connectionManager)
        : base(
            requiredReconnectionSettings,
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
        _urls = urls;
        _upsellCarouselWindowActivator = upsellCarouselWindowActivator;
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(ISettings.IsCustomDnsServersEnabled):
                OnPropertyChanged(nameof(CustomDnsServersSettingsState));
                break;
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(CustomDnsServersSettingsState));
    }

    protected override void OnSaveSettings()
    {
        Settings.NatType = CurrentNatType;
        Settings.IsAlternativeRoutingEnabled = IsAlternativeRoutingEnabled;
        Settings.IsIpv6LeakProtectionEnabled = IsIpv6LeakProtectionEnabled;
        Settings.OpenVpnAdapter = CurrentOpenVpnAdapter;
    }

    protected override void OnRetrieveSettings()
    {
        CurrentNatType = Settings.NatType;
        IsAlternativeRoutingEnabled = Settings.IsAlternativeRoutingEnabled;
        IsIpv6LeakProtectionEnabled = Settings.IsIpv6LeakProtectionEnabled;
        CurrentOpenVpnAdapter = Settings.OpenVpnAdapter;
    }

    protected override IEnumerable<ChangedSettingArgs> GetSettings()
    {
        yield return new(nameof(ISettings.NatType), CurrentNatType, Settings.NatType != CurrentNatType);
        yield return new(nameof(ISettings.IsAlternativeRoutingEnabled), IsAlternativeRoutingEnabled,
            Settings.IsAlternativeRoutingEnabled != IsAlternativeRoutingEnabled);
        yield return new(nameof(ISettings.OpenVpnAdapter), CurrentOpenVpnAdapter,
            Settings.OpenVpnAdapter != CurrentOpenVpnAdapter);
        yield return new(nameof(ISettings.IsIpv6LeakProtectionEnabled), IsIpv6LeakProtectionEnabled,
            Settings.IsIpv6LeakProtectionEnabled != IsIpv6LeakProtectionEnabled);
    }

    [RelayCommand]
    private async Task NavigateToCustomDnsServersPageAsync()
    {
        if (!IsPaidUser)
        {
            _upsellCarouselWindowActivator.Activate();
            return;
        }

        await ParentViewNavigator.NavigateToCustomDnsSettingsViewAsync();
    }

    [RelayCommand]
    private void TriggerNatTypeUpsellProcess()
    {
        _upsellCarouselWindowActivator.Activate();
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