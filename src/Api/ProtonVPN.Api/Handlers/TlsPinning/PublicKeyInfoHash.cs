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
using System.Security.Cryptography.X509Certificates;
using ProtonVPN.Api.Extensions;

namespace ProtonVPN.Api.Handlers.TlsPinning
{
    public class PublicKeyInfoHash
    {
        private readonly X509Certificate _certificate;

        public PublicKeyInfoHash(X509Certificate certificate)
        {
            _certificate = certificate;
        }

        public string Value()
        {
            byte[] digest;
            using (var sha256 = new SHA256CryptoServiceProvider())
            {
                digest = sha256.ComputeHash(_certificate.GetSubjectPublicKeyInfo());
            }

            return Convert.ToBase64String(digest);
        }
    }
}