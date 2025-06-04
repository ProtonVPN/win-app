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

using ProtonVPN.Client.Core.Bases;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.Profiles;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Features.Bases;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Client.UI.Main.Settings.Connection;

namespace ProtonVPN.Client.UI.Main.Features.KillSwitch;

public partial class KillSwitchWidgetViewModel : FeatureWidgetViewModelBase
{
    private readonly Lazy<List<ChangedSettingArgs>> _disableKillSwitchSettings;
    private readonly Lazy<List<ChangedSettingArgs>> _enableStandardKillSwitchSettings;
    private readonly Lazy<List<ChangedSettingArgs>> _enableAdvancedKillSwitchSettings;

    public override string Header => Localizer.Get("Settings_Connection_KillSwitch");

    public string InfoMessage => Localizer.Get("Flyouts_KillSwitch_Info");

    public string WarningMessage => ConnectionManager.IsConnected
        ? Localizer.Get("Flyouts_KillSwitch_Warning_Connected")
        : Localizer.Get("Flyouts_KillSwitch_Warning_Disconnected");

    public string SuccessMessage => KillSwitchMode == KillSwitchMode.Advanced
        ? Localizer.Get("Flyouts_KillSwitch_Advanced_Success")
        : Localizer.Get("Flyouts_KillSwitch_Standard_Success");

    public bool IsAdvancedKillSwitchInfoMessageVisible => IsAdvancedKillSwitchEnabled && ConnectionManager.IsDisconnected;

    public bool IsKillSwitchEnabled => Settings.IsKillSwitchEnabled;

    public KillSwitchMode KillSwitchMode => Settings.KillSwitchMode;

    public bool IsInfoMessageVisible => !IsKillSwitchEnabled
                                     || (!ConnectionManager.IsConnected && KillSwitchMode == KillSwitchMode.Standard);

    public bool IsWarningMessageVisible => IsKillSwitchEnabled
                                        && KillSwitchMode == KillSwitchMode.Advanced;

    public bool IsSuccessMessageVisible => IsKillSwitchEnabled && ConnectionManager.IsConnected;

    public override bool IsRestricted => false;

    public bool IsStandardKillSwitchEnabled => IsKillSwitchEnabled && KillSwitchMode == KillSwitchMode.Standard;

    public bool IsAdvancedKillSwitchEnabled => IsKillSwitchEnabled && KillSwitchMode == KillSwitchMode.Advanced;

    protected override UpsellFeatureType? UpsellFeature { get; } = null;

    public KillSwitchWidgetViewModel(
        IViewModelHelper viewModelHelper,
        ISettings settings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        ISettingsConflictResolver settingsConflictResolver,
        IProfileEditor profileEditor)
        : base(viewModelHelper,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               connectionManager,
               upsellCarouselWindowActivator,
               requiredReconnectionSettings,
               settingsConflictResolver, 
               profileEditor,
               ConnectionFeature.KillSwitch)
    {
        _disableKillSwitchSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.IsKillSwitchEnabled, () => false)
        ]);

        _enableStandardKillSwitchSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.KillSwitchMode, () => KillSwitchMode.Standard),
            ChangedSettingArgs.Create(() => Settings.IsKillSwitchEnabled, () => true)
        ]);

        _enableAdvancedKillSwitchSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.KillSwitchMode, () => KillSwitchMode.Advanced),
            ChangedSettingArgs.Create(() => Settings.IsKillSwitchEnabled, () => true)
        ]);
    }

    protected override IEnumerable<string> GetSettingsChangedForUpdate()
    {
        yield return nameof(ISettings.IsKillSwitchEnabled);
        yield return nameof(ISettings.KillSwitchMode);
    }

    protected override string GetFeatureStatus()
    {
        return Localizer.Get(
            IsKillSwitchEnabled
                ? KillSwitchMode switch
                {
                    KillSwitchMode.Standard => "Settings_Connection_KillSwitch_Standard",
                    KillSwitchMode.Advanced => "Settings_Connection_KillSwitch_Advanced",
                    _ => throw new ArgumentOutOfRangeException(nameof(ISettings.KillSwitchMode))
                }
                : "Common_States_Off");
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(WarningMessage));
        OnPropertyChanged(nameof(SuccessMessage));
    }

    protected override void OnSettingsChanged()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(SuccessMessage));
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsWarningMessageVisible));
        OnPropertyChanged(nameof(IsSuccessMessageVisible));
        OnPropertyChanged(nameof(IsKillSwitchEnabled));
        OnPropertyChanged(nameof(KillSwitchMode));
        OnPropertyChanged(nameof(IsStandardKillSwitchEnabled));
        OnPropertyChanged(nameof(IsAdvancedKillSwitchEnabled));
        OnPropertyChanged(nameof(IsAdvancedKillSwitchInfoMessageVisible));
    }

    protected override void OnConnectionStatusChanged()
    {
        OnPropertyChanged(nameof(WarningMessage));
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsSuccessMessageVisible));
        OnPropertyChanged(nameof(IsAdvancedKillSwitchInfoMessageVisible));
    }

    protected override bool IsOnFeaturePage(PageViewModelBase? currentPageContext)
    {
        return currentPageContext is KillSwitchPageViewModel;
    }

    [RelayCommand]
    private Task<bool> DisableKillSwitchAsync()
    {
        return TryChangeFeatureSettingsAsync(_disableKillSwitchSettings.Value);
    }

    [RelayCommand]
    private Task<bool> EnableStandardKillSwitchAsync()
    {
        return TryChangeFeatureSettingsAsync(_enableStandardKillSwitchSettings.Value);
    }

    [RelayCommand]
    private Task<bool> EnableAdvancedKillSwitchAsync()
    {
        return TryChangeFeatureSettingsAsync(_enableAdvancedKillSwitchSettings.Value);
    }
}