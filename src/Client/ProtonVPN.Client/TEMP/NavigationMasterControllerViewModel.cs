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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Notification;
using ProtonVPN.Client.Contracts.Services.Activation;

namespace ProtonVPN.Client.TEMP;

public partial class NavigationMasterControllerViewModel : ViewModelBase
{
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IMainWindowViewNavigator _mainWindowViewNavigator;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly IDetailsViewNavigator _detailsViewNavigator;
    private readonly ILoginViewNavigator _loginViewNavigator;
    private readonly ISidebarViewNavigator _sidebarViewNavigator;
    private readonly IConnectionsViewNavigator _connectionsViewNavigator;
    private readonly ISettingsViewNavigator _settingsViewNavigator;
    private readonly IReportIssueWindowActivator _reportIssueWindowActivator;
    private readonly IReportIssueViewNavigator _reportIssueViewNavigator;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IAppNotificationSender _appNotificationSender;

    public NavigationMasterControllerViewModel(
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IMainWindowActivator mainWindowActivator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IMainWindowViewNavigator mainWindowViewNavigator,
        IMainViewNavigator mainViewNavigator,
        IDetailsViewNavigator detailsViewNavigator,
        ILoginViewNavigator loginViewNavigator,
        ISidebarViewNavigator sidebarViewNavigator,
        IConnectionsViewNavigator connectionsViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IReportIssueWindowActivator reportIssueWindowActivator,
        IReportIssueViewNavigator reportIssueViewNavigator,
        IUserAuthenticator userAuthenticator,
        IAppNotificationSender appNotificationSender)
        : base(localizer, logger, issueReporter)
    {
        _mainWindowActivator = mainWindowActivator;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _mainWindowViewNavigator = mainWindowViewNavigator;
        _mainViewNavigator = mainViewNavigator;
        _detailsViewNavigator = detailsViewNavigator;
        _loginViewNavigator = loginViewNavigator;
        _sidebarViewNavigator = sidebarViewNavigator;
        _connectionsViewNavigator = connectionsViewNavigator;
        _settingsViewNavigator = settingsViewNavigator;
        _reportIssueWindowActivator = reportIssueWindowActivator;
        _reportIssueViewNavigator = reportIssueViewNavigator;
        _userAuthenticator = userAuthenticator;
        _appNotificationSender = appNotificationSender;
    }

    [RelayCommand]
    private async Task NavigateToLoginViewAsync()
    {
        await _mainWindowViewNavigator.NavigateToLoginViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToMainViewAsync()
    {
        await _mainWindowViewNavigator.NavigateToMainViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToSignInViewAsync()
    {
        await _loginViewNavigator.NavigateToSignInViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToTwoFactorViewAsync()
    {
        await _loginViewNavigator.NavigateToTwoFactorViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToLoadingViewAsync()
    {
        await _loginViewNavigator.NavigateToLoadingViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToHomeViewAsync()
    {
        await _mainViewNavigator.NavigateToHomeViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToSettingsViewAsync()
    {
        await _mainViewNavigator.NavigateToSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToNetShieldFeatureViewAsync()
    {
        await _mainViewNavigator.NavigateToSettingsViewAsync();
        await _settingsViewNavigator.NavigateToNetShieldSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToKillSwitchFeatureViewAsync()
    {
        await _mainViewNavigator.NavigateToSettingsViewAsync();
        await _settingsViewNavigator.NavigateToKillSwitchSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToPortForwardingFeatureViewAsync()
    {
        await _mainViewNavigator.NavigateToSettingsViewAsync();
        await _settingsViewNavigator.NavigateToPortForwardingSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToSplitTunnelingFeatureViewAsync()
    {
        await _mainViewNavigator.NavigateToSettingsViewAsync();
        await _settingsViewNavigator.NavigateToSplitTunnelingSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToLocationDetailsViewAsync()
    {
        await _detailsViewNavigator.NavigateToLocationDetailsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToConnectionDetailsViewAsync()
    {
        await _detailsViewNavigator.NavigateToConnectionDetailsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToSearchViewAsync()
    {
        await _sidebarViewNavigator.NavigateToSearchViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToConnectionsViewAsync()
    {
        await _sidebarViewNavigator.NavigateToConnectionsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToRecentsViewAsync()
    {
        await _connectionsViewNavigator.NavigateToRecentsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToGatewaysViewAsync()
    {
        await _connectionsViewNavigator.NavigateToGatewaysViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToProfilesViewAsync()
    {
        await _connectionsViewNavigator.NavigateToProfilesViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToCountriesViewAsync()
    {
        await _connectionsViewNavigator.NavigateToCountriesViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToCommonSettingsViewAsync()
    {
        await _settingsViewNavigator.NavigateToCommonSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToAdvancedSettingsViewAsync()
    {
        await _settingsViewNavigator.NavigateToAdvancedSettingsViewAsync();
    }

    [RelayCommand]
    private Task TriggerLogoutAsync()
    {
        return _userAuthenticator.LogoutAsync(LogoutReason.UserAction);
    }

    [RelayCommand]
    private void ShowReportIssue()
    {
        _reportIssueWindowActivator.Activate();
    }

    [RelayCommand]
    private async Task ShowMessageOverlayAsync()
    {
        await _mainWindowOverlayActivator.ShowMessageAsync(new()
        {
            Title = "Message Title",
            Message = "Message Description",
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
        });
    }

    [RelayCommand]
    private async Task ShowLoadingMessageOverlayAsync()
    {
        await _mainWindowOverlayActivator.ShowLoadingMessageAsync(new()
        {
            Title = "Loading Message Title",
            Message = "Loading Message Description",
            ShowLoadingAnimation = true
        }, Task.Delay(2000));
    }

    [RelayCommand]
    private async Task ShowSecureCoreOverlayAsync()
    {
        await _mainWindowOverlayActivator.ShowSecureCoreInfoOverlayAsync();
    }

    [RelayCommand]
    private void ExitApplication()
    {
        _mainWindowActivator.Exit();
    }

    [RelayCommand]
    private void SendAppNotification()
    {
        _appNotificationSender.Send("App notification title", "App notification message");
    }
}