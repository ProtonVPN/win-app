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
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Logic.Connection.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Settings.Contracts;
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
    IEventMessageReceiver<ConnectionCertificateUpdatedMessage>
{
    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IEntityMapper _entityMapper;
    private readonly IConnectionRequestCreator _connectionRequestCreator;
    private readonly IReconnectionRequestCreator _reconnectionRequestCreator;
    private readonly IDisconnectionRequestCreator _disconnectionRequestCreator;
    private readonly IServersLoader _serversLoader;
    private readonly TimeSpan _reconnectInterval = TimeSpan.FromMinutes(1);

    private TrafficBytes? _bytesTransferred;
    private DateTime _minReconnectionDateUtc = DateTime.MinValue;
    private bool _isNetworkBlocked;
    private bool _isConnectionStatusHandled;

    public ConnectionStatus ConnectionStatus { get; private set; }
    public VpnErrorTypeIpcEntity CurrentError { get; private set; }
    public IConnectionIntent? CurrentConnectionIntent { get; private set; }
    public ConnectionDetails? CurrentConnectionDetails { get; private set; }

    public bool IsDisconnected => ConnectionStatus == ConnectionStatus.Disconnected;
    public bool IsConnecting => ConnectionStatus == ConnectionStatus.Connecting;
    public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
    public bool HasError => CurrentError is not VpnErrorTypeIpcEntity.None and not VpnErrorTypeIpcEntity.NoneKeepEnabledKillSwitch;
    public bool IsNetworkBlocked => _isNetworkBlocked;

    public ConnectionManager(
        ILogger logger,
        ISettings settings,
        IVpnServiceCaller vpnServiceCaller,
        IEventMessageSender eventMessageSender,
        IEntityMapper entityMapper,
        IConnectionRequestCreator connectionRequestCreator,
        IReconnectionRequestCreator reconnectionRequestCreator,
        IDisconnectionRequestCreator disconnectionRequestCreator,
        IServersLoader serversLoader)
    {
        _logger = logger;
        _settings = settings;
        _vpnServiceCaller = vpnServiceCaller;
        _eventMessageSender = eventMessageSender;
        _entityMapper = entityMapper;
        _connectionRequestCreator = connectionRequestCreator;
        _reconnectionRequestCreator = reconnectionRequestCreator;
        _disconnectionRequestCreator = disconnectionRequestCreator;
        _serversLoader = serversLoader;
    }

    public async Task ConnectAsync(IConnectionIntent? connectionIntent = null)
    {
        connectionIntent ??= _settings.VpnPlan.IsPaid ? ConnectionIntent.Default : ConnectionIntent.FreeDefault;
        CurrentConnectionIntent = CreateNewIntentIfUserPlanIsFree(connectionIntent);

        _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] Connection attempt to: {connectionIntent}.");

        SetConnectionStatus(ConnectionStatus.Connecting, forceSendStatusUpdate: true);

        ConnectionRequestIpcEntity request = await _connectionRequestCreator.CreateAsync(connectionIntent);

        await SendRequestIfValidAsync(request);
    }

    protected IConnectionIntent CreateNewIntentIfUserPlanIsFree(IConnectionIntent connectionIntent)
    {
        if (_settings.VpnPlan.IsPaid)
        {
            return connectionIntent;
        }

        ILocationIntent locationIntent = connectionIntent.Location.IsForPaidUsersOnly
            ? new FreeServerLocationIntent()
            : connectionIntent.Location;

        IFeatureIntent? featureIntent = connectionIntent.Feature is null || connectionIntent.Feature.IsForPaidUsersOnly
            ? null
            : connectionIntent.Feature;

        return new ConnectionIntent(locationIntent, featureIntent);
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

    /// <returns>True if reconnecting. False if not.</returns>
    public async Task<bool> ReconnectIfNotRecentlyReconnectedAsync()
    {
        if (DateTime.UtcNow > _minReconnectionDateUtc)
        {
            return await ReconnectAsync();
        }

        return false;
    }

    /// <summary>Reconnects if the most recent action was a Connect and not a Disconnect.</summary>
    /// <returns>True if reconnecting. False if not.</returns>
    public async Task<bool> ReconnectAsync()
    {
        _minReconnectionDateUtc = DateTime.UtcNow + _reconnectInterval;

        IConnectionIntent? connectionIntent = CurrentConnectionIntent;
        if (connectionIntent is null)
        {
            await DisconnectAsync();
            return false;
        }

        IConnectionIntent newConnectionIntent = CreateNewIntentIfUserPlanIsFree(connectionIntent);
        if (newConnectionIntent is null)
        {
            await DisconnectAsync();
            return false;
        }

        if (newConnectionIntent != connectionIntent)
        {
            _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] The reconnection attempt is changing the intent from " +
                $"{connectionIntent?.ToString() ?? "<no intent>"} to {newConnectionIntent?.ToString() ?? "<no intent>"}.");
            CurrentConnectionIntent = newConnectionIntent;
            connectionIntent = newConnectionIntent;
        }

        _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] Reconnection attempt to: {connectionIntent?.ToString() ?? "<no intent>"}.");


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

        // If the app starts after a crash and an active VPN connection, skip the first bytes
        // as it will display a huge spike equal to the amount of data downloaded/uploaded.
        if (_bytesTransferred is null)
        {
            _bytesTransferred = bytesTransferred;
            return TrafficBytes.Zero;
        }

        ulong downloadSpeed = bytesTransferred.BytesIn >= _bytesTransferred.Value.BytesIn
            ? bytesTransferred.BytesIn - _bytesTransferred.Value.BytesIn
            : 0;
        ulong uploadSpeed = bytesTransferred.BytesOut >= _bytesTransferred.Value.BytesOut
            ? bytesTransferred.BytesOut - _bytesTransferred.Value.BytesOut
            : 0;

        _bytesTransferred = bytesTransferred;

        return new TrafficBytes(downloadSpeed, uploadSpeed);
    }

    public async Task HandleAsync(VpnStateIpcEntity message)
    {
        IConnectionIntent connectionIntent = CurrentConnectionIntent ?? ConnectionIntent.Default;
        ConnectionStatus connectionStatus = _entityMapper.Map<VpnStatusIpcEntity, ConnectionStatus>(message.Status);
        bool isToForceStatusUpdate = _isNetworkBlocked != message.NetworkBlocked || !_isConnectionStatusHandled;

        _isConnectionStatusHandled = true;
        _isNetworkBlocked = message.NetworkBlocked;

        if (message.Status == VpnStatusIpcEntity.Connected)
        {
            Server? server = GetCurrentServer(message);
            PhysicalServer? physicalServer = server?.Servers.FirstOrDefault(FilterPhysicalServerByVpnState(message));

            if (server is not null && physicalServer is not null)
            {
                VpnProtocol vpnProtocol = _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(message.VpnProtocol);
                CurrentConnectionDetails = new ConnectionDetails(connectionIntent, server, physicalServer, vpnProtocol);
            }
            else if (server is null)
            {
                _logger.Error<AppLog>($"The status changed to Connected but the associated Server is null. Error: '{message.Error}' " +
                                      $"NetworkBlocked: '{message.NetworkBlocked}' " +
                                      $"Status: '{message.Status}' EntryIp: '{message.EndpointIp}' Label: '{message.Label}' " +
                                      $"NetworkAdapterType: '{message.OpenVpnAdapterType}' VpnProtocol: '{message.VpnProtocol}'");

                // VPNWIN-2105 - Either (1) Reconnect without last server, or (2) Delete this comment
                await ReconnectAsync();
            }
            else // Tier is too low for the connected server
            {
                await ReconnectIfNotRecentlyReconnectedAsync();
            }
        }

        if (message.Status != VpnStatusIpcEntity.ActionRequired)
        {
            SetConnectionStatus(connectionStatus, message.Error, isToForceStatusUpdate);
        }
    }

    private Server? GetCurrentServer(VpnStateIpcEntity state)
    {
        // VPNWIN-2113 - instead of EndpointIp and Label we should have VpnHost (including Id property) so we can easily find server by ID.
        return _serversLoader.GetServers().FirstOrDefault(s => s.Servers.Any(FilterPhysicalServerByVpnState(state)));
    }

    private Func<PhysicalServer, bool> FilterPhysicalServerByVpnState(VpnStateIpcEntity state)
    {
        return physicalServer => physicalServer.EntryIp == state.EndpointIp && physicalServer.Label == state.Label;
    }

    private void SetConnectionStatus(ConnectionStatus connectionStatus, VpnErrorTypeIpcEntity error = VpnErrorTypeIpcEntity.None, bool forceSendStatusUpdate = false)
    {
        if (ConnectionStatus == connectionStatus && CurrentError == error && !forceSendStatusUpdate)
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
        _eventMessageSender.Send(new ConnectionDetailsChanged
        {
            ClientCountryCode = message.ClientCountryIsoCode,
            ClientIpAddress = message.ClientIpAddress,
            ServerIpAddress = message.ServerIpAddress,
        });
    }

    public async void Receive(ConnectionCertificateUpdatedMessage message)
    {
        if (message.Certificate is not null)
        {
            await _vpnServiceCaller.UpdateConnectionCertificateAsync(new ConnectionCertificateIpcEntity
            {
                Pem = message.Certificate.Value.Pem,
                ExpirationDateUtc = message.Certificate.Value.ExpirationUtcDate.UtcDateTime
            });
        }
    }

    public async Task InitializeAsync(IConnectionIntent? connectionIntent)
    {
        CurrentConnectionIntent = connectionIntent;
        await _vpnServiceCaller.RequestConnectionDetailsAsync();
    }
}