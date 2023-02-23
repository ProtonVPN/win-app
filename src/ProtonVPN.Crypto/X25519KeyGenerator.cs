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
using System.Linq;
using System.Security.Cryptography;

namespace ProtonVPN.Crypto
{
    public class X25519KeyGenerator : IX25519KeyGenerator
    {
        public SecretKey FromEd25519SecretKey(SecretKey secretKey)
        {
            ThrowIfSecretKeyIsNotEd25519(secretKey);
            byte[] secretKeyLast32Bytes = secretKey.Bytes.Skip(secretKey.Bytes.Length - 32).Take(32).ToArray();

            byte[] x25519SecretKey;
            using (SHA512 shaM = new SHA512Managed())
            {
                x25519SecretKey = shaM.ComputeHash(secretKeyLast32Bytes);
            }

            x25519SecretKey = x25519SecretKey.Take(32).ToArray();
            x25519SecretKey[0] &= 0xF8;
            x25519SecretKey[31] = (byte)((x25519SecretKey[31] & 0x7F) | 0x40);

            return new SecretKey(x25519SecretKey, KeyAlgorithm.X25519);
        }

        private void ThrowIfSecretKeyIsNotEd25519(SecretKey secretKey)
        {
            if (secretKey.Algorithm != KeyAlgorithm.Ed25519)
            {
                throw new ArgumentException($"The Secret key provided does not use the '{KeyAlgorithm.Ed25519}' " +
                    $"algorithm, it uses the '{secretKey.Algorithm}' algorithm instead.");
            }
            if (secretKey.Bytes.Length < 32)
            {
                throw new ArgumentException($"The provided {KeyAlgorithm.Ed25519} secret key does not have at least 32 bytes.");
            }
        }
    }
}
