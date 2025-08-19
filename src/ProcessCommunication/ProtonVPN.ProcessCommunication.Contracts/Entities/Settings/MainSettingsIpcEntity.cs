/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;

[DataContract]
public class MainSettingsIpcEntity
{
    [DataMember(Order = 1)]
    public KillSwitchModeIpcEntity KillSwitchMode { get; set; }

    [DataMember(Order = 2)]
    public SplitTunnelSettingsIpcEntity SplitTunnel { get; set; }

    [DataMember(Order = 3)]
    public int NetShieldMode { get; set; }

    [DataMember(Order = 4)]
    public bool ModerateNat { get; set; }

    [DataMember(Order = 5)]
    public bool SplitTcp { get; set; }

    [DataMember(Order = 6)]
    public bool Ipv6LeakProtection { get; set; }

    [DataMember(Order = 7)]
    public bool IsIpv6FeatureFlagEnabled { get; set; }

    [DataMember(Order = 8)]
    public List<string> Ipv6Fragments { get; set; }

    [DataMember(Order = 9)]
    public bool IsShareCrashReportsEnabled { get; set; }

    [DataMember(Order = 10)]
    public bool IsLocalAreaNetworkAccessEnabled { get; set; }

    [DataMember(Order = 11)]
    public VpnProtocolIpcEntity VpnProtocol { get; set; }

    [DataMember(Order = 12)]
    public OpenVpnAdapterIpcEntity OpenVpnAdapter { get; set; }

    [DataMember(Order = 13)]
    public bool PortForwarding { get; set; }

    [DataMember(Order = 14)]
    public TimeSpan WireGuardConnectionTimeout { get; set; }

    public MainSettingsIpcEntity()
    {
        SplitTunnel = new();
    }
}