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

namespace ProtonVPN.Common.Helpers
{
    public class StructConverter
    {
        public static byte[] Pack(object[] items, bool littleEndian)
        {
            var outputBytes = new List<byte>();
            var flip = littleEndian != BitConverter.IsLittleEndian;

            foreach (var o in items)
            {
                var theseBytes = TypeAgnosticGetBytes(o);
                if (flip)
                {
                    theseBytes = theseBytes.Reverse().ToArray();
                }

                outputBytes.AddRange(theseBytes);
            }

            return outputBytes.ToArray();
        }

        private static byte[] TypeAgnosticGetBytes(object o)
        {
            if (o is int i) return BitConverter.GetBytes(i);
            if (o is uint u) return BitConverter.GetBytes(u);
            if (o is long l) return BitConverter.GetBytes(l);
            if (o is ulong @ulong) return BitConverter.GetBytes(@ulong);
            if (o is short s) return BitConverter.GetBytes(s);
            if (o is ushort @ushort) return BitConverter.GetBytes(@ushort);
            if (o is byte || o is sbyte) return new[] { (byte)o };
            throw new ArgumentException("Unsupported object type found");
        }
    }
}
