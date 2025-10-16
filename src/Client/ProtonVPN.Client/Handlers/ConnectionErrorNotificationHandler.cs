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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Notifications.Contracts;

namespace ProtonVPN.Client.Handlers;

public class ConnectionErrorNotificationHandler : IHandler,
    IEventMessageReceiver<ConnectionErrorMessage>
{
    private readonly IConnectionErrorNotificationSender _connectionErrorNotificationSender;

    public ConnectionErrorNotificationHandler(
        IConnectionErrorNotificationSender connectionErrorNotificationSender)
    {
        _connectionErrorNotificationSender = connectionErrorNotificationSender;
    }

    public void Receive(ConnectionErrorMessage message)
    {
        if (message.VpnError.IsSessionLimitError())
        {
            _connectionErrorNotificationSender.SendSessionLimitNotification();
            return;
        }

        if (message.VpnError.IsTwoFactorRequiredError())
        {
            _connectionErrorNotificationSender.SendTwoFactorRequiredNotification();
            return;
        }
    }
}