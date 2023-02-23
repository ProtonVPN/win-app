/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software:
 you can redistribute it and/or modify
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

using System.Linq;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace ProtonVPN.Crypto
{
    public class Ed25519Asn1KeyGenerator : IEd25519Asn1KeyGenerator
    {
        public static readonly byte[] SecretKeyAsn1Header = { 0x30, 0x2E, 0x02, 0x01, 0x00, 0x30, 0x05, 0x06, 0x03, 0x2B, 0x65, 0x70, 0x04, 0x22, 0x04, 0x20 };
        public static readonly byte[] PublicKeyAsn1Header = { 0x30, 0x2A, 0x30, 0x05, 0x06, 0x03, 0x2B, 0x65, 0x70, 0x03, 0x21, 0x00 };

        private readonly SecureRandom _secureRandom = new SecureRandom();

        public AsymmetricKeyPair Generate()
        {
            Ed25519PrivateKeyParameters secretKeyParams = new Ed25519PrivateKeyParameters(_secureRandom);
            Ed25519PublicKeyParameters publicKeyParams = secretKeyParams.GeneratePublicKey();
            
            byte[] asn1SecretKey = SecretKeyAsn1Header.Concat(secretKeyParams.GetEncoded()).ToArray();
            byte[] asn1PublicKey = PublicKeyAsn1Header.Concat(publicKeyParams.GetEncoded()).ToArray();

            return new AsymmetricKeyPair(
                new SecretKey(asn1SecretKey, KeyAlgorithm.Ed25519), 
                new PublicKey(asn1PublicKey, KeyAlgorithm.Ed25519));
        }
    }
}
