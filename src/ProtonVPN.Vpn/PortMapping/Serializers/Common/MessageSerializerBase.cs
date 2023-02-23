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
using ProtonVPN.Vpn.PortMapping.Messages.Common;

namespace ProtonVPN.Vpn.PortMapping.Serializers.Common
{
    public abstract class MessageSerializerBase<TMessage>
        where TMessage : MessageBase
    {
        protected abstract int ExpectedLength { get; }

        protected void ThrowIfSerializedMessageLengthIsInvalid(byte[] serializedMessage)
        {
            if (serializedMessage.Length != ExpectedLength)
            {
                throw new ArgumentOutOfRangeException("The received serialized " +
                    $"message for '{typeof(TMessage).Name}' has the wrong length. " +
                    $"Is {serializedMessage.Length} but expected {ExpectedLength}.");
            }
        }

        protected ushort DeserializeUshort(byte[] data, int startIndex)
        {
            return (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(data, startIndex));
        }

        protected uint DeserializeUint(byte[] data, int startIndex)
        {
            return (uint)IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(data, startIndex));
        }

        protected byte[] SerializeUshort(ushort data)
        {
            return BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)data));
        }

        protected byte[] SerializeUint(uint data)
        {
            return BitConverter.GetBytes((uint)IPAddress.HostToNetworkOrder((int)data));
        }
    }
}