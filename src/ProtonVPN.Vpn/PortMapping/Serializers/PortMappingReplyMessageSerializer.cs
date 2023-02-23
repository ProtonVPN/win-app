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
using ProtonVPN.Vpn.PortMapping.Messages;
using ProtonVPN.Vpn.PortMapping.Serializers.Common;

namespace ProtonVPN.Vpn.PortMapping.Serializers
{
    public class PortMappingReplyMessageSerializer : MessageSerializerBase<PortMappingReplyMessage>, 
        IMessageSerializerOfType<PortMappingReplyMessage>
    {
        protected override int ExpectedLength => 16;

        public PortMappingReplyMessage FromBytes(byte[] serializedMessage)
        {
            ThrowIfSerializedMessageLengthIsInvalid(serializedMessage);
            return new()
            {
                Version = serializedMessage[0],
                Operation = serializedMessage[1],
                ResultCode = DeserializeUshort(serializedMessage, 2),
                StartOfEpochSecond = DeserializeUint(serializedMessage, 4),
                InternalPort = DeserializeUshort(serializedMessage, 8),
                ExternalPort = DeserializeUshort(serializedMessage, 10),
                LifetimeSeconds = DeserializeUint(serializedMessage, 12)
            };
        }

        public byte[] ToBytes(PortMappingReplyMessage message)
        {
            byte[] bytes = new byte[ExpectedLength];
            bytes[0] = message.Version;
            bytes[1] = message.Operation;
            Array.Copy(SerializeUshort(message.ResultCode), 0, bytes, 2, 2);
            Array.Copy(SerializeUint(message.StartOfEpochSecond), 0, bytes, 4, 4);
            Array.Copy(SerializeUshort(message.InternalPort), 0, bytes, 8, 2);
            Array.Copy(SerializeUshort(message.ExternalPort), 0, bytes, 10, 2);
            Array.Copy(SerializeUint(message.LifetimeSeconds), 0, bytes, 12, 4);
            return bytes;
        }
    }
}