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
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;

namespace ProtonVPN.Crypto
{
    public class Ed25519SignatureValidator : IEd25519SignatureValidator
    {
        public bool IsValid(string data, string base64Signature, string base64PublicKey)
        {
            AsymmetricKeyParameter publicKey = PublicKeyFactory.CreateKey(Convert.FromBase64String(base64PublicKey));
            Ed25519Signer validator = GetSignatureValidator(publicKey, data);
            return validator.VerifySignature(Convert.FromBase64String(base64Signature));
        }

        private Ed25519Signer GetSignatureValidator(AsymmetricKeyParameter publicKey, string data)
        {
            Ed25519Signer validator = new();
            validator.Init(false, publicKey);
            validator.BlockUpdate(Encoding.ASCII.GetBytes(data), 0, data.Length);
            return validator;
        }
    }
}