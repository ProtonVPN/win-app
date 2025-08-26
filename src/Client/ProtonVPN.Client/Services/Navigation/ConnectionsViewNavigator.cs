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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Mapping;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Navigation.Bases;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.Contracts;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Countries;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Gateways;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Profiles;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Recents;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Services.Navigation;

public class ConnectionsViewNavigator : ViewNavigatorBase, IConnectionsViewNavigator,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<ServerListChangedMessage>,
    IEventMessageReceiver<RecentConnectionsChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>
{
    private readonly IRecentConnectionsManager _recentConnectionsManager;
    private readonly IServersLoader _serversLoader;
    private readonly ISettings _settings;

    public override FrameLoadedBehavior LoadBehavior { get; protected set; } = FrameLoadedBehavior.NavigateToDefaultViewIfEmpty;

    public ConnectionsViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher,
        IRecentConnectionsManager recentConnectionsManager,
        IServersLoader serversLoader,
        ISettings settings)
        : base(logger, pageViewMapper, uiThreadDispatcher)
    {
        _recentConnectionsManager = recentConnectionsManager;
        _serversLoader = serversLoader;
        _settings = settings;
    }

    public bool CanNavigateToCountriesView()
    {
        return _serversLoader.HasAnyCountries();
    }

    public async Task<bool> NavigateToCountriesViewAsync()
    {
        return CanNavigateToCountriesView()
            && await NavigateToAsync<CountriesPageViewModel>();
    }

    public bool CanNavigateToGatewaysView()
    {
        return _serversLoader.HasAnyGateways();
    }

    public async Task<bool> NavigateToGatewaysViewAsync()
    {
        return CanNavigateToGatewaysView()
            && await NavigateToAsync<GatewaysPageViewModel>();
    }

    public bool CanNavigateToProfilesView()
    {
        return true;
    }

    public async Task<bool> NavigateToProfilesViewAsync()
    {
        return CanNavigateToProfilesView()
            && await NavigateToAsync<ProfilesPageViewModel>();
    }

    public bool CanNavigateToRecentsView()
    {
        return _settings.VpnPlan.IsPaid
            || _recentConnectionsManager.HasAnyRecentConnections();
    }

    public async Task<bool> NavigateToRecentsViewAsync()
    {
        return CanNavigateToRecentsView()
            && await NavigateToAsync<RecentsPageViewModel>();
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return _recentConnectionsManager.HasAnyRecentConnections()
                ? NavigateToRecentsViewAsync()
                : _serversLoader.HasAnyGateways()
                    ? NavigateToGatewaysViewAsync()
                    : NavigateToCountriesViewAsync();
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        UIThreadDispatcher.TryEnqueue(async () => await InvalidateCurrentPageAsync());
    }

    public void Receive(ServerListChangedMessage message)
    {
        UIThreadDispatcher.TryEnqueue(async () => await InvalidateCurrentPageAsync());
    }

    public void Receive(RecentConnectionsChangedMessage message)
    {
        UIThreadDispatcher.TryEnqueue(async () => await InvalidateCurrentPageAsync());
    }

    public void Receive(LoggedInMessage message)
    {
        UIThreadDispatcher.TryEnqueue(async () => await NavigateToDefaultAsync());
    }

    private async Task InvalidateCurrentPageAsync()
    {
        if (GetCurrentPageContext() is not IConnectionPage connectionPage || !connectionPage.IsAvailable)
        {
            await NavigateToDefaultAsync();
        }
    }
}