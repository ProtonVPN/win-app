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
using System.Linq;
using System.Runtime.Serialization;
using ProtonVPN.Crypto;

namespace ProtonVPN.Service.Contract.Crypto
{
    [DataContract]
    public class AsymmetricKeyPairContract : IValidatableObject
    {
        [DataMember]
        public SecretKeyContract SecretKey { get; set; }

        [DataMember]
        public PublicKeyContract PublicKey { get; set; }

        public AsymmetricKeyPairContract()
        {
        }

        public AsymmetricKeyPairContract(AsymmetricKeyPair asymmetricKeyPair)
        {
            SecretKey = new SecretKeyContract(asymmetricKeyPair.SecretKey);
            PublicKey = new PublicKeyContract(asymmetricKeyPair.PublicKey);
        }

        public AsymmetricKeyPair ConvertBack()
        {
            return new(SecretKey.ConvertBack(), PublicKey.ConvertBack());
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return PublicKey.Validate(validationContext).Concat(SecretKey.Validate(validationContext));
        }
    }
}