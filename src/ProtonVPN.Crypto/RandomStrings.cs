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
using Org.BouncyCastle.Security;

namespace ProtonVPN.Crypto
{
    /// <summary> Generates random alphanumeric strings. </summary>
    public class RandomStrings
    {
        private readonly SecureRandom _random = new();

        public string RandomString(int length)
        {
            if (length < 0)
            {
                throw new ArgumentException($"RandomString length can't be a negative number but is {length}.");
            }

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] randomChars = new char[length];

            for (int i = 0; i < randomChars.Length; i++)
            {
                randomChars[i] = chars[_random.Next(chars.Length)];
            }

            return new string(randomChars);
        }
    }
}
