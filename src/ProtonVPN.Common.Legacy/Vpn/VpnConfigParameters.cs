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
using System.Collections.Generic;
using ProtonVPN.Common.Core.Dns;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Common.Legacy.Vpn;

public class VpnConfigParameters
{
    public IReadOnlyDictionary<VpnProtocol, IReadOnlyCollection<int>> Ports { get; set; }
    public IReadOnlyCollection<string> CustomDns { get; set; }
    public SplitTunnelMode SplitTunnelMode { get; set; }
    public IReadOnlyCollection<string> SplitTunnelIPs { get; set; }
    public OpenVpnAdapter OpenVpnAdapter { get; set; }
    public VpnProtocol VpnProtocol { get; set; }
    public IList<VpnProtocol> PreferredProtocols { get; set; }
    public int NetShieldMode { get; set; }
    public bool SplitTcp { get; set; }
    public bool ModerateNat { get; set; }
    public bool PortForwarding { get; set; }
    public bool IsIpv6Enabled { get; set; }
    public TimeSpan WireGuardConnectionTimeout { get; set; }
    public DnsBlockMode DnsBlockMode { get; set; }

    public VpnConfigParameters()
    {
        Ports = new Dictionary<VpnProtocol, IReadOnlyCollection<int>>();
        CustomDns = [];
        SplitTunnelIPs = [];
    }
}