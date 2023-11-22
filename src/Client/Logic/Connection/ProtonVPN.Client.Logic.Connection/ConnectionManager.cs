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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Wrappers;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ConnectionDetails = ProtonVPN.Client.Logic.Connection.Contracts.Models.ConnectionDetails;

namespace ProtonVPN.Client.Logic.Connection;

public class ConnectionManager : IConnectionManager,
    IEventMessageReceiver<VpnStateIpcEntity>,
    IEventMessageReceiver<ConnectionDetailsIpcEntity>
{
    private readonly ILogger _logger;
    private readonly IServiceCaller _serviceCaller;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IEntityMapper _entityMapper;
    private readonly IConnectionRequestWrapper _connectionRequestWrapper;
    private readonly IDisconnectionRequestWrapper _disconnectionRequestWrapper;
    private readonly IServerManager _serverManager;

    private ConnectionDetails? _connectionDetails;
    private TrafficBytes _bytesTransferred = TrafficBytes.Zero;

    public ConnectionStatus ConnectionStatus { get; private set; }

    public bool IsDisconnected => ConnectionStatus == ConnectionStatus.Disconnected;

    public bool IsConnecting => ConnectionStatus == ConnectionStatus.Connecting;

    public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;

    public ConnectionManager(
        ILogger logger,
        IServiceCaller serviceCaller,
        IEventMessageSender eventMessageSender,
        IEntityMapper entityMapper,
        IConnectionRequestWrapper connectionRequestWrapper,
        IDisconnectionRequestWrapper disconnectionRequestWrapper,
        IServerManager serverManger)
    {
        _logger = logger;
        _serviceCaller = serviceCaller;
        _eventMessageSender = eventMessageSender;
        _entityMapper = entityMapper;
        _connectionRequestWrapper = connectionRequestWrapper;
        _disconnectionRequestWrapper = disconnectionRequestWrapper;
        _serverManager = serverManger;
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

    public void Receive(ConnectionDetailsIpcEntity message)
    {
        _eventMessageSender.Send(new ConnectionDetailsChanged(message.ClientIpAddress, message.ServerIpAddress));
    }

    public async Task<TrafficBytes> GetTrafficBytesAsync()
    {
        Result<TrafficBytesIpcEntity> trafficBytes = await _serviceCaller.GetTrafficBytesAsync();
        return trafficBytes.Success
            ? _entityMapper.Map<TrafficBytesIpcEntity, TrafficBytes>(trafficBytes.Value)
            : TrafficBytes.Zero;
    }

    public async Task<TrafficBytes> GetCurrentSpeedAsync()
    {
        TrafficBytes bytesTransferred = await GetTrafficBytesAsync();

        ulong downloadSpeed = Math.Max(0, bytesTransferred.BytesIn - _bytesTransferred.BytesIn);
        ulong uploadSpeed = Math.Max(0, bytesTransferred.BytesOut - _bytesTransferred.BytesOut);

        _bytesTransferred = bytesTransferred;

        return new TrafficBytes(downloadSpeed, uploadSpeed);
    }

    public void Receive(VpnStateIpcEntity message)
    {
        ConnectionStatus connectionStatus = _entityMapper.Map<VpnStatusIpcEntity, ConnectionStatus>(message.Status);

        if (_connectionDetails != null && message.Status == VpnStatusIpcEntity.Connected)
        {
            Server? server = GetCurrentServer(message);
            if (server is null)
            {
                _logger.Error<AppLog>($"The status changed to Connected but the associated Server is null. Error: '{message.Error}' " +
                                      $"NetworkBlocked: '{message.NetworkBlocked}' " +
                                      $"Status: '{message.Status}' EntryIp: '{message.EndpointIp}' Label: '{message.Label}' " +
                                      $"NetworkAdapterType: '{message.OpenVpnAdapterType}' VpnProtocol: '{message.VpnProtocol}'");

                // TODO: call reconnection logic excluding the last server
            }
            else
            {
                VpnProtocol vpnProtocol = _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(message.VpnProtocol);
                _connectionDetails = new ConnectionDetails(_connectionDetails.OriginalConnectionIntent, server, vpnProtocol);
            }
        }

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

    private Server? GetCurrentServer(VpnStateIpcEntity state)
    {
        List<Server> servers = _serverManager.GetServers();

        //TODO: instead of EndpointIp and Label we should have VpnHost (including Id property) so we can easily find server by ID.
        return (from server in servers
            from physicalServer in server.Servers
            where physicalServer.EntryIp == state.EndpointIp && physicalServer.Label == state.Label
            select server).FirstOrDefault();
    }
}