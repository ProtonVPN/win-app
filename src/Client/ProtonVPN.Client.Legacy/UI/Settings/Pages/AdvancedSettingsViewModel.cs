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

using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Client.Common.Attributes;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Legacy.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Legacy.Models.Activation;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Legacy.UI.Settings.Pages.Advanced;
using ProtonVPN.Client.Legacy.UI.Settings.Pages.Entities;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Legacy.Models.Activation.Custom;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;

namespace ProtonVPN.Client.Legacy.UI.Settings.Pages;

public partial class AdvancedSettingsViewModel : SettingsPageViewModelBase,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    public List<OpenVpnAdapter> OpenVpnAdapters =
    [
        OpenVpnAdapter.Tap,
        OpenVpnAdapter.Tun
    ];

    private readonly IUrls _urls;
    private readonly IUpsellCarouselDialogActivator _upsellCarouselDialogActivator;
    [ObservableProperty]
    private bool _isAlternativeRoutingEnabled;

    [ObservableProperty]
    private bool _isIpv6LeakProtectionEnabled;

    [ObservableProperty]
    private OpenVpnAdapter _selectedOpenVpnAdapter;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.NatType))]
    [NotifyPropertyChangedFor(nameof(IsStrictNatType))]
    [NotifyPropertyChangedFor(nameof(IsModerateNatType))]
    private NatType _currentNatType;

    public override string? Title => Localizer.Get("Settings_Connection_AdvancedSettings");

    public bool IsPaidUser => Settings.VpnPlan.IsPaid;

    public string CustomDnsServersSettingsState => Localizer.GetToggleValue(Settings.IsCustomDnsServersEnabled);

    public string NatTypeLearnMoreUrl => _urls.NatTypeLearnMore;

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

    public AdvancedSettingsViewModel(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IOverlayActivator overlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IUrls urls,
        IConnectionManager connectionManager,
        ILogger logger,
        IIssueReporter issueReporter,
        IUpsellCarouselDialogActivator upsellCarouselDialogActivator)
        : base(viewNavigator,
               localizationProvider,
               overlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager,
               logger,
               issueReporter)
    {
        _urls = urls;
        _upsellCarouselDialogActivator = upsellCarouselDialogActivator;
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
        Settings.OpenVpnAdapter = SelectedOpenVpnAdapter;
    }

    protected override void OnRetrieveSettings()
    {
        CurrentNatType = Settings.NatType;
        IsAlternativeRoutingEnabled = Settings.IsAlternativeRoutingEnabled;
        IsIpv6LeakProtectionEnabled = Settings.IsIpv6LeakProtectionEnabled;
        SelectedOpenVpnAdapter = Settings.OpenVpnAdapter;
    }

    protected override IEnumerable<ChangedSettingArgs> GetSettings()
    {
        yield return new(nameof(ISettings.NatType), CurrentNatType, Settings.NatType != CurrentNatType);
        yield return new(nameof(ISettings.IsAlternativeRoutingEnabled), IsAlternativeRoutingEnabled,
            Settings.IsAlternativeRoutingEnabled != IsAlternativeRoutingEnabled);
        yield return new(nameof(ISettings.OpenVpnAdapter), SelectedOpenVpnAdapter, Settings.OpenVpnAdapter != SelectedOpenVpnAdapter);
        yield return new(nameof(ISettings.IsIpv6LeakProtectionEnabled), IsIpv6LeakProtectionEnabled,
            Settings.IsIpv6LeakProtectionEnabled != IsIpv6LeakProtectionEnabled);
    }

    [RelayCommand]
    private async Task NavigateToCustomDnsServersPageAsync()
    {
        if (!IsPaidUser)
        {
            _upsellCarouselDialogActivator.ShowDialog(ModalSources.CustomDns);
            return;
        }

        await ViewNavigator.NavigateToAsync<CustomDnsServersViewModel>();
    }

    [RelayCommand]
    private void TriggerNatTypeUpsellProcess()
    {
        _upsellCarouselDialogActivator.ShowDialog(ModalSources.ModerateNat);
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
}