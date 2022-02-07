/*
 * Copyright (c) 2022 Proton Technologies AG
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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Service.Contract.Crypto;

namespace ProtonVPN.Service.Contract.Vpn
{
    [DataContract]
    public class VpnHostContract : IValidatableObject
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Ip { get; set; }

        [DataMember]
        public string Label { get; set; }

        [DataMember]
        public ServerPublicKeyContract X25519PublicKey { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return X25519PublicKey == null ? ValidateIp() : ValidateIp().Concat(X25519PublicKey.Validate(validationContext));
        }

        public IEnumerable<ValidationResult> ValidateIp()
        {
            if (!Ip.IsValidIpAddress())
            {
                yield return new ValidationResult($"Invalid server IP address: {Ip} for server {Name}");
            }
        }
    }
}