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
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.UI.Settings.Pages.Entities;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Features.KillSwitch;

public partial class KillSwitchViewModel : SettingsPageViewModelBase
{
    private readonly IUrls _urls;
    private readonly IVpnServiceSettingsUpdater _vpnServiceSettingsUpdater;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(KillSwitchFeatureIconSource))]
    private bool _isKillSwitchEnabled;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.KillSwitchMode))]
    [NotifyPropertyChangedFor(nameof(IsStandardKillSwitch))]
    [NotifyPropertyChangedFor(nameof(IsAdvancedKillSwitch))]
    [NotifyPropertyChangedFor(nameof(KillSwitchFeatureIconSource))]
    private KillSwitchMode _currentKillSwitchMode;

    public override bool IsBackEnabled => false;

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
        IOverlayActivator overlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IUrls urls,
        IConnectionManager connectionManager,
        IVpnServiceSettingsUpdater vpnServiceSettingsUpdater,
        ILogger logger,
        IIssueReporter issueReporter)
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
        _vpnServiceSettingsUpdater = vpnServiceSettingsUpdater;
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

    protected override void SaveSettings()
    {
        Settings.IsKillSwitchEnabled = IsKillSwitchEnabled;
        Settings.KillSwitchMode = CurrentKillSwitchMode;

        _vpnServiceSettingsUpdater.SendAsync();
    }

    protected override void RetrieveSettings()
    {
        IsKillSwitchEnabled = Settings.IsKillSwitchEnabled;
        CurrentKillSwitchMode = Settings.KillSwitchMode;
    }

    protected override IEnumerable<ChangedSettingArgs> GetSettings()
    {
        yield return new(nameof(ISettings.IsKillSwitchEnabled), IsKillSwitchEnabled, Settings.IsKillSwitchEnabled != IsKillSwitchEnabled);
        yield return new(nameof(ISettings.KillSwitchMode), CurrentKillSwitchMode, Settings.KillSwitchMode != CurrentKillSwitchMode);
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