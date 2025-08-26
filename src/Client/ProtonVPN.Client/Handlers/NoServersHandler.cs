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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Cache;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.UI.Main;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.Handlers;

public class NoServersHandler : IHandler,
    IEventMessageReceiver<ServerListChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>
{
    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IServersCache _serversCache;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IMainWindowViewNavigator _mainWindowViewNavigator;
    private readonly IConnectionManager _connectionManager;

    public NoServersHandler(
        ILogger logger,
        ISettings settings,
        IServersCache serversCache,
        IUIThreadDispatcher uiThreadDispatcher,
        IUserAuthenticator userAuthenticator,
        IMainWindowViewNavigator mainWindowViewNavigator,
        IConnectionManager connectionManager)
    {
        _logger = logger;
        _settings = settings;
        _serversCache = serversCache;
        _uiThreadDispatcher = uiThreadDispatcher;
        _userAuthenticator = userAuthenticator;
        _mainWindowViewNavigator = mainWindowViewNavigator;
        _connectionManager = connectionManager;
    }

    public async void Receive(ServerListChangedMessage message)
    {
        if (!_userAuthenticator.IsLoggedIn)
        {
            return;
        }

        await _uiThreadDispatcher.TryEnqueueAsync(async () =>
        {
            if (_serversCache.IsEmpty())
            {
                if (!_connectionManager.IsDisconnected)
                {
                    _logger.Info<ConnectionLog>("Disconnecting from VPN due to no servers available.");

                    await _connectionManager.DisconnectAsync(VpnTriggerDimension.Undefined);
                }

                await _mainWindowViewNavigator.NavigateToNoServersViewAsync();
            }
            else if (_mainWindowViewNavigator.GetCurrentPageContext() is NoServersPageViewModel)
            {
                await _mainWindowViewNavigator.NavigateToMainViewAsync();
            }

            HandleDefaultConnectionSetting();
        });
    }

    private void HandleDefaultConnectionSetting()
    {
        if (_serversCache.Gateways.Any() && !_serversCache.Countries.Any() &&
            _settings.DefaultConnection.Type is DefaultConnectionType.Fastest or DefaultConnectionType.Random)
        {
            _settings.DefaultConnection = DefaultConnection.Last;
        }
    }

    public void Receive(LoggedInMessage message)
    {
        _uiThreadDispatcher.TryEnqueue(HandleDefaultConnectionSetting);
    }
}