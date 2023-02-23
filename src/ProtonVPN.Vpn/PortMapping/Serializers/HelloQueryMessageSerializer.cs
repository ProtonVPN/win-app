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

using ProtonVPN.Vpn.PortMapping.Messages;
using ProtonVPN.Vpn.PortMapping.Serializers.Common;

namespace ProtonVPN.Vpn.PortMapping.Serializers
{
    public class HelloQueryMessageSerializer : MessageSerializerBase<HelloQueryMessage>, 
        IMessageSerializerOfType<HelloQueryMessage>
    {
        protected override int ExpectedLength => 2;

        public HelloQueryMessage FromBytes(byte[] serializedMessage)
        {
            ThrowIfSerializedMessageLengthIsInvalid(serializedMessage);
            return new()
            {
                Version = serializedMessage[0],
                Operation = serializedMessage[1]
            };
        }

        public byte[] ToBytes(HelloQueryMessage message)
        {
            return new[] { message.Version, message.Operation };
        }
    }
}