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

using System;
using System.Collections.Generic;
using ProtonVPN.Vpn.PortMapping.Messages.Common;

namespace ProtonVPN.Vpn.PortMapping.Serializers.Common
{
    public class MessageSerializerFactory : IMessageSerializerFactory
    {
        private readonly IDictionary<Type, IMessageSerializer> _messageSerializersByType = 
            new Dictionary<Type, IMessageSerializer>();

        public MessageSerializerFactory(IEnumerable<IMessageSerializer> messageSerializers)
        {
            if (messageSerializers is not null)
            {
                foreach (IMessageSerializer messageSerializer in messageSerializers)
                {
                    AddMessageSerializer(messageSerializer);
                }
            }
        }

        private void AddMessageSerializer(IMessageSerializer messageSerializer)
        {
            foreach (Type messageSerializerInterface in messageSerializer.GetType().GetInterfaces())
            {
                foreach (Type messageSerializerInterfaceGeneric in messageSerializerInterface.GenericTypeArguments)
                {
                    if (typeof(MessageBase).IsAssignableFrom(messageSerializerInterfaceGeneric))
                    {
                        _messageSerializersByType.Add(messageSerializerInterfaceGeneric, messageSerializer);
                        return;
                    }
                }
            }
        }

        public IMessageSerializer Get<TMessage>() 
            where TMessage : MessageBase
        {
            return _messageSerializersByType[typeof(TMessage)];
        }
    }
}