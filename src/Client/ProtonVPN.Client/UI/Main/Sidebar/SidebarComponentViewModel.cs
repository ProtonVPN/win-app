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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Cache;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.UI.Main.Sidebar.Connections;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.Contracts;
using ProtonVPN.Client.UI.Main.Sidebar.Search.Contracts;

namespace ProtonVPN.Client.UI.Main.Sidebar;

public partial class SidebarComponentViewModel : HostViewModelBase<ISidebarViewNavigator>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly IServersCache _serversCache;
    private readonly ISearchInputReceiver _searchInputReceiver;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isSearchVisible = true;

    public ObservableCollection<IConnectionPage> ConnectionPages { get; }

    public SidebarComponentViewModel(
        IServersCache serversCache,
        ISidebarViewNavigator childViewNavigator,
        ILocalizationProvider localizer,
        ISearchInputReceiver searchInputReceiver,
        IEnumerable<IConnectionPage> connectionPages,
        IViewModelHelper viewModelHelper)
        : base(childViewNavigator, viewModelHelper)
    {
        _serversCache = serversCache;
        _searchInputReceiver = searchInputReceiver;

        ConnectionPages = new(connectionPages.OrderBy(p => p.SortIndex));
    }

    protected override void OnChildNavigation(NavigationEventArgs e)
    {
        base.OnChildNavigation(e);

        if (ChildViewNavigator.GetCurrentPageContext() is ConnectionsPageViewModel)
        {
            ClearSearch();
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        _searchInputReceiver.SearchAsync(value).Wait();
    }

    public void ClearSearch()
    {
        SearchText = string.Empty;
    }

    public void OnSearchTextBoxGotFocus(object sender, RoutedEventArgs _)
    {
        if (sender is TextBox)
        {
            ChildViewNavigator.NavigateToSearchViewAsync();
        }
    }

    public void OnSearchTextBoxLostFocus(object sender, RoutedEventArgs _)
    {
        if (sender is TextBox && string.IsNullOrWhiteSpace(SearchText))
        {
            ChildViewNavigator.NavigateToConnectionsViewAsync();
        }
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            ClearSearch();
            InvalidateSearchVisibility();
        });
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateSearchVisibility);
    }

    private void InvalidateSearchVisibility()
    {
        bool hasAnyCountries = _serversCache.Countries.Any();
        IsSearchVisible = hasAnyCountries;

        if (!hasAnyCountries)
        {
            ChildViewNavigator.NavigateToConnectionsViewAsync();
        }
    }

    [RelayCommand]
    private Task LeaveSearchModeAsync()
    {
        ClearSearch();
        return ChildViewNavigator.NavigateToConnectionsViewAsync();
    }
}