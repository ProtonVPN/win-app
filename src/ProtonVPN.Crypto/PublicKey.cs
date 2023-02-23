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

namespace ProtonVPN.Crypto
{
    public sealed class PublicKey : Key
    {
        public PublicKey(byte[] bytes, KeyAlgorithm algorithm) 
            : base(bytes, algorithm)
        {
        }

        public PublicKey(string base64, KeyAlgorithm algorithm) 
            : base(base64, algorithm)
        {
        }

        protected override string CreatePem()
        {
            return $"-----BEGIN PUBLIC KEY-----\r\n{Base64}\r\n-----END PUBLIC KEY-----";
        }
    }
}