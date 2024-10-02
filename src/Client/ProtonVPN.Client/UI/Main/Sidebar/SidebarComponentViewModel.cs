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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Services.Navigation;
using ProtonVPN.Client.UI.Main.Sidebar.Connections;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.Contracts;

namespace ProtonVPN.Client.UI.Main.Sidebar;

public partial class SidebarComponentViewModel : HostViewModelBase<ISidebarViewNavigator>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSearching))]
    private string _searchText = string.Empty;

    public bool IsSearching => !string.IsNullOrWhiteSpace(SearchText);

    public ObservableCollection<IConnectionPage> ConnectionPages { get; }

    public SidebarComponentViewModel(
        ISidebarViewNavigator childViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IEnumerable<IConnectionPage> connectionPages)
        : base(childViewNavigator, localizer, logger, issueReporter)
    {
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
        if (IsSearching)
        {
            ChildViewNavigator.NavigateToSearchViewAsync();
        }
        else
        {
            ChildViewNavigator.NavigateToConnectionsViewAsync();
        }
    }

    public void ClearSearch()
    {
        SearchText = string.Empty;
    }
}