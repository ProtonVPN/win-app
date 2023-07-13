﻿/*
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

using ProtonVPN.Crypto.Contracts;

namespace ProtonVPN.Core.Auth
{
    public class AuthCredential
    {
        public AsymmetricKeyPair KeyPair { get; set; }
        public string CertificatePem { get; set; }

        public AuthCredential()
        {
        }

        public AuthCredential(AsymmetricKeyPair keyPair, string certificatePem)
        {
            KeyPair = keyPair;
            CertificatePem = certificatePem;
        }
    }
}