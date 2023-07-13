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
using ProtonVPN.Crypto.Contracts;

namespace ProtonVPN.Crypto;

/// <summary> Generates random alphanumeric strings. </summary>
public class RandomStringGenerator : IRandomStringGenerator
{
    private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private readonly SecureRandom _random = new();

    public string Generate(int length)
    {
        if (length < 0)
        {
            throw new ArgumentException($"Generate length can't be a negative number but is {length}.");
        }

        char[] randomChars = new char[length];

        for (int i = 0; i < randomChars.Length; i++)
        {
            randomChars[i] = CHARS[_random.Next(CHARS.Length)];
        }

        return new string(randomChars);
    }
}