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

namespace ProtonVPN.Crypto.Contracts.Extensions;

public static class EncryptionExtensions
{
    public static string Encrypt(this string data)
    {
        byte[] encryptedBytes = Encoding.UTF8.GetBytes(data).Encrypt();
        return Convert.ToBase64String(encryptedBytes);
    }

    public static string Decrypt(this string data)
    {
        byte[] encryptedBytes = Convert.FromBase64String(data);
        byte[] decryptedBytes = encryptedBytes.Decrypt();
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public static byte[] Encrypt(this byte[] data)
    {
        try
        {
            return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        }
        catch
        {
            return ProtectedData.Protect(data, null, DataProtectionScope.LocalMachine);
        }
    }

    public static byte[] Decrypt(this byte[] data)
    {
        try
        {
            return ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
        }
        catch
        {
            return ProtectedData.Unprotect(data, null, DataProtectionScope.LocalMachine);
        }
    }
}