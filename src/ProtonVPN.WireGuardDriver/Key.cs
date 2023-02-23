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
using System.Runtime.InteropServices;

namespace ProtonVPN.WireGuardDriver
{
    public class Key
    {
        private byte[] _bytes;
        public byte[] Bytes
        {
            get => _bytes;
            set
            {
                if (value is not { Length: 32 })
                {
                    throw new ArgumentException("Keys must be 32 bytes");
                }

                _bytes = value;
            }
        }

        public Key(byte[] bytes)
        {
            Bytes = bytes;
        }

        public unsafe Key(byte* bytes)
        {
            _bytes = new byte[32];
            Marshal.Copy((IntPtr)bytes, _bytes, 0, 32);
        }

        public override string ToString()
        {
            return Convert.ToBase64String(_bytes);
        }
    }
}
