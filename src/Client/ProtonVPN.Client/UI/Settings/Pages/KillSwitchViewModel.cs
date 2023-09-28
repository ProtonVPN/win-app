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
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;

namespace ProtonVPN.Client.UI.Settings.Pages;

public partial class KillSwitchViewModel : SettingsPageViewModelBase
{
    private readonly IUrls _urls;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(KillSwitchFeatureIconSource))]
    private bool _isKillSwitchEnabled;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.KillSwitchMode))]
    [NotifyPropertyChangedFor(nameof(IsStandardKillSwitch))]
    [NotifyPropertyChangedFor(nameof(IsAdvancedKillSwitch))]
    [NotifyPropertyChangedFor(nameof(KillSwitchFeatureIconSource))]
    private KillSwitchMode _currentKillSwitchMode;

    public override string? Title => Localizer.Get("Settings_Features_KillSwitch");

    public ImageSource KillSwitchFeatureIconSource => GetFeatureIconSource(IsKillSwitchEnabled, CurrentKillSwitchMode);

    public string LearnMoreUrl => _urls.KillSwitchLearnMore;

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

    public KillSwitchViewModel(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IUrls urls)
        : base(viewNavigator, localizationProvider, settings, settingsConflictResolver)
    {
        _urls = urls;
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

    protected override bool HasConfigurationChanged()
    {
        return Settings.IsKillSwitchEnabled != IsKillSwitchEnabled
            || Settings.KillSwitchMode != CurrentKillSwitchMode;
    }

    protected override void SaveSettings()
    {
        Settings.IsKillSwitchEnabled = IsKillSwitchEnabled;
        Settings.KillSwitchMode = CurrentKillSwitchMode;
    }

    protected override void RetrieveSettings()
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