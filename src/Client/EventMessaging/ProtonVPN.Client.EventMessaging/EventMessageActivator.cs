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

using CommunityToolkit.Mvvm.Messaging;
using ProtonVPN.Client.EventMessaging.Contracts;

namespace ProtonVPN.Client.EventMessaging
{
    public class EventMessageActivator
    {
        private readonly IMessenger _messenger = MessengerFactory.Get();

        public EventMessageActivator(IEnumerable<IEventMessageReceiver> eventMessageReceivers)
        {
            _messenger.Reset();
            foreach (IEventMessageReceiver eventMessageReceiver in eventMessageReceivers)
            {
                try
                {
                    _messenger.RegisterAll(eventMessageReceiver);
                }
                catch (InvalidOperationException ex) when (ex.Message == "The target recipient has already subscribed to the target message.")
                {
                }
            }
        }
    }
}