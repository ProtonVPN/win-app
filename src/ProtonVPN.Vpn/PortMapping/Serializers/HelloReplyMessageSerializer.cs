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
using System.Net;
using ProtonVPN.Vpn.PortMapping.Messages;
using ProtonVPN.Vpn.PortMapping.Serializers.Common;

namespace ProtonVPN.Vpn.PortMapping.Serializers
{
    public class HelloReplyMessageSerializer : MessageSerializerBase<HelloReplyMessage>, 
        IMessageSerializerOfType<HelloReplyMessage>
    {
        protected override int ExpectedLength => 12;

        public HelloReplyMessage FromBytes(byte[] serializedMessage)
        {
            ThrowIfSerializedMessageLengthIsInvalid(serializedMessage);
            return new()
            {
                Version = serializedMessage[0],
                Operation = serializedMessage[1],
                ResultCode = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(serializedMessage, 2)),
                Timestamp = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(serializedMessage, 4)),
                ExternalIpv4Address = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(serializedMessage, 8))
            };
        }

        public byte[] ToBytes(HelloReplyMessage message)
        {
            byte[] bytes = new byte[ExpectedLength];
            bytes[0] = message.Version;
            bytes[1] = message.Operation;
            Array.Copy(SerializeUshort(message.ResultCode), 0, bytes, 2, 2);
            Array.Copy(SerializeUint(message.Timestamp), 0, bytes, 4, 4);
            Array.Copy(SerializeUint(message.ExternalIpv4Address), 0, bytes, 8, 4);
            return bytes;
        }
    }
}