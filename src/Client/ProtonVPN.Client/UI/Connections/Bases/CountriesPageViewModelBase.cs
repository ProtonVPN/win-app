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

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Connections.Common.Enums;
using ProtonVPN.Client.UI.Connections.Common.Factories;
using ProtonVPN.Client.UI.Connections.Common.Items;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Connections.Bases;

public abstract partial class CountriesPageViewModelBase : ConnectionsPageViewModelBase
{
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public override bool IsBackEnabled => false;

    public virtual string Description => string.Empty;

    public virtual SearchDepthLevel SearchDepthLevel => SearchDepthLevel.Two;

    public abstract ImageSource IllustrationSource { get; }

    protected CountriesPageViewModelBase(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        LocationItemFactory locationItemFactory,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        ISettings settings)
        : base(viewNavigator,
               localizationProvider,
               logger,
               issueReporter,
               locationItemFactory,
               serversLoader,
               connectionManager,
               settings)
    { }

    public override void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        base.OnNavigatedTo(parameter, isBackNavigation);

        if (!isBackNavigation)
        {
            SearchQuery = string.Empty;
        }
    }

    protected override IEnumerable<LocationItemBase> FilterItems(IEnumerable<LocationItemBase> items)
    {
        // When searching, includes up to two sub levels of items
        if (SearchDepthLevel > SearchDepthLevel.Zero && !string.IsNullOrWhiteSpace(SearchQuery))
        {
            IEnumerable<LocationItemBase> subItems =
                items.SelectMany(c => c.SubItems);

            items = items.Concat(subItems);

            if (SearchDepthLevel > SearchDepthLevel.One)
            {
                IEnumerable<LocationItemBase> subSubItems =
                    subItems.SelectMany(c => c.SubItems);

                items = items.Concat(subSubItems);
            }
        }

        return items.Where(item => item.MatchesSearchQuery(SearchQuery)).Distinct();
    }

    partial void OnSearchQueryChanged(string value)
    {
        GroupItems();
    }
}