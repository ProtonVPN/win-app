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
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;

namespace ProtonVPN.Client.UI.Main.Settings.Connection;

public partial class KillSwitchPageViewModel : SettingsPageViewModelBase
{
    private readonly IUrlsBrowser _urlsBrowser;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(KillSwitchFeatureIconSource))]
    private bool _isKillSwitchEnabled;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.KillSwitchMode))]
    [NotifyPropertyChangedFor(nameof(IsStandardKillSwitch))]
    [NotifyPropertyChangedFor(nameof(IsAdvancedKillSwitch))]
    [NotifyPropertyChangedFor(nameof(KillSwitchFeatureIconSource))]
    private KillSwitchMode _currentKillSwitchMode;

    public override string Title => Localizer.Get("Settings_Connection_KillSwitch");
    public ImageSource KillSwitchFeatureIconSource => GetFeatureIconSource(IsKillSwitchEnabled, CurrentKillSwitchMode);

    public string LearnMoreUrl => _urlsBrowser.KillSwitchLearnMore;

    public bool IsStandardKillSwitch
    {
        get => IsKillSwitchMode(KillSwitchMode.Standard);
        set => SetKillSwitchMode(value, KillSwitchMode.Standard);
    }

    public bool IsAdvancedKillSwitch
    {
        get => IsKillSwitchMode(KillSwitchMode.Advanced);
        set => SetKillSwitchMode(value, KillSwitchMode.Advanced);
    }

    public KillSwitchPageViewModel(
        IUrlsBrowser urlsBrowser,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
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

        PageSettings =
        [
            ChangedSettingArgs.Create(() => Settings.KillSwitchMode, () => CurrentKillSwitchMode),
            ChangedSettingArgs.Create(() => Settings.IsKillSwitchEnabled, () => IsKillSwitchEnabled)
        ];
    }

    public static ImageSource GetFeatureIconSource(bool isEnabled, KillSwitchMode mode)
    {
        if (!isEnabled)
        {
            return ResourceHelper.GetIllustration("KillSwitchOffIllustrationSource");
        }

        return mode switch
        {
            KillSwitchMode.Standard => ResourceHelper.GetIllustration("KillSwitchStandardIllustrationSource"),
            KillSwitchMode.Advanced => ResourceHelper.GetIllustration("KillSwitchAdvancedIllustrationSource"),
            _ => throw new ArgumentOutOfRangeException(nameof(mode)),
        };
    }

    protected override void OnRetrieveSettings()
    {
        IsKillSwitchEnabled = Settings.IsKillSwitchEnabled;
        CurrentKillSwitchMode = Settings.KillSwitchMode;
    }

    private bool IsKillSwitchMode(KillSwitchMode killSwitchMode)
    {
        return CurrentKillSwitchMode == killSwitchMode;
    }

    private void SetKillSwitchMode(bool value, KillSwitchMode killSwitchMode)
    {
        if (value)
        {
            CurrentKillSwitchMode = killSwitchMode;
        }
    }
}