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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Mapping;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Navigation.Bases;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Countries;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Gateways;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Profiles;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Recents;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;

namespace ProtonVPN.Client.Services.Navigation;

public class ConnectionsViewNavigator : ViewNavigatorBase, IConnectionsViewNavigator,
    IEventMessageReceiver<LoggedInMessage>
{
    private readonly IRecentConnectionsManager _recentConnectionsManager;
    private readonly IServersLoader _serversLoader;

    public override FrameLoadedBehavior LoadBehavior { get; protected set; } = FrameLoadedBehavior.DoNothing;

    public ConnectionsViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IRecentConnectionsManager recentConnectionsManager,
        IServersLoader serversLoader)
        : base(logger, pageViewMapper)
    {
        _recentConnectionsManager = recentConnectionsManager;
        _serversLoader = serversLoader;
    }

    public Task<bool> NavigateToCountriesViewAsync()
    {
        return NavigateToAsync<CountriesPageViewModel>();
    }

    public Task<bool> NavigateToCountriesViewAsync(CountriesConnectionType connectionType)
    {
        return NavigateToAsync<CountriesPageViewModel>(connectionType);
    }

    public Task<bool> NavigateToGatewaysViewAsync()
    {
        if (_serversLoader.HasAnyGateways())
        {
            return NavigateToAsync<GatewaysPageViewModel>();
        }
        return Task.FromResult(false);
    }

    public Task<bool> NavigateToProfilesViewAsync()
    {
        return NavigateToAsync<ProfilesPageViewModel>();
    }

    public Task<bool> NavigateToRecentsViewAsync()
    {
        return NavigateToAsync<RecentsPageViewModel>();
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return _recentConnectionsManager.GetRecentConnections().Any()
                ? NavigateToRecentsViewAsync()
                : _serversLoader.GetGateways().Any()
                    ? NavigateToGatewaysViewAsync()
                    : NavigateToCountriesViewAsync(CountriesConnectionType.All);
    }

    public void Receive(LoggedInMessage message)
    {
        NavigateToDefaultAsync();
    }
}