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
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Notifications.Contracts;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.Handlers;

public class PortForwardingNotificationHandler : IHandler, IEventMessageReceiver<PortForwardingPortChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IPortForwardingNewPortNotificationSender _portForwardingNewPortNotificationSender;
    private readonly IPortForwardingManager _portForwardingManager;

    public PortForwardingNotificationHandler(
        ISettings settings,
        IPortForwardingNewPortNotificationSender portForwardingNewPortNotificationSender,
        IPortForwardingManager portForwardingManager)
    {
        _settings = settings;
        _portForwardingNewPortNotificationSender = portForwardingNewPortNotificationSender;
        _portForwardingManager = portForwardingManager;
    }

    public void Receive(PortForwardingPortChangedMessage message)
    {
        int? activePortNumber = _portForwardingManager.ActivePort;
        if (activePortNumber is not null && _settings.IsPortForwardingNotificationEnabled)
        {
            _portForwardingNewPortNotificationSender.Send(activePortNumber.Value);
        }
    }
}