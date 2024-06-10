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

using Microsoft.Toolkit.Uwp.Notifications;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Notifications.Contracts;

namespace ProtonVPN.Client.Notifications;

public class NotificationActivationHandler
{
    private readonly IEventMessageSender _eventMessageSender;

    public NotificationActivationHandler(IEventMessageSender eventMessageSender)
    {
        _eventMessageSender = eventMessageSender;
        ToastNotificationManagerCompat.OnActivated += OnActivated;
    }

    private void OnActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        _eventMessageSender.Send(new NotificationActivationMessage { Argument = e.Argument });
    }
}