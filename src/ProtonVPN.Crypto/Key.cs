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

namespace ProtonVPN.Crypto
{
    public abstract class Key
    {
        public byte[] Bytes { get; }
        public string Base64 { get; }
        public KeyAlgorithm Algorithm { get; }
        public string Pem { get; }

        protected Key(byte[] bytes, KeyAlgorithm algorithm)
            : this(bytes, Convert.ToBase64String(bytes), algorithm)
        {
        }

        protected Key(string base64, KeyAlgorithm algorithm)
            : this(Convert.FromBase64String(base64), base64, algorithm)
        {
        }

        private Key(byte[] bytes, string base64, KeyAlgorithm algorithm)
        {
            if (bytes == null || bytes.Length <= 0)
            {
                throw new ArgumentException($"The '{nameof(bytes)}' argument is empty.");
            }
            if (string.IsNullOrEmpty(base64))
            {
                throw new ArgumentException($"The '{nameof(base64)}' argument is empty.");
            }
            Bytes = bytes;
            Base64 = base64;
            Algorithm = algorithm;
            Pem = CreatePem();
        }

        protected abstract string CreatePem();
    }
}