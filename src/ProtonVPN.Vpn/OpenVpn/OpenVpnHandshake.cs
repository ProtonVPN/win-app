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
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ProtonVPN.Common.Legacy.Helpers;

namespace ProtonVPN.Vpn.OpenVpn;

internal class OpenVpnHandshake
{
    private readonly byte[] _key;

    public OpenVpnHandshake(byte[] key)
    {
        _key = key;
    }

    public byte[] Bytes(bool includeLength)
    {
        byte[] sid = GetRandomBytes(8);
        int ts = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        List<object> packet = [1, ts, (byte)(7 << 3), .. sid, (byte)0, 0];

        using HMACSHA512 h = new(_key);
        byte[] data = StructConverter.Pack(packet.ToArray(), false);
        byte[] hash = h.ComputeHash(data);

        List<object> result = [(byte)(7 << 3), .. sid, .. hash, 1, ts, (byte)0, 0];

        byte[] bytes = StructConverter.Pack(result.ToArray(), false);
        if (!includeLength)
        {
            return bytes;
        }

        byte[] length = StructConverter.Pack([(ushort)bytes.Length], false);
        return length.Concat(bytes).ToArray();
    }

    private byte[] GetRandomBytes(int length)
    {
        byte[] bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        return bytes;
    }
}