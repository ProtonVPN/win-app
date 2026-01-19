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
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Restrictions;

namespace ProtonVPN.Client.Logic.Connection;

public class RestrictionsObserver : IEventMessageReceiver<RestrictionsIpcEntity>
{
    private readonly IEventMessageSender _eventMessageSender;

    public RestrictionsObserver(IEventMessageSender eventMessageSender)
    {
        _eventMessageSender = eventMessageSender;
    }

    public void Receive(RestrictionsIpcEntity message)
    {
        if (message.Restrictions == null || message.Restrictions.Count == 0)
        {
            return;
        }

        if (message.Restrictions.Contains(RestrictionIpcEntity.Bittorrenting))
        {
            _eventMessageSender.Send<P2PTrafficDetectedMessage>();
        }

        if (message.Restrictions.Contains(RestrictionIpcEntity.Streaming))
        {
            _eventMessageSender.Send<StreamingTrafficDetectedMessage>();
        }
    }
}