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
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ConnectionDetails = ProtonVPN.Client.Logic.Connection.Contracts.Models.ConnectionDetails;

namespace ProtonVPN.Client.Logic.Connection;

public class ConnectionManager : IInternalConnectionManager,
    IEventMessageReceiver<ConnectionDetailsIpcEntity>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ILogger _logger;
    private readonly IServiceCaller _serviceCaller;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IEntityMapper _entityMapper;
    private readonly IConnectionRequestCreator _connectionRequestCreator;
    private readonly IReconnectionRequestCreator _reconnectionRequestCreator;
    private readonly IDisconnectionRequestCreator _disconnectionRequestCreator;
    private readonly IServersLoader _serversLoader;

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
        IConnectionRequestCreator connectionRequestCreator,
        IReconnectionRequestCreator reconnectionRequestCreator,
        IDisconnectionRequestCreator disconnectionRequestCreator,
        IServersLoader serversLoader)
    {
        _logger = logger;
        _serviceCaller = serviceCaller;
        _eventMessageSender = eventMessageSender;
        _entityMapper = entityMapper;
        _connectionRequestCreator = connectionRequestCreator;
        _reconnectionRequestCreator = reconnectionRequestCreator;
        _disconnectionRequestCreator = disconnectionRequestCreator;
        _serversLoader = serversLoader;
    }

    public async Task ConnectAsync(IConnectionIntent? connectionIntent)
    {
        connectionIntent ??= ConnectionIntent.Default;

        _connectionDetails = new ConnectionDetails(connectionIntent);

        ConnectionRequestIpcEntity request = _connectionRequestCreator.Create(connectionIntent);

        await SendRequestIfValidAsync(request);
    }

    private async Task<bool> SendRequestIfValidAsync(ConnectionRequestIpcEntity request)
    {
        if (request.Servers.Length > 0)
        {
            SetConnectionStatus(ConnectionStatus.Connecting);
            await _serviceCaller.ConnectAsync(request);
            return true;
        }
        else
        {
            _eventMessageSender.Send(new ConnectionErrorMessage { VpnError = VpnError.NoServers });
            return false;
        }
    }

    /// <summary>Reconnects if the most recent action was a Connect and not a Disconnect.</summary>
    /// <returns>True if reconnecting. False if not.</returns>
    public async Task<bool> ReconnectAsync()
    {
        if (_connectionDetails is null)
        {
            await DisconnectAsync();
            return false;
        }

        IConnectionIntent connectionIntent = _connectionDetails?.OriginalConnectionIntent ?? ConnectionIntent.Default;

        _connectionDetails = new ConnectionDetails(connectionIntent);
        SetConnectionStatus(ConnectionStatus.Connecting);

        ConnectionRequestIpcEntity request = _reconnectionRequestCreator.Create(connectionIntent);

        return await SendRequestIfValidAsync(request);
    }

    public async Task DisconnectAsync()
    {
        _connectionDetails = null;

        DisconnectionRequestIpcEntity request = _disconnectionRequestCreator.Create();

        await _serviceCaller.DisconnectAsync(request);
    }

    public ConnectionDetails? GetConnectionDetails()
    {
        return _connectionDetails;
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

    public async Task HandleAsync(VpnStateIpcEntity message)
    {
        ConnectionStatus connectionStatus = _entityMapper.Map<VpnStatusIpcEntity, ConnectionStatus>(message.Status);

        if (_connectionDetails != null)
        {
            if (message.Status == VpnStatusIpcEntity.Connected)
            {
                Server? server = GetCurrentServer(message);
                if (server is null)
                {
                    _logger.Error<AppLog>($"The status changed to Connected but the associated Server is null. Error: '{message.Error}' " +
                                          $"NetworkBlocked: '{message.NetworkBlocked}' " +
                                          $"Status: '{message.Status}' EntryIp: '{message.EndpointIp}' Label: '{message.Label}' " +
                                          $"NetworkAdapterType: '{message.OpenVpnAdapterType}' VpnProtocol: '{message.VpnProtocol}'");

                    // TODO: Either (1) Reconnect without last server, or (2) Delete this comment
                    await ReconnectAsync();
                }
                else
                {
                    VpnProtocol vpnProtocol = _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(message.VpnProtocol);
                    _connectionDetails = new ConnectionDetails(_connectionDetails.OriginalConnectionIntent, server, vpnProtocol);
                }
            }
        }

        SetConnectionStatus(connectionStatus);
    }

    private Server? GetCurrentServer(VpnStateIpcEntity state)
    {
        //TODO: instead of EndpointIp and Label we should have VpnHost (including Id property) so we can easily find server by ID.
        return _serversLoader.GetServers()
            .FirstOrDefault(s => s.Servers.Any(physicalServer => physicalServer.EntryIp == state.EndpointIp && physicalServer.Label == state.Label));
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

    public void Receive(ConnectionDetailsIpcEntity message)
    {
        _eventMessageSender.Send(new ConnectionDetailsChanged()
        {
            ClientCountryCode = message.ClientCountryIsoCode,
            ClientIpAddress = message.ClientIpAddress,
            ServerIpAddress = message.ServerIpAddress,
        });
    }

    public async void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.AuthenticationCertificatePem) &&
            ConnectionStatus == ConnectionStatus.Connected &&
            message.NewValue is not null)
        {
            await _serviceCaller.UpdateAuthCertificateAsync(new AuthCertificateIpcEntity
            {
                Certificate = (string)message.NewValue
            });
        }
    }
}