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
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;

namespace ProtonVPN.Client.Handlers;

public class AutoConnectHandler : IHandler,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly IConnectionManager _connectionManager;
    private readonly IRecentConnectionsProvider _recentConnectionsProvider;
    private readonly ISettings _settings;

    private bool _isUserAutoLoggedIn;
    private bool _isServersListUpdated;
    private bool _isHandled;

    public AutoConnectHandler(
        IConnectionManager connectionManager,
        IRecentConnectionsProvider recentConnectionsProvider,
        ISettings settings)
    {
        _connectionManager = connectionManager;
        _recentConnectionsProvider = recentConnectionsProvider;
        _settings = settings;
    }

    public async void Receive(LoggedInMessage message)
    {
        _isUserAutoLoggedIn = message.IsAutoLogin;

        await TryAutoConnectAsync();
    }

    public async void Receive(ServerListChangedMessage message)
    {
        _isServersListUpdated = true;

        await TryAutoConnectAsync();
    }

    public void Receive(LoggedOutMessage message)
    {
        // Reset flags
        _isUserAutoLoggedIn = false;
        _isServersListUpdated = false;
        _isHandled = false;
    }

    private async Task TryAutoConnectAsync()
    {
        if (_isHandled || !_isUserAutoLoggedIn || !_isServersListUpdated)
        {
            return;
        }

        if (_settings.IsAutoConnectEnabled && _connectionManager.IsDisconnected)
        {
            switch (_settings.AutoConnectMode)
            {
                case AutoConnectMode.LatestConnection:
                    await _connectionManager.ConnectAsync(_recentConnectionsProvider.GetMostRecentConnection()?.ConnectionIntent);
                    break;

                case AutoConnectMode.FastestConnection:
                    await _connectionManager.ConnectAsync(ConnectionIntent.Default);
                    break;
            }
        }

        _isHandled = true;
    }
}