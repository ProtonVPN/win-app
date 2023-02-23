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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Crypto;

namespace ProtonVPN.Service.Contract.Crypto
{
    [DataContract]
    public abstract class KeyContract : IValidatableObject
    {
        [DataMember]
        public byte[] Bytes { get; set; }

        [DataMember]
        public string Base64 { get; set; }

        [DataMember]
        public KeyAlgorithmContract Algorithm { get; set; }

        [DataMember]
        public string Pem { get; set; }

        protected abstract int KeyLength { get; }

        protected KeyContract()
        {
        }

        protected KeyContract(Key key)
        {
            Bytes = key.Bytes;
            Base64 = key.Base64;
            Algorithm = key.Algorithm.ToString().ToEnum<KeyAlgorithmContract>();
            Pem = key.Pem;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Base64.Length != KeyLength)
            {
                yield return new ValidationResult($"Incorrect key length {Base64.Length}, should be {KeyLength}");
            }

            if (!Base64.IsValidBase64Key())
            {
                yield return new ValidationResult($"Incorrect key value: {Base64}");
            }
        }
    }
}