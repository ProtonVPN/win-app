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
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;

namespace ProtonVPN.Client.Logic.Connection;

public class PortForwardingManager : IPortForwardingManager, IEventMessageReceiver<PortForwardingStateIpcEntity>
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IEntityMapper _entityMapper;

    public int? ActivePort { get; private set; }
    public PortMappingStatus Status { get; private set; }
    public bool IsFetchingPort => Status is not PortMappingStatus.Stopped or
        PortMappingStatus.Error or PortMappingStatus.DestroyPortMappingCommunication;

    public PortForwardingManager(IEventMessageSender eventMessageSender,
        IEntityMapper entityMapper)
    {
        _eventMessageSender = eventMessageSender;
        _entityMapper = entityMapper;
    }

    public void Receive(PortForwardingStateIpcEntity message)
    {
        PortForwardingState portForwardingState = _entityMapper.Map<PortForwardingStateIpcEntity, PortForwardingState>(message);

        int? newActivePort = portForwardingState.MappedPort?.MappedPort?.ExternalPort;
        if (ActivePort != newActivePort)
        {
            ActivePort = newActivePort;
            NotifyPortChange(newActivePort);
        }

        PortMappingStatus newStatus = portForwardingState.Status;
        if (Status != newStatus)
        {
            Status = newStatus;
            NotifyStatusChange(newStatus);
        }
    }

    private void NotifyPortChange(int? newActivePort)
    {
        _eventMessageSender.Send(new PortForwardingPortChanged(newActivePort));
    }

    private void NotifyStatusChange(PortMappingStatus newStatus)
    {
        _eventMessageSender.Send(new PortForwardingStatusChanged(newStatus));
    }
}