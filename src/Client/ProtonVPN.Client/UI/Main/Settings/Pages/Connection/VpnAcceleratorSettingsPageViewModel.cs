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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.UI.Settings.Pages.Entities;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.Connection;

public partial class VpnAcceleratorSettingsPageViewModel : SettingsPageViewModelBase
{
    private readonly IUrls _urls;

    [ObservableProperty]
    private bool _isVpnAcceleratorEnabled;

    public override string Title => Localizer.Get("Settings_Connection_VpnAccelerator");

    public string LearnMoreUrl => _urls.VpnAcceleratorLearnMore;

    public VpnAcceleratorSettingsPageViewModel(
        IUrls urls,
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
    }

    protected override void OnSaveSettings()
    {
        Settings.IsVpnAcceleratorEnabled = IsVpnAcceleratorEnabled;
    }

    protected override void OnRetrieveSettings()
    {
        IsVpnAcceleratorEnabled = Settings.IsVpnAcceleratorEnabled;
    }

    protected override IEnumerable<ChangedSettingArgs> GetSettings()
    {
        yield return new(nameof(ISettings.IsVpnAcceleratorEnabled), IsVpnAcceleratorEnabled,
            Settings.IsVpnAcceleratorEnabled != IsVpnAcceleratorEnabled);
    }
}