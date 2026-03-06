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

using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Client;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Cache;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts;

namespace ProtonVPN.Client.UI.Main;

public partial class NoServersPageViewModel : PageViewModelBase<IMainWindowViewNavigator, IMainViewNavigator>
{
    private const int MIN_LOAD_TIME_IN_MS = 2000;
    private const string REFRESH_ACTION_CODE = "Refresh";
    private const string SIGNOUT_ACTION_CODE = "SignOut";

    private readonly IServersUpdater _serversUpdater;
    private readonly IServersCache _serversCache;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IReportIssueWindowActivator _reportIssueWindowActivator;
    private readonly IVpnPlanUpdater _vpnPlanUpdater;
    private readonly IMainWindowActivator _mainWindowActivator;

    private BaseResponseDetail? AuthResponseDetails => _vpnPlanUpdater.AuthResponseDetails;

    public override string Title => Localizer.Get(string.IsNullOrEmpty(AuthResponseDetails?.Title)
        ? "NoServers_Title"
        : AuthResponseDetails.Title);

    public string Description => Localizer.Get(HasServersRequestFailed
        ? "NoServers_FailedToLoad"
        : string.IsNullOrEmpty(AuthResponseDetails?.Body)
            ? "NoServers_Tip"
            : AuthResponseDetails.Body);

    public bool HasServersRequestFailed => _serversCache.HasServersRequestFailed();

    public bool IsRefreshButtonVisible => GetIsActionVisible(REFRESH_ACTION_CODE);

    public bool IsSignOutButtonVisible => GetIsActionVisible(SIGNOUT_ACTION_CODE);

    public bool IsHelpSectionVisible => AuthResponseDetails is null && HasServersRequestFailed;

    public bool IsMarkdownHintVisible => !string.IsNullOrWhiteSpace(HintMarkdown);

    public string HintMarkdown => AuthResponseDetails?.HintWithMarkdown ?? string.Empty;

    public MarkdownConfig MarkdownConfig { get; } = MarkdownConfig.Default;

    [ObservableProperty]
    private bool _isRefreshing;

    public NoServersPageViewModel(
        IServersUpdater serversUpdater,
        IServersCache serversCache,
        IUserAuthenticator userAuthenticator,
        IReportIssueWindowActivator reportIssueWindowActivator,
        IVpnPlanUpdater vpnPlanUpdater,
        IMainWindowActivator mainWindowActivator,
        IMainWindowViewNavigator parentViewNavigator,
        IMainViewNavigator childViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, childViewNavigator, viewModelHelper)
    {
        _serversUpdater = serversUpdater;
        _serversCache = serversCache;
        _userAuthenticator = userAuthenticator;
        _reportIssueWindowActivator = reportIssueWindowActivator;
        _vpnPlanUpdater = vpnPlanUpdater;
        _mainWindowActivator = mainWindowActivator;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        DisableMainWindowResizeCapabilities();
    }

    protected override void OnDeactivated()
    {
        IsRefreshing = false;
        RestoreMainWindowResizeCapabilities();

        base.OnDeactivated();
    }

    private bool GetIsActionVisible(string actionCode)
    {
        if (AuthResponseDetails?.Actions is null)
        {
            return true;
        }

        return AuthResponseDetails.Actions.Any(a =>
            string.Equals(a.Code, actionCode, StringComparison.OrdinalIgnoreCase));
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        try
        {
            IsRefreshing = true;

            // Add some fake waiting time in case the response is fast,
            // so we prevent the user from refreshing too often
            await Task.WhenAll(
                AuthResponseDetails is null
                    ? _serversUpdater.ForceUpdateAsync()
                    : AutoLoginWithoutWindowRepositionAsync(),
                Task.Delay(MIN_LOAD_TIME_IN_MS));
        }
        finally
        {
            bool isNavigatingAwayFromNoServers = _userAuthenticator.IsLoggedIn && !_serversCache.HasNoServers();
            if (!isNavigatingAwayFromNoServers)
            {
                IsRefreshing = false;
            }
        }
    }

    [RelayCommand]
    private Task SignOutAsync()
    {
        return _userAuthenticator.LogoutAsync(LogoutReason.UserAction);
    }

    [RelayCommand]
    private void ContactUs()
    {
        _reportIssueWindowActivator.Activate();
    }

    private bool CanRefresh()
    {
        return !IsRefreshing;
    }

    private async Task AutoLoginWithoutWindowRepositionAsync()
    {
        _mainWindowActivator.SetWindowMovable(isMovable: false);
        try
        {
            await _userAuthenticator.AutoLoginUserAsync(isAppStartup: false);
        }
        finally
        {
            _mainWindowActivator.SetWindowMovable(isMovable: true);
        }
    }

    private void DisableMainWindowResizeCapabilities()
    {
        if (_mainWindowActivator.Window is MainWindow mainWindow)
        {
            mainWindow.InvalidateWindowResizeCapabilities(canResize: false);
        }
    }

    private void RestoreMainWindowResizeCapabilities()
    {
        if (_mainWindowActivator.Window is MainWindow mainWindow)
        {
            mainWindow.InvalidateTitleBarVisibility(isTitleBarVisible: _userAuthenticator.IsLoggedIn);
            mainWindow.InvalidateWindowResizeCapabilities(canResize: _userAuthenticator.IsLoggedIn);
        }
    }
}