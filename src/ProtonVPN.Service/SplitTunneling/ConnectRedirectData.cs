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

using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace ProtonVPN.Service.SplitTunneling;

public class ConnectRedirectData
{
    private const int ADDRESS_FAMILY_SIZE = sizeof(ushort);
    private const int PADDING_SIZE = sizeof(ushort);
    private const int UNION_SIZE = 16;
    private const int TOTAL_SIZE = ADDRESS_FAMILY_SIZE + PADDING_SIZE + UNION_SIZE;

    private readonly IPAddress _ipAddress;

    public ConnectRedirectData(IPAddress ipAddress)
    {
        _ipAddress = ipAddress;
    }

    public byte[] Value()
    {
        byte[] buffer = new byte[TOTAL_SIZE];
        Span<byte> span = buffer.AsSpan();

        ushort family = _ipAddress.AddressFamily switch
        {
            AddressFamily.InterNetwork => (ushort)AddressFamily.InterNetwork,
            AddressFamily.InterNetworkV6 => (ushort)AddressFamily.InterNetworkV6,
            _ => throw new NotSupportedException($"Unsupported address family {_ipAddress.AddressFamily} for redirect data."),
        };

        BinaryPrimitives.WriteUInt16LittleEndian(span, family);

        byte[] addressBytes = _ipAddress.GetAddressBytes();
        switch (addressBytes.Length)
        {
            case 4:
            case 16:
                Array.Copy(addressBytes, 0, buffer, ADDRESS_FAMILY_SIZE + PADDING_SIZE, addressBytes.Length);
                break;
            default:
                throw new NotSupportedException($"Unexpected IP address length {addressBytes.Length}.");
        }

        return buffer;
    }

    public static implicit operator byte[](ConnectRedirectData item) => item?.Value() ?? Array.Empty<byte>();
}