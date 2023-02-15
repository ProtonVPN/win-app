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
using System.Security.Cryptography;

namespace ProtonVPN.Common.Helpers
{
    /// <summary>
    /// Generates random alphanumeric strings.
    /// </summary>
    public class RandomStrings
    {
        private readonly RNGCryptoServiceProvider _random = new RNGCryptoServiceProvider();

        public string RandomString(int length)
        {
            Ensure.IsTrue(length >= 0);

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var randomChars = new char[length];

            for (var i = 0; i < randomChars.Length; i++)
            {
                randomChars[i] = chars[Next(chars.Length)];
            }

            return new string(randomChars);
        }

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        private int Next(int maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue));
            }

            if (maxValue == 0)
            {
                return 0;
            }

            while (true)
            {
                uint rand = GetRandomUInt32();

                long max = 1 + (long)uint.MaxValue;

                if (rand < max)
                {
                    return (int)(rand % maxValue);
                }
            }
        }

        /// <summary>
        /// Gets one random unsigned 32bit integer in a thread safe manner.
        /// </summary>
        private uint GetRandomUInt32()
        {
            lock (this)
            {
                var buffer = new byte[sizeof(uint)];
                _random.GetBytes(buffer, 0, buffer.Length);
                uint rand = BitConverter.ToUInt32(buffer, 0);
                return rand;
            }
        }
    }
}
