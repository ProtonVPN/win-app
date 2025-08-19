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

using System.Data;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Logic.Connection.Extensions;
using ProtonVPN.Client.Logic.Connection.GuestHole;
using ProtonVPN.Client.Logic.Connection.Statistics;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;
using ConnectionDetails = ProtonVPN.Client.Logic.Connection.Contracts.Models.ConnectionDetails;
using IpAddressInfo = ProtonVPN.Common.Core.Vpn.IpAddressInfo;

namespace ProtonVPN.Client.Logic.Connection;

public class ConnectionManager : IInternalConnectionManager, IGuestHoleConnector,
    IEventMessageReceiver<ConnectionDetailsIpcEntity>,
    IEventMessageReceiver<ConnectionCertificateUpdatedMessage>,
    IEventMessageReceiver<GuestHoleStatusChangedMessage>
{
    private readonly TimeSpan _reconnectInterval = TimeSpan.FromMinutes(1);
    private readonly Random _random = new();

    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IEntityMapper _entityMapper;
    private readonly IConnectionRequestCreator _connectionRequestCreator;
    private readonly IReconnectionRequestCreator _reconnectionRequestCreator;
    private readonly IDisconnectionRequestCreator _disconnectionRequestCreator;
    private readonly IServersLoader _serversLoader;
    private readonly IGuestHoleServersFileStorage _guestHoleServersFileStorage;
    private readonly IGuestHoleConnectionRequestCreator _guestHoleConnectionRequestCreator;
    private readonly IConnectionStatisticalEventsManager _statisticalEventManager;

    private DateTime _minReconnectionDateUtc = DateTime.MinValue;

    private bool _isNetworkBlocked;
    private bool _isConnectionStatusHandled;
    private bool _isGuestHoleActive;

    private VpnStateIpcEntity? _cachedMessage;
    private IpAddressInfo? _cachedServerIpAddress;

    private VpnStatusIpcEntity? _currentStatus = VpnStatusIpcEntity.Disconnected;
    private VpnErrorTypeIpcEntity? _currentError = VpnErrorTypeIpcEntity.None;

    public ConnectionStatus ConnectionStatus { get; private set; }
    public IConnectionIntent? CurrentConnectionIntent { get; private set; }
    public ConnectionDetails? CurrentConnectionDetails { get; private set; }

    public bool IsDisconnected => ConnectionStatus == ConnectionStatus.Disconnected;
    public bool IsConnecting => ConnectionStatus == ConnectionStatus.Connecting;
    public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
    public bool HasError => _currentError is not VpnErrorTypeIpcEntity.None and not VpnErrorTypeIpcEntity.NoneKeepEnabledKillSwitch;
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
        IServersLoader serversLoader,
        IGuestHoleServersFileStorage guestHoleServersFileStorage,
        IGuestHoleConnectionRequestCreator guestHoleConnectionRequestCreator,
        IConnectionStatisticalEventsManager statisticalEventManager)
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
        _guestHoleServersFileStorage = guestHoleServersFileStorage;
        _guestHoleConnectionRequestCreator = guestHoleConnectionRequestCreator;
        _guestHoleConnectionRequestCreator = guestHoleConnectionRequestCreator;
        _statisticalEventManager = statisticalEventManager;
    }

    public async Task ConnectAsync(
        VpnTriggerDimension connectionTrigger,
        IConnectionIntent? connectionIntent = null)
    {
        _statisticalEventManager.SetConnectionAttempt(connectionTrigger, ConnectionStatus);

        connectionIntent ??= _settings.VpnPlan.IsPaid ? ConnectionIntent.Default : ConnectionIntent.FreeDefault;
        connectionIntent = ChangeConnectionIntent(connectionIntent, CreateNewIntentIfUserPlanIsFree);

        CurrentConnectionIntent = connectionIntent;

        _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] Connection attempt to: {connectionIntent}.");

        ConnectionRequestIpcEntity request = await _connectionRequestCreator.CreateAsync(connectionIntent);

        await SendRequestIfValidAsync(request);
    }

    public async Task ConnectToGuestHoleAsync()
    {
        IOrderedEnumerable<GuestHoleServerContract> servers = (await _guestHoleServersFileStorage.GetAsync()).OrderBy(_ => _random.Next());
        if (!servers.Any())
        {
            throw new GuestHoleException("No guest hole servers provided.");
        }

        ConnectionRequestIpcEntity request = await _guestHoleConnectionRequestCreator.CreateAsync(servers);

        _logger.Info<ConnectTriggerLog>("Guest hole connection requested.");
        await _vpnServiceCaller.ConnectAsync(request);
    }

    public async Task DisconnectFromGuestHoleAsync()
    {
        DisconnectionRequestIpcEntity request = _disconnectionRequestCreator.Create(VpnError.NoneKeepEnabledKillSwitch);

        await _vpnServiceCaller.DisconnectAsync(request);
    }

    private IConnectionIntent CreateNewIntentIfUserPlanIsFree(IConnectionIntent connectionIntent)
    {
        if (_settings.VpnPlan.IsPaid)
        {
            return connectionIntent;
        }

        ILocationIntent locationIntent = connectionIntent.Location.IsForPaidUsersOnly
            ? FreeServerLocationIntent.Fastest
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
            await DisconnectAsync(VpnTriggerDimension.Auto);

            _eventMessageSender.Send(new ConnectionErrorMessage { VpnError = error });
            return false;
        }
    }

    /// <returns>True if reconnecting. False if not.</returns>
    public async Task<bool> ReconnectIfNotRecentlyReconnectedAsync()
    {
        if (DateTime.UtcNow > _minReconnectionDateUtc)
        {
            return await ReconnectAsync(VpnTriggerDimension.Auto);
        }

        return false;
    }

    /// <summary>Reconnects if the most recent action was a Connect and not a Disconnect.</summary>
    /// <returns>True if reconnecting. False if not.</returns>
    public async Task<bool> ReconnectAsync(VpnTriggerDimension connectionTrigger)
    {
        _minReconnectionDateUtc = DateTime.UtcNow + _reconnectInterval;

        IConnectionIntent? connectionIntent = CurrentConnectionIntent;
        if (connectionIntent is null)
        {
            await DisconnectAsync(VpnTriggerDimension.Auto);
            return false;
        }

        _statisticalEventManager.SetReconnectionAttempt(connectionTrigger, ConnectionStatus);

        connectionIntent = ChangeConnectionIntent(connectionIntent, CreateNewIntentIfUserPlanIsFree);

        CurrentConnectionIntent = connectionIntent;

        _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] Reconnection attempt to: {connectionIntent}.");

        ConnectionRequestIpcEntity request = await _reconnectionRequestCreator.CreateAsync(connectionIntent);

        return await SendRequestIfValidAsync(request);
    }

    public async Task DisconnectAsync(VpnTriggerDimension vpnTriggerDimension)
    {
        _statisticalEventManager.SetDisconnectionAttempt(vpnTriggerDimension, ConnectionStatus);

        _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] Disconnection attempt.");

        CurrentConnectionIntent = null;

        DisconnectionRequestIpcEntity request = _disconnectionRequestCreator.Create();

        await _vpnServiceCaller.DisconnectAsync(request);
    }

    public async Task HandleAsync(VpnStateIpcEntity message)
    {
        _cachedMessage = message;

        IConnectionIntent connectionIntent = CurrentConnectionIntent ?? ConnectionIntent.Default;
        bool isToForceStatusUpdate = _isNetworkBlocked != message.NetworkBlocked || !_isConnectionStatusHandled;

        _isConnectionStatusHandled = true;
        _isNetworkBlocked = message.NetworkBlocked;

        if (!_isGuestHoleActive)
        {
            if (message.Status is VpnStatusIpcEntity.Pinging or VpnStatusIpcEntity.Connected)
            {
                VpnProtocol vpnProtocol = _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(message.VpnProtocol);
                Server? server = GetCurrentServer(message, vpnProtocol);
                PhysicalServer? physicalServer = server?.Servers.FirstOrDefault(FilterPhysicalServerByVpnState(message, vpnProtocol));

                if (server is not null && physicalServer is not null)
                {
                    if (CurrentConnectionDetails is null || !CurrentConnectionDetails.OriginalConnectionIntent.IsSameAs(connectionIntent))
                    {
                        CurrentConnectionDetails = new ConnectionDetails(
                            connectionIntent,
                            server,
                            physicalServer,
                            vpnProtocol,
                            message.EndpointPort);
                    }
                    else
                    {
                        CurrentConnectionDetails.UpdateServer(server, physicalServer, vpnProtocol, message.EndpointPort);
                    }

                    if (_cachedServerIpAddress is not null)
                    {
                        CurrentConnectionDetails.UpdateServerIpAddress(_cachedServerIpAddress.Value);
                        _cachedServerIpAddress = null;
                    }
                }
                else if (server is null)
                {
                    _logger.Error<AppLog>($"The status changed to Connected but the associated Server is null. Error: '{message.Error}' " +
                                            $"NetworkBlocked: '{message.NetworkBlocked}' " +
                                            $"Status: '{message.Status}' EntryIp: '{message.EndpointIp}' Label: '{message.Label}' " +
                                            $"NetworkAdapterType: '{message.OpenVpnAdapterType}' VpnProtocol: '{message.VpnProtocol}'");

                    // VPNWIN-2105 - Either (1) Reconnect without last server, or (2) Delete this comment
                    await ReconnectAsync(VpnTriggerDimension.Auto);
                }
                else // Tier is too low for the connected server
                {
                    await ReconnectIfNotRecentlyReconnectedAsync();
                }
            }
            else if (message.Status == VpnStatusIpcEntity.Disconnected)
            {
                CurrentConnectionDetails = null;
            }
        }

        if (message.Status != VpnStatusIpcEntity.ActionRequired)
        {
            SetConnectionStatus(message.Status, message.Error, isToForceStatusUpdate);
        }
    }

    private Server? GetCurrentServer(VpnStateIpcEntity state, VpnProtocol vpnProtocol)
    {
        return _serversLoader.GetServers().FirstOrDefault(s => s.Servers.Any(FilterPhysicalServerByVpnState(state, vpnProtocol)));
    }

    private Func<PhysicalServer, bool> FilterPhysicalServerByVpnState(VpnStateIpcEntity state, VpnProtocol vpnProtocol)
    {
        return physicalServer => physicalServer.Label == state.Label
            && (physicalServer.EntryIp == state.EndpointIp ||
                (physicalServer.RelayIpByProtocol is not null &&
                 physicalServer.RelayIpByProtocol.ContainsKey(vpnProtocol) &&
                 physicalServer.RelayIpByProtocol[vpnProtocol] == state.EndpointIp));
    }

    private void SetConnectionStatus(
        VpnStatusIpcEntity status,
        VpnErrorTypeIpcEntity error,
        bool forceSendStatusUpdate = false)
    {
        if (_currentStatus == status && _currentError == error && !forceSendStatusUpdate)
        {
            return;
        }

        _currentStatus = status;
        _currentError = error;

        CurrentConnectionDetails?.UpdateStatus(status);

        ConnectionStatus = _entityMapper.Map<VpnStatusIpcEntity, ConnectionStatus>(status);

        _eventMessageSender.Send(new ConnectionStatusChangedMessage(ConnectionStatus));

        _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] Status updated to {ConnectionStatus}{(_isGuestHoleActive ? " (Guest hole)" : string.Empty)}.{(IsConnected ? $" Connected to server {CurrentConnectionDetails?.ServerName}" : string.Empty)}");

        _statisticalEventManager.OnVpnStateChanged(status, error, CurrentConnectionDetails);
    }

    public void Receive(ConnectionDetailsIpcEntity message)
    {
        IpAddressInfo serverIpAddress = _entityMapper.Map<VpnServerAddressIpcEntity, IpAddressInfo>(message.ServerIpAddress);
        _cachedServerIpAddress = serverIpAddress;

        CurrentConnectionDetails?.UpdateServerIpAddress(serverIpAddress);

        _eventMessageSender.Send(new ConnectionDetailsChangedMessage
        {
            ClientCountryCode = message.ClientCountryIsoCode,
            ClientIpAddress = message.ClientIpAddress,
            ServerIpAddress = serverIpAddress,
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

        if (_cachedMessage is not null)
        {
            await HandleAsync(_cachedMessage);
        }

        await _vpnServiceCaller.RequestConnectionDetailsAsync();
    }

    public void Receive(GuestHoleStatusChangedMessage message)
    {
        _isGuestHoleActive = message.IsActive;
    }

    private IConnectionIntent ChangeConnectionIntent(IConnectionIntent connectionIntent, Func<IConnectionIntent, IConnectionIntent> changeIntentFunc)
    {
        IConnectionIntent newConnectionIntent = changeIntentFunc(connectionIntent);
        if (newConnectionIntent != connectionIntent)
        {
            _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] The connection intent is changing from " +
                                            $"{connectionIntent} to {newConnectionIntent}.");
        }

        return newConnectionIntent;
    }
}