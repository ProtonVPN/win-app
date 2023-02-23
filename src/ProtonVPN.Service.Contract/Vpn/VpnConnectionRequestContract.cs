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
using ProtonVPN.Service.Contract.Settings;

namespace ProtonVPN.Service.Contract.Vpn
{
    [DataContract]
    public class VpnConnectionRequestContract : IValidatableObject
    {
        [DataMember(IsRequired = true)]
        public VpnHostContract[] Servers { get; set; }

        [DataMember(IsRequired = true)]
        public VpnProtocolContract Protocol { get; set; }

        [DataMember(IsRequired = true)]
        public VpnConfigContract VpnConfig { get; set; }

        [DataMember(IsRequired = true)]
        public VpnCredentialsContract Credentials { get; set; }

        [DataMember(IsRequired = true)]
        public SettingsContract Settings { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return Credentials.Validate(validationContext)
                .Concat(VpnConfig.Validate(validationContext))
                .Concat(GetServerValidationResult(validationContext));
        }

        private IEnumerable<ValidationResult> GetServerValidationResult(ValidationContext validationContext)
        {
            IEnumerable<ValidationResult> result = new List<ValidationResult>();

            foreach (VpnHostContract server in Servers)
            {
                result = result.Concat(server.Validate(validationContext));
            }

            return result;
        }
    }
}