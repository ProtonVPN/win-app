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

using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Services.Mapping;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Contracts.Services.Navigation.Bases;
using ProtonVPN.Client.UI.Main.Sidebar.Connections;
using ProtonVPN.Client.UI.Main.Sidebar.Search;

namespace ProtonVPN.Client.Services.Navigation;

public class SidebarViewNavigator : ViewNavigatorBase, ISidebarViewNavigator
{
    public SidebarViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper)
        : base(logger, pageViewMapper)
    { }

    public Task<bool> NavigateToConnectionsViewAsync()
    {
        return NavigateToAsync<ConnectionsPageViewModel>();
    }

    public Task<bool> NavigateToSearchViewAsync()
    {
        return NavigateToAsync<SearchResultsPageViewModel>();
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return NavigateToConnectionsViewAsync();
    }
}