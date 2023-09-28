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
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Wrappers;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection;

public class ConnectionManager : IConnectionManager, IEventMessageReceiver<VpnStateIpcEntity>
{
    private readonly IServiceCaller _serviceCaller;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IEntityMapper _entityMapper;
    private readonly IConnectionRequestWrapper _connectionRequestWrapper;
    private readonly IDisconnectionRequestWrapper _disconnectionRequestWrapper;

    private ConnectionDetails? _connectionDetails;

    public ConnectionStatus ConnectionStatus { get; private set; }

    public bool IsDisconnected => ConnectionStatus == ConnectionStatus.Disconnected;

    public bool IsConnecting => ConnectionStatus == ConnectionStatus.Connecting;

    public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;

    public ConnectionManager(
        IServiceCaller serviceCaller,
        IEventMessageSender eventMessageSender,
        IEntityMapper entityMapper,
        IConnectionRequestWrapper connectionRequestWrapper,
        IDisconnectionRequestWrapper disconnectionRequestWrapper)
    {
        _serviceCaller = serviceCaller;
        _eventMessageSender = eventMessageSender;
        _entityMapper = entityMapper;
        _connectionRequestWrapper = connectionRequestWrapper;
        _disconnectionRequestWrapper = disconnectionRequestWrapper;
    }

    public async Task ConnectAsync(IConnectionIntent? connectionIntent)
    {
        connectionIntent ??= ConnectionIntent.Default;

        _connectionDetails = new ConnectionDetails(connectionIntent);
        SetConnectionStatus(ConnectionStatus.Connecting);

        ConnectionRequestIpcEntity request = _connectionRequestWrapper.Wrap(connectionIntent);

        await _serviceCaller.ConnectAsync(request);
    }
    public async Task ReconnectAsync()
    {
        await ConnectAsync(_connectionDetails?.OriginalConnectionIntent);
    }

    public async Task DisconnectAsync()
    {
        _connectionDetails = null;

        DisconnectionRequestIpcEntity request = _disconnectionRequestWrapper.Wrap();

        await _serviceCaller.DisconnectAsync(request);
    }

    public ConnectionDetails? GetConnectionDetails()
    {
        return _connectionDetails;
    }

    public void Receive(VpnStateIpcEntity message)
    {
        ConnectionStatus connectionStatus = _entityMapper.Map<VpnStatusIpcEntity, ConnectionStatus>(message.Status);

        SetConnectionStatus(connectionStatus);
    }

    private void SetConnectionStatus(ConnectionStatus connectionStatus)
    {
        if (ConnectionStatus == connectionStatus)
        {
            return;
        }

        ConnectionStatus = connectionStatus;
        _eventMessageSender.Send(new ConnectionStatusChanged(ConnectionStatus));
    }
}