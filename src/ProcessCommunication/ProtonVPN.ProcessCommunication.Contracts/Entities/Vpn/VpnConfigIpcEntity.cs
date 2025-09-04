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
using System.Runtime.Serialization;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Dns;

namespace ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

[DataContract]
public class VpnConfigIpcEntity
{
    [DataMember(Order = 1, IsRequired = true)]
    public IDictionary<VpnProtocolIpcEntity, int[]> Ports { get; set; }

    [DataMember(Order = 2, IsRequired = true)]
    public List<string> CustomDns { get; set; }

    [DataMember(Order = 3, IsRequired = true)]
    public SplitTunnelModeIpcEntity SplitTunnelMode { get; set; }

    [DataMember(Order = 4, IsRequired = true)]
    public List<string> SplitTunnelIPs { get; set; }

    [DataMember(Order = 5, IsRequired = true)]
    public int NetShieldMode { get; set; }

    [DataMember(Order = 6, IsRequired = true)]
    public VpnProtocolIpcEntity VpnProtocol { get; set; }

    [DataMember(Order = 7, IsRequired = true)]
    public IList<VpnProtocolIpcEntity> PreferredProtocols { get; set; }

    [DataMember(Order = 8, IsRequired = true)]
    public bool ModerateNat { get; set; }

    [DataMember(Order = 9, IsRequired = true)]
    public bool SplitTcp { get; set; }

    [DataMember(Order = 10, IsRequired = true)]
    public bool PortForwarding { get; set; }

    [DataMember(Order = 11, IsRequired = true)]
    public bool IsIpv6Enabled { get; set; }

    [DataMember(Order = 12, IsRequired = true)]
    public TimeSpan WireGuardConnectionTimeout { get; set; }

    [DataMember(Order = 13, IsRequired = true)]
    public DnsBlockModeIpcEntity DnsBlockMode { get; set; }

    public VpnConfigIpcEntity()
    {
        Ports = new Dictionary<VpnProtocolIpcEntity, int[]>();
        CustomDns = [];
        SplitTunnelIPs = [];
        PreferredProtocols = [];
    }
}