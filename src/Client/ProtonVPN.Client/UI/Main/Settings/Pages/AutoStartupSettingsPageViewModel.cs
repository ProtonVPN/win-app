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
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;

namespace ProtonVPN.Client.UI.Main.Settings.Pages;

public partial class AutoStartupSettingsPageViewModel : SettingsPageViewModelBase
{
    [ObservableProperty] private bool _isAutoLaunchEnabled;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.AutoLaunchMode))]
    [NotifyPropertyChangedFor(nameof(IsAutoLaunchOpenOnDesktop))]
    [NotifyPropertyChangedFor(nameof(IsAutoLaunchMinimizeToSystemTray))]
    [NotifyPropertyChangedFor(nameof(IsAutoLaunchMinimizeToTaskbar))]
    private AutoLaunchMode _currentAutoLaunchMode;

    [ObservableProperty]
    private bool _isAutoConnectEnabled;

    public bool IsAutoLaunchOpenOnDesktop
    {
        get => IsAutoLaunchMode(AutoLaunchMode.OpenOnDesktop);
        set => SetAutoLaunchMode(value, AutoLaunchMode.OpenOnDesktop);
    }

    public bool IsAutoLaunchMinimizeToSystemTray
    {
        get => IsAutoLaunchMode(AutoLaunchMode.MinimizeToSystemTray);
        set => SetAutoLaunchMode(value, AutoLaunchMode.MinimizeToSystemTray);
    }

    public bool IsAutoLaunchMinimizeToTaskbar
    {
        get => IsAutoLaunchMode(AutoLaunchMode.MinimizeToTaskbar);
        set => SetAutoLaunchMode(value, AutoLaunchMode.MinimizeToTaskbar);
    }

    public override string Title => Localizer.Get("Settings_General_AutoStartup");

    public AutoStartupSettingsPageViewModel(
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
    }

    protected override void OnSaveSettings()
    {
        Settings.IsAutoLaunchEnabled = IsAutoLaunchEnabled;
        Settings.AutoLaunchMode = CurrentAutoLaunchMode;
        Settings.IsAutoConnectEnabled = IsAutoConnectEnabled;
    }

    protected override void OnRetrieveSettings()
    {
        IsAutoLaunchEnabled = Settings.IsAutoLaunchEnabled;
        CurrentAutoLaunchMode = Settings.AutoLaunchMode;
        IsAutoConnectEnabled = Settings.IsAutoConnectEnabled;
    }

    protected override IEnumerable<ChangedSettingArgs> GetSettings()
    {
        yield return new(nameof(ISettings.IsAutoLaunchEnabled), IsAutoLaunchEnabled,
            Settings.IsAutoLaunchEnabled != IsAutoLaunchEnabled);
        yield return new(nameof(ISettings.AutoLaunchMode), CurrentAutoLaunchMode,
            Settings.AutoLaunchMode != CurrentAutoLaunchMode);
        yield return new(nameof(ISettings.IsAutoConnectEnabled), IsAutoConnectEnabled,
            Settings.IsAutoConnectEnabled != IsAutoConnectEnabled);
    }

    private bool IsAutoLaunchMode(AutoLaunchMode autoLaunchMode)
    {
        return CurrentAutoLaunchMode == autoLaunchMode;
    }

    private void SetAutoLaunchMode(bool value, AutoLaunchMode autoLaunchMode)
    {
        if (value)
        {
            CurrentAutoLaunchMode = autoLaunchMode;
        }
    }
}