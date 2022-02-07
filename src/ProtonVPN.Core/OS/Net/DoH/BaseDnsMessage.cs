/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.Linq;

namespace ProtonVPN.Core.OS.Net.DoH
{
    public abstract class BaseDnsMessage : IDnsMessage
    {
        public string ToBase64String()
        {
            var bytes = GetBytes();
            var cleanBytes = RemoveTrailingZeroBytes(bytes);

            return Convert.ToBase64String(cleanBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        protected abstract byte[] GetBytes();

        private byte[] RemoveTrailingZeroBytes(byte[] bytes)
        {
            var i = bytes.Length - 1;
            while (bytes[i] == 0)
            {
                --i;
            }

            var newSize = i + 1;
            var result = new byte[newSize];
            Array.Copy(bytes, result, newSize);

            return result;
        }
    }
}
