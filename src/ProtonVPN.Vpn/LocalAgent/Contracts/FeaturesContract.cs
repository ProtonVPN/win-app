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

using Newtonsoft.Json;

namespace ProtonVPN.Vpn.LocalAgent.Contracts
{
    public class FeaturesContract
    {
        [JsonProperty(PropertyName = "randomized-nat")]
        public bool RandomizedNat { get; set; }

        [JsonProperty(PropertyName = "bouncing")]
        public string Bouncing { get; set; }

        [JsonProperty(PropertyName = "split-tcp")]
        public bool SplitTcp { get; set; }

        [JsonProperty(PropertyName = "netshield-level")]
        public int NetShieldLevel { get; set; }

        [JsonProperty(PropertyName = "safe-mode", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SafeMode { get; set; }

        [JsonProperty(PropertyName = "port-forwarding")]
        public bool PortForwarding { get; set; }
    }
}