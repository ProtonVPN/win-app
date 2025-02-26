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
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;

namespace ProtonVPN.Client.UI.Main.Settings.Pages;

public partial class CensorshipSettingsPageViewModel : SettingsPageViewModelBase
{
    private readonly IUrlsBrowser _urlsBrowser;

    [ObservableProperty]
    private bool _isShareStatisticsEnabled;

    [ObservableProperty]
    private bool _isShareCrashReportsEnabled;

    public override string Title => Localizer.Get("Settings_Improve_Censorship");

    public string LearnMoreUrl => _urlsBrowser.UsageStatisticsLearnMore;

    public CensorshipSettingsPageViewModel(
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
            ChangedSettingArgs.Create(() => Settings.IsShareStatisticsEnabled, () => IsShareStatisticsEnabled),
            ChangedSettingArgs.Create(() => Settings.IsShareCrashReportsEnabled, () => IsShareCrashReportsEnabled),
        ];
    }

    protected override void OnRetrieveSettings()
    {
        IsShareStatisticsEnabled = Settings.IsShareStatisticsEnabled;
        IsShareCrashReportsEnabled = Settings.IsShareCrashReportsEnabled;
    }
}