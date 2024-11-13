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
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Notifications;

namespace ProtonVPN.Client.Legacy.Handlers;

public class ConnectionStatusNotificationHandler : IHandler, IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private readonly IConnectionStatusNotificationSender _connectionStatusNotificationSender;

    public ConnectionStatusNotificationHandler(IConnectionStatusNotificationSender connectionStatusNotificationSender)
    {
        _connectionStatusNotificationSender = connectionStatusNotificationSender;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        _connectionStatusNotificationSender.Send(message.ConnectionStatus);
    }
}