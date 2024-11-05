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
using ProtonVPN.Client.Contracts.Helpers;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.UI.Settings.Pages.Entities;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Settings.Connection;

public partial class KillSwitchPageViewModel : SettingsPageViewModelBase
{
    public override string Title => Localizer.Get("Settings_Connection_KillSwitch");

    private readonly IUrls _urls;
    private readonly IVpnServiceSettingsUpdater _vpnServiceSettingsUpdater;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(KillSwitchFeatureIconSource))]
    private bool _isKillSwitchEnabled;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.KillSwitchMode))]
    [NotifyPropertyChangedFor(nameof(IsStandardKillSwitch))]
    [NotifyPropertyChangedFor(nameof(IsAdvancedKillSwitch))]
    [NotifyPropertyChangedFor(nameof(KillSwitchFeatureIconSource))]
    private KillSwitchMode _currentKillSwitchMode;

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

    public KillSwitchPageViewModel(
        IUrls urls,
        IVpnServiceSettingsUpdater vpnServiceSettingsUpdater,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager)
        : base(mainViewNavigator, settingsViewNavigator, localizer, logger, issueReporter, mainWindowOverlayActivator, settings, settingsConflictResolver, connectionManager)
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

    protected override async Task OnSaveSettingsAsync()
    {
        Settings.IsKillSwitchEnabled = IsKillSwitchEnabled;
        Settings.KillSwitchMode = CurrentKillSwitchMode;

        await _vpnServiceSettingsUpdater.SendAsync();
    }

    protected override void OnRetrieveSettings()
    {
        IsKillSwitchEnabled = Settings.IsKillSwitchEnabled;
        CurrentKillSwitchMode = Settings.KillSwitchMode;
    }

    protected override IEnumerable<ChangedSettingArgs> GetSettings()
    {
        yield return new(nameof(ISettings.IsKillSwitchEnabled), IsKillSwitchEnabled,
            Settings.IsKillSwitchEnabled != IsKillSwitchEnabled);
        yield return new(nameof(ISettings.KillSwitchMode), CurrentKillSwitchMode,
            Settings.KillSwitchMode != CurrentKillSwitchMode);
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