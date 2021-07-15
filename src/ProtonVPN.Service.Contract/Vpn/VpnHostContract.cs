/*
 * Copyright (c) 2021 Proton Technologies AG
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

namespace ProtonVPN.Service.Contract.Vpn
{
    [DataContract]
    public class VpnHostContract : IValidatableObject
    {
        private const int ServerPublicKeyLength = 44;

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Ip { get; set; }

        [DataMember]
        public string Label { get; set; }

        [DataMember]
        public string X25519PublicKey { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!X25519PublicKey.IsNullOrEmpty() && X25519PublicKey.Length != ServerPublicKeyLength ||
                !X25519PublicKey.IsValidBase64Key())
            {
                yield return new ValidationResult($"Invalid public key {X25519PublicKey} for server {Name}");
            }

            if (!Ip.IsValidIpAddress())
            {
                yield return new ValidationResult($"Invalid server IP address: {Ip} for server {Name}");
            }
        }
    }
}