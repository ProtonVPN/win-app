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
using ProtonVPN.Client.Logic.Connection.Extensions;
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
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ConnectionDetails = ProtonVPN.Client.Logic.Connection.Contracts.Models.ConnectionDetails;

namespace ProtonVPN.Client.Logic.Connection;

public class ConnectionManager : IInternalConnectionManager,
    IEventMessageReceiver<ConnectionDetailsIpcEntity>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ILogger _logger;
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IEntityMapper _entityMapper;
    private readonly IConnectionRequestCreator _connectionRequestCreator;
    private readonly IReconnectionRequestCreator _reconnectionRequestCreator;
    private readonly IDisconnectionRequestCreator _disconnectionRequestCreator;
    private readonly IServersLoader _serversLoader;

    private TrafficBytes _bytesTransferred = TrafficBytes.Zero;

    public ConnectionStatus ConnectionStatus { get; private set; }
    public VpnErrorTypeIpcEntity CurrentError { get; private set; }
    public IConnectionIntent? CurrentConnectionIntent { get; private set; }
    public ConnectionDetails? CurrentConnectionDetails { get; private set; }

    public bool IsDisconnected => ConnectionStatus == ConnectionStatus.Disconnected;
    public bool IsConnecting => ConnectionStatus == ConnectionStatus.Connecting;
    public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
    public bool HasError => CurrentError is not VpnErrorTypeIpcEntity.None and not VpnErrorTypeIpcEntity.NoneKeepEnabledKillSwitch;

    public ConnectionManager(
        ILogger logger,
        IVpnServiceCaller vpnServiceCaller,
        IEventMessageSender eventMessageSender,
        IEntityMapper entityMapper,
        IConnectionRequestCreator connectionRequestCreator,
        IReconnectionRequestCreator reconnectionRequestCreator,
        IDisconnectionRequestCreator disconnectionRequestCreator,
        IServersLoader serversLoader)
    {
        _logger = logger;
        _vpnServiceCaller = vpnServiceCaller;
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

        CurrentConnectionIntent = connectionIntent;

        _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] Connection attempt to: {connectionIntent}.");

        SetConnectionStatus(ConnectionStatus.Connecting);

        ConnectionRequestIpcEntity request = await _connectionRequestCreator.CreateAsync(connectionIntent);

        await SendRequestIfValidAsync(request);
    }

    private async Task<bool> SendRequestIfValidAsync(ConnectionRequestIpcEntity request)
    {
        VpnError error = request.GetVpnError();
        if (error == VpnError.None)
        {
            await _vpnServiceCaller.ConnectAsync(request);
            return true;
        }
        else
        {
            SetConnectionStatus(ConnectionStatus.Disconnected);

            _eventMessageSender.Send(new ConnectionErrorMessage { VpnError = error });
            return false;
        }
    }

    /// <summary>Reconnects if the most recent action was a Connect and not a Disconnect.</summary>
    /// <returns>True if reconnecting. False if not.</returns>
    public async Task<bool> ReconnectAsync()
    {
        IConnectionIntent? connectionIntent = CurrentConnectionIntent;

        _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] Reconnection attempt to: {connectionIntent?.ToString() ?? "<no intent>"}.");

        if (connectionIntent is null)
        {
            await DisconnectAsync();
            return false;
        }

        SetConnectionStatus(ConnectionStatus.Connecting);

        ConnectionRequestIpcEntity request = await _reconnectionRequestCreator.CreateAsync(connectionIntent);

        return await SendRequestIfValidAsync(request);
    }

    public async Task DisconnectAsync()
    {
        _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] Disconnection attempt.");

        CurrentConnectionIntent = null;
        CurrentConnectionDetails = null;

        DisconnectionRequestIpcEntity request = _disconnectionRequestCreator.Create();

        await _vpnServiceCaller.DisconnectAsync(request);
    }

    public async Task<TrafficBytes> GetTrafficBytesAsync()
    {
        Result<TrafficBytesIpcEntity> trafficBytes = await _vpnServiceCaller.GetTrafficBytesAsync();
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
        IConnectionIntent connectionIntent = CurrentConnectionIntent ?? ConnectionIntent.Default;
        ConnectionStatus connectionStatus = _entityMapper.Map<VpnStatusIpcEntity, ConnectionStatus>(message.Status);

        if (message.Status == VpnStatusIpcEntity.Connected)
        {
            Server? server = GetCurrentServer(message);
            PhysicalServer? physicalServer = server?.Servers.FirstOrDefault(FilterPhysicalServerByVpnState(message));

            if (server is null || physicalServer is null)
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
                CurrentConnectionDetails = new ConnectionDetails(connectionIntent, server, physicalServer, vpnProtocol);
            }
        }

        SetConnectionStatus(connectionStatus, message.Error);
    }

    private Server? GetCurrentServer(VpnStateIpcEntity state)
    {
        //TODO: instead of EndpointIp and Label we should have VpnHost (including Id property) so we can easily find server by ID.
        return _serversLoader.GetServers().FirstOrDefault(s => s.Servers.Any(FilterPhysicalServerByVpnState(state)));
    }

    private Func<PhysicalServer, bool> FilterPhysicalServerByVpnState(VpnStateIpcEntity state)
    {
        return physicalServer => physicalServer.EntryIp == state.EndpointIp && physicalServer.Label == state.Label;
    }

    private void SetConnectionStatus(ConnectionStatus connectionStatus, VpnErrorTypeIpcEntity error = VpnErrorTypeIpcEntity.None)
    {
        if (ConnectionStatus == connectionStatus && CurrentError == error)
        {
            return;
        }

        ConnectionStatus = connectionStatus;
        CurrentError = error;
        _eventMessageSender.Send(new ConnectionStatusChanged(connectionStatus));

        _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] Status updated to {ConnectionStatus}.{(IsConnected ? $" Connected to server {CurrentConnectionDetails?.ServerName}" : string.Empty)}");
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
            message.NewValue is not null)
        {
            await _vpnServiceCaller.UpdateAuthCertificateAsync(new AuthCertificateIpcEntity
            {
                Certificate = (string)message.NewValue
            });
        }
    }
}