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

using System.Reflection;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Updaters;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Settings.Pages.Entities;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.DeveloperTools;

public partial class DeveloperToolsPageViewModel : SettingsPageViewModelBase
{
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IServersUpdater _serversUpdater;
    private readonly IUserAuthenticator _userAuthenticator;

    [ObservableProperty]
    private List<Overlay> _overlaysList;

    [ObservableProperty]
    private Overlay? _selectedOverlay;

    public override string Title => "Developer tools";

    public DeveloperToolsPageViewModel(
        IServersUpdater serversUpdater,
        IUserAuthenticator userAuthenticator,
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
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _serversUpdater = serversUpdater;
        _userAuthenticator = userAuthenticator;

        OverlaysList =
        [
            ..typeof(IMainWindowOverlayActivator).GetMethods()
                .Where(m => m.GetParameters().Length == 0)
                .Select(m => new Overlay
                {
                    Id =  m.Name,
                    Name = GenerateOverlayDisplayName(m.Name)
                })
                .ToList()
        ];
    }

    [RelayCommand]
    public void TriggerAppCrash()
    {
        throw new StackOverflowException("Intentional crash test");
    }

    [RelayCommand]
    public async Task TriggerLogicalsRefreshAsync()
    {
        await _serversUpdater.UpdateAsync(ServersRequestParameter.ForceFullUpdate, true);
    }

    [RelayCommand]
    public async Task LogoutUserWithClientOutdatedReasonAsync()
    {
        await _userAuthenticator.LogoutAsync(LogoutReason.ClientOutdated);
    }

    [RelayCommand]
    public void ShowOverlay()
    {
        if (SelectedOverlay != null)
        {
            MethodInfo? methodInfo = _mainWindowOverlayActivator.GetType().GetMethod(SelectedOverlay.Id);
            methodInfo?.Invoke(_mainWindowOverlayActivator, null);
        }
    }

    [RelayCommand]
    public void ResetInfoBanners()
    {
        Settings.IsGatewayInfoBannerDismissed = false;
        Settings.IsP2PInfoBannerDismissed = false;
        Settings.IsSecureCoreInfoBannerDismissed = false;
        Settings.IsTorInfoBannerDismissed = false;
    }

    private string GenerateOverlayDisplayName(string methodName)
    {
        string displayName = Regex.Replace(methodName, "^(Show)", "", RegexOptions.IgnoreCase);
        displayName = Regex.Replace(displayName, "(OverlayAsync)$", "", RegexOptions.IgnoreCase);

        // Insert spaces before uppercase letters, handling acronyms properly (e.g., VPN, B2B)
        displayName = Regex.Replace(displayName, "(?<=[a-z])([A-Z])", " $1");

        return displayName.Trim();
    }

    protected override IEnumerable<ChangedSettingArgs> GetSettings()
    {
        return [];
    }
}