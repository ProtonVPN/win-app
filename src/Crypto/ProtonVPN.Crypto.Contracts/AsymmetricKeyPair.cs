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

namespace ProtonVPN.Crypto.Contracts;

public class AsymmetricKeyPair
{
    public SecretKey SecretKey { get; }
    public PublicKey PublicKey { get; }

    public AsymmetricKeyPair(SecretKey secretKey, PublicKey publicKey)
    {
        if (secretKey is null)
        {
            throw new ArgumentNullException(nameof(secretKey), "The SecretKey argument is mandatory but is null.");
        }
        if (publicKey is null)
        {
            throw new ArgumentNullException(nameof(publicKey), "The PublicKey argument is mandatory but is null.");
        }
        if (secretKey.Algorithm != publicKey.Algorithm)
        {
            throw new ArgumentException("The algorithms used for the key pair are different. " +
                                        $"Secret key algorithm is '{secretKey.Algorithm}' and Public key algorithm is '{publicKey.Algorithm}'.");
        }

        SecretKey = secretKey;
        PublicKey = publicKey;
    }
}