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

using Newtonsoft.Json;

namespace ProtonVPN.Core.Api.Contracts
{
    public class VpnConfig : BaseResponse
    {
        public DefaultPorts DefaultPorts { get; set; }

        public int? ServerRefreshInterval { get; set; }

        public FeatureFlags FeatureFlags { get; set; }

        public SmartProtocol SmartProtocol { get; set; }
    }

    public class DefaultPorts
    {
        [JsonProperty(PropertyName = "OpenVPN")]
        public OpenVpnPorts OpenVpn { get; set; }

        public WireGuardPorts WireGuard { get; set; }
    }

    public class OpenVpnPorts
    {
        [JsonProperty(PropertyName = "UDP")] public int[] Udp { get; set; }

        [JsonProperty(PropertyName = "TCP")] public int[] Tcp { get; set; }
    }

    public class WireGuardPorts
    {
        [JsonProperty(PropertyName = "UDP")] public int[] Udp { get; set; }
    }

    public class FeatureFlags
    {
        public bool NetShield { get; set; }

        public bool GuestHoles { get; set; }

        public bool? ServerRefresh { get; set; }

        public bool? PortForwarding { get; set; }

        public bool? VpnAccelerator { get; set; }

        [JsonProperty(PropertyName = "PollNotificationAPI")]
        public bool? PollNotificationApi { get; set; }

        public bool? StreamingServicesLogos { get; set; }

        public bool? SmartReconnect { get; set; }

        public bool? SafeMode { get; set; }
    }

    public class SmartProtocol
    {
        public bool WireGuard { get; set; }
    }
}