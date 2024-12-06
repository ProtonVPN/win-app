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
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Core.Services.Mapping;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Navigation.Bases;
using ProtonVPN.Client.UI.Main.Home.Details.Connection;
using ProtonVPN.Client.UI.Main.Home.Details.Location;
using ProtonVPN.Client.Common.Dispatching;

namespace ProtonVPN.Client.Services.Navigation;

public class DetailsViewNavigator : ViewNavigatorBase, IDetailsViewNavigator,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private readonly IConnectionManager _connectionManager;

    public DetailsViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher,
        IConnectionManager connectionManager)
        : base(logger, pageViewMapper, uiThreadDispatcher)
    {
        _connectionManager = connectionManager;
    }

    public Task<bool> NavigateToLocationDetailsViewAsync()
    {
        return NavigateToAsync<LocationDetailsPageViewModel>();
    }

    public Task<bool> NavigateToConnectionDetailsViewAsync()
    {
        return NavigateToAsync<ConnectionDetailsPageViewModel>();
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return _connectionManager.IsConnected
            ? NavigateToConnectionDetailsViewAsync()
            : NavigateToLocationDetailsViewAsync();
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        UIThreadDispatcher.TryEnqueue(async () => await NavigateToDefaultAsync());
    }
}