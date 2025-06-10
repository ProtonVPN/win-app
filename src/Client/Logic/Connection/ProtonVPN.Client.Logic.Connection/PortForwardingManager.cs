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
using ProtonVPN.Common.Legacy.PortForwarding;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;

namespace ProtonVPN.Client.Logic.Connection;

public class PortForwardingManager : IPortForwardingManager, IEventMessageReceiver<PortForwardingStateIpcEntity>
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IEntityMapper _entityMapper;
    private readonly ILogger _logger;
    private readonly IConnectionManager _connectionManager;

    private DateTime _lastPortChangeTimeUtc;
    private int? _port;
    private PortMappingStatus _status;

    protected bool IsConnectedToNonP2PServer => _connectionManager.IsConnected && _connectionManager.CurrentConnectionDetails?.IsP2P != true;

    public bool IsFetchingPort =>
        !IsConnectedToNonP2PServer &&
        _status is not PortMappingStatus.Stopped 
                    or PortMappingStatus.Error 
                    or PortMappingStatus.DestroyPortMappingCommunication;
    
    public int? ActivePort => IsConnectedToNonP2PServer ? null : _port;

    public DateTime? LastPortChangeTimeUtc => ActivePort.HasValue ? _lastPortChangeTimeUtc : null;

    public PortForwardingManager(
        IEventMessageSender eventMessageSender,
        IEntityMapper entityMapper, 
        ILogger logger,
        IConnectionManager connectionManager)
    {
        _eventMessageSender = eventMessageSender;
        _entityMapper = entityMapper;
        _logger = logger;
        _connectionManager = connectionManager;
    }

    public void Receive(PortForwardingStateIpcEntity message)
    {
        PortForwardingState portForwardingState = _entityMapper.Map<PortForwardingStateIpcEntity, PortForwardingState>(message);

        int? newPort = portForwardingState.MappedPort?.MappedPort?.ExternalPort;
        if (_port != newPort)
        {
            _logger.Info<AppLog>($"Port forwarding port changed from '{_port}' to '{newPort}'.");
            _port = newPort;
            _lastPortChangeTimeUtc = DateTime.UtcNow;
            NotifyPortChange(newPort);
        }

        PortMappingStatus newStatus = portForwardingState.Status;
        if (_status != newStatus)
        {
            _logger.Info<AppLog>($"Port forwarding status changed from '{_status}' to '{newStatus}'.");
            _status = newStatus;
            NotifyStatusChange(newStatus);
        }
    }

    private void NotifyPortChange(int? newPort)
    {
        _eventMessageSender.Send(new PortForwardingPortChangedMessage(newPort));
    }

    private void NotifyStatusChange(PortMappingStatus newStatus)
    {
        _eventMessageSender.Send(new PortForwardingStatusChangedMessage(newStatus));
    }
}