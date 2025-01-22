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
using System.Security.Cryptography;
using System.Text;

namespace ProtonVPN.Crypto;

public static class HashGenerator
{
    private static readonly RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();

    /// <summary>Returns a uniformly distributed unsigned integer based on the provided string</summary>
    /// <returns>uint between 0 and uint.Max (4294967295)</returns>
    public static uint HashToUint(string text)
    {
        byte[] hash;
        using (SHA512 sha512 = SHA512.Create())
        {
            hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(text));
        }
        return BitConverter.ToUInt32(hash);
    }

    /// <summary>Returns a uniformly distributed percentage between 0 and 1 based on the provided string</summary>
    /// <returns>decimal between 0 and 1</returns>
    public static decimal HashToPercentage(string text)
    {
        uint hash = HashToUint(text);
        return decimal.Divide(hash, uint.MaxValue);
    }

    /// <summary>Returns a cryptographically strong random string of any given length</summary>
    /// <returns>string with the defined argument length</returns>
    public static string GenerateRandomString(int length)
    {
        byte[] bytes = new byte[(length + 1) / 2];
        _randomNumberGenerator.GetBytes(bytes);
        StringBuilder sb = new();
        foreach (byte b in bytes)
        {
            sb.Append(b.ToString("X2"));
        }

        return sb.ToString().Substring(0, length);
    }
}