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

using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Servers.Mappers;

public class PhysicalServerMapper : IMapper<PhysicalServerResponse, PhysicalServer>
{
    public PhysicalServer Map(PhysicalServerResponse leftEntity)
    {
        return leftEntity is null
            ? null
            : new()
            {
                Id = leftEntity.Id,
                EntryIp = leftEntity.EntryIp,
                ExitIp = string.Empty,
                Domain = leftEntity.Domain,
                Label = leftEntity.Label,
                Status = leftEntity.Status,
                X25519PublicKey = leftEntity.X25519PublicKey,
                Signature = leftEntity.Signature,
                RelayIpByProtocol = CreateRelayIpByProtocol(leftEntity.EntryPerProtocol)
            };
    }

    private Dictionary<VpnProtocol, string> CreateRelayIpByProtocol(EntryPerProtocolResponse entryPerProtocol)
    {
        Dictionary<VpnProtocol, string> relayIpByProtocol = [];

        if (!string.IsNullOrWhiteSpace(entryPerProtocol?.WireGuardUdp?.Ipv4))
        {
            relayIpByProtocol.Add(VpnProtocol.WireGuardUdp, entryPerProtocol.WireGuardUdp.Ipv4);
        }
        if (!string.IsNullOrWhiteSpace(entryPerProtocol?.WireGuardTcp?.Ipv4))
        {
            relayIpByProtocol.Add(VpnProtocol.WireGuardTcp, entryPerProtocol.WireGuardTcp.Ipv4);
        }
        if (!string.IsNullOrWhiteSpace(entryPerProtocol?.WireGuardTls?.Ipv4))
        {
            relayIpByProtocol.Add(VpnProtocol.WireGuardTls, entryPerProtocol.WireGuardTls.Ipv4);
        }
        if (!string.IsNullOrWhiteSpace(entryPerProtocol?.OpenVpnUdp?.Ipv4))
        {
            relayIpByProtocol.Add(VpnProtocol.OpenVpnUdp, entryPerProtocol.OpenVpnUdp.Ipv4);
        }
        if (!string.IsNullOrWhiteSpace(entryPerProtocol?.OpenVpnTcp?.Ipv4))
        {
            relayIpByProtocol.Add(VpnProtocol.OpenVpnTcp, entryPerProtocol.OpenVpnTcp.Ipv4);
        }

        return relayIpByProtocol;
    }

    public PhysicalServerResponse Map(PhysicalServer rightEntity)
    {
        throw new NotImplementedException("We don't need to map to API responses.");
    }
}