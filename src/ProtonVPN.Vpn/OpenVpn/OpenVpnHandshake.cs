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
using System.Linq;
using System.Security.Cryptography;
using ProtonVPN.Common.Helpers;

namespace ProtonVPN.Vpn.OpenVpn
{
    internal class OpenVpnHandshake
    {
        private readonly byte[] _key;
        private readonly RNGCryptoServiceProvider _rngCsp = new();

        public OpenVpnHandshake(byte[] key)
        {
            _key = key;
        }

        public byte[] Bytes(bool includeLength)
        {
            byte[] sid = GetRandomBytes(8);
            int ts = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var packet = new List<object> { 1, ts, (byte)(7 << 3) };
            foreach (byte s in sid)
            {
                packet.Add(s);
            }

            packet.Add((byte)0);
            packet.Add(0);

            using var h = new HMACSHA512(_key);
            byte[] data = StructConverter.Pack(packet.ToArray(), false);
            byte[] hash = h.ComputeHash(data);

            List<object> result = new List<object> { (byte)(7 << 3) };
            foreach (byte s in sid)
            {
                result.Add(s);
            }

            foreach (byte hs in hash)
            {
                result.Add(hs);
            }

            result.Add(1);
            result.Add(ts);
            result.Add((byte)0);
            result.Add(0);

            byte[] bytes = StructConverter.Pack(result.ToArray(), false);
            if (!includeLength)
            {
                return bytes;
            }

            byte[] length = StructConverter.Pack(new object[] { (ushort)bytes.Length }, false);
            return length.Concat(bytes).ToArray();
        }

        private byte[] GetRandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            _rngCsp.GetBytes(bytes);
            return bytes;
        }
    }
}