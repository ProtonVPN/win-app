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
using System.Net;
using System.Runtime.Serialization;
using ProtonVPN.Common;

namespace ProtonVPN.Service.Contract.Vpn
{
    [DataContract]
    public class VpnConfigContract : IValidatableObject
    {
        [DataMember(IsRequired = true)]
        public IDictionary<VpnProtocolContract, int[]> Ports { get; set; }

        [DataMember(IsRequired = true)]
        public List<string> CustomDns { get; set; }

        [DataMember(IsRequired = true)]
        public SplitTunnelMode SplitTunnelMode { get; set; }

        [DataMember(IsRequired = true)]
        public List<string> SplitTunnelIPs { get; set; }

        [DataMember(IsRequired = true)]
        public int NetShieldMode { get; set; }

        [DataMember(IsRequired = true)]
        public VpnProtocolContract VpnProtocol { get; set; }

        [DataMember(IsRequired = true)]
        public IList<VpnProtocolContract> PreferredProtocols { get; set; }

        [DataMember(IsRequired = true)]
        public bool ModerateNat { get; set; }

        [DataMember(IsRequired = true)]
        public bool SplitTcp { get; set; }

        [DataMember(IsRequired = true)]
        public bool? AllowNonStandardPorts { get; set; }

        [DataMember(IsRequired = true)]
        public bool PortForwarding { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (string address in CustomDns)
            {
                if (!IPAddress.TryParse(address, out _))
                {
                    yield return new ValidationResult("Incorrect IP address format: " + address);
                }
            }
        }
    }
}