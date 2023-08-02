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

using System.Security.Cryptography;
using System.Text;
using ProtonVPN.Crypto.Contracts;

namespace ProtonVPN.Crypto;

public class Sha1Calculator : ISha1Calculator
{
    private const string LOWERCASE_HEXADECIMAL_FORMAT = "x2";

    private readonly SHA1 _sha1 = SHA1.Create();

    public string Hash(string input)
    {
        byte[] bytes = _sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

        StringBuilder builder = new(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            builder.Append(b.ToString(LOWERCASE_HEXADECIMAL_FORMAT));
        }
        return builder.ToString();
    }
}