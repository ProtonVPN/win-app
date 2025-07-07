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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Users.Handlers;

public class VpnPlanChangedHandler : IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly ILogger _logger;
    private readonly IServersUpdater _serversUpdater;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IConnectionManager _connectionManager;
    private readonly IConnectionCertificateManager _connectionCertificateManager;

    public VpnPlanChangedHandler(ILogger logger,
        IServersUpdater serversUpdater,
        IUserAuthenticator userAuthenticator,
        IConnectionManager connectionManager,
        IConnectionCertificateManager connectionCertificateManager)
    {
        _logger = logger;
        _serversUpdater = serversUpdater;
        _userAuthenticator = userAuthenticator;
        _connectionManager = connectionManager;
        _connectionCertificateManager = connectionCertificateManager;
    }

    public async void Receive(VpnPlanChangedMessage message)
    {
        if (_userAuthenticator.IsLoggedIn)
        {
            await HandleVpnPlanChangeAsync(message.IsDowngrade());
        }
    }

    private async Task HandleVpnPlanChangeAsync(bool isDowngrade)
    {
        _logger.Info<AppLog>("Requesting new certificate after VPN plan change.");
        await _connectionCertificateManager.ForceRequestNewCertificateAsync();

        _logger.Info<AppLog>("Reprocessing current servers and fetching new servers after VPN plan change.");
        await _serversUpdater.ForceUpdateAsync();

        if (isDowngrade)
        {
            _logger.Info<AppLog>("Reconnecting due to VPN plan downgrade.");
            await _connectionManager.ReconnectIfNotRecentlyReconnectedAsync();
        }
    }
}