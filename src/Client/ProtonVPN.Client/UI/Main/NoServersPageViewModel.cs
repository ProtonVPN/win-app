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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Cache;
using ProtonVPN.Client.Logic.Servers.Contracts;

namespace ProtonVPN.Client.UI.Main;

public partial class NoServersPageViewModel : PageViewModelBase<IMainWindowViewNavigator, IMainViewNavigator>
{
    private const int MIN_LOAD_TIME_IN_MS = 2000;

    private readonly IServersUpdater _serversUpdater;
    private readonly IServersCache _serversCache;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IReportIssueWindowActivator _reportIssueWindowActivator;

    public override string Title => Localizer.Get("NoServers_Title");

    public bool HasServersRequestFailed => _serversCache.HasServersRequestFailed();

    public string Description => Localizer.Get(HasServersRequestFailed
        ? "NoServers_FailedToLoad"
        : "NoServers_Tip");

    public bool IsRefreshButtonVisible => true;

    public bool IsSignOutButtonVisible => true;

    [ObservableProperty]
    private bool _isRefreshing;

    public NoServersPageViewModel(
        IServersUpdater serversUpdater,
        IServersCache serversCache,
        IUserAuthenticator userAuthenticator,
        IReportIssueWindowActivator reportIssueWindowActivator,
        IMainWindowViewNavigator parentViewNavigator,
        IMainViewNavigator childViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, childViewNavigator, viewModelHelper)
    {
        _serversUpdater = serversUpdater;
        _serversCache = serversCache;
        _userAuthenticator = userAuthenticator;
        _reportIssueWindowActivator = reportIssueWindowActivator;
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
                _serversUpdater.ForceUpdateAsync(),
                Task.Delay(MIN_LOAD_TIME_IN_MS));
        }
        finally
        {
            IsRefreshing = false;
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
}