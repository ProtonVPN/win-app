/*
 * Copyright (c) 2026 Proton AG
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
using System.Globalization;
using System.Text;
using ARSoft.Tools.Net.Dns;

namespace ProtonVPN.Dns.Resolvers;

/// <summary>Encodes a single-question DNS query in RFC 1035 wire format.</summary>
public static class DnsQueryEncoder
{
    private const ushort RECURSION_DESIRED_FLAG = 0x0100;
    private const int MAX_LABEL_LENGTH = 63;
    private const int MAX_NAME_LENGTH = 255;

    private static readonly IdnMapping _idnMapping = new();

    public static byte[] Encode(string host, RecordType recordType)
    {
        List<byte> message = new();

        AddUShort(message, 0); // Transaction ID, 0 per RFC 8484 so responses can be cached
        AddUShort(message, RECURSION_DESIRED_FLAG);
        AddUShort(message, 1); // Question count
        AddUShort(message, 0); // Answer count
        AddUShort(message, 0); // Authority count
        AddUShort(message, 0); // Additional count

        AddName(message, host);
        AddUShort(message, (ushort)recordType);
        AddUShort(message, (ushort)RecordClass.INet);

        return message.ToArray();
    }

    private static void AddName(List<byte> message, string host)
    {
        string asciiHost = _idnMapping.GetAscii(host.TrimEnd('.'));
        int nameStart = message.Count;

        foreach (string label in asciiHost.Split('.'))
        {
            if (label.Length is 0 or > MAX_LABEL_LENGTH)
            {
                throw new ArgumentException($"Invalid DNS label length in host '{host}'.", nameof(host));
            }

            message.Add((byte)label.Length);
            message.AddRange(Encoding.ASCII.GetBytes(label));
        }

        message.Add(0);

        if (message.Count - nameStart > MAX_NAME_LENGTH)
        {
            throw new ArgumentException($"Host '{host}' is too long for DNS.", nameof(host));
        }
    }

    private static void AddUShort(List<byte> message, ushort value)
    {
        message.Add((byte)(value >> 8));
        message.Add((byte)value);
    }
}