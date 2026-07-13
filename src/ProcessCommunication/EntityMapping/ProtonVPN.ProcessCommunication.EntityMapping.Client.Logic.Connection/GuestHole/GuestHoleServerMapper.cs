/*
 * Copyright (c) 2026 Proton AG
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

using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Client.Logic.Connection.GuestHole;

public class GuestHoleServerMapper : IMapper<GuestHoleServerContract, VpnServerIpcEntity>
{
    private readonly IEntityMapper _entityMapper;

    public GuestHoleServerMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public VpnServerIpcEntity Map(GuestHoleServerContract leftEntity)
    {
        return leftEntity is null
            ? null
            : new VpnServerIpcEntity()
            {
                Name = leftEntity.Host,
                Ip = leftEntity.Ip,
                Label = leftEntity.Label,
                Signature = leftEntity.Signature,
                X25519PublicKey = _entityMapper.Map<PublicKey, ServerPublicKeyIpcEntity>(
                    new PublicKey(leftEntity.X25519PublicKey, KeyAlgorithm.X25519)),
                RelayIpByProtocol = CreateRelayIpByProtocol(leftEntity.EntryPerProtocol),
            };
    }

    private Dictionary<VpnProtocolIpcEntity, string> CreateRelayIpByProtocol(GuestHoleEntryPerProtocolContract entryPerProtocol)
    {
        Dictionary<VpnProtocolIpcEntity, string> relayIpByProtocol = [];

        if (!string.IsNullOrWhiteSpace(entryPerProtocol?.WireGuardUdp?.Ipv4))
        {
            relayIpByProtocol.Add(VpnProtocolIpcEntity.WireGuardUdp, entryPerProtocol.WireGuardUdp.Ipv4);
            relayIpByProtocol.Add(VpnProtocolIpcEntity.ProTunUdp, entryPerProtocol.WireGuardUdp.Ipv4);
        }
        if (!string.IsNullOrWhiteSpace(entryPerProtocol?.WireGuardTcp?.Ipv4))
        {
            relayIpByProtocol.Add(VpnProtocolIpcEntity.WireGuardTcp, entryPerProtocol.WireGuardTcp.Ipv4);
            relayIpByProtocol.Add(VpnProtocolIpcEntity.ProTunTcp, entryPerProtocol.WireGuardTcp.Ipv4);
        }
        if (!string.IsNullOrWhiteSpace(entryPerProtocol?.WireGuardTls?.Ipv4))
        {
            relayIpByProtocol.Add(VpnProtocolIpcEntity.WireGuardTls, entryPerProtocol.WireGuardTls.Ipv4);
            relayIpByProtocol.Add(VpnProtocolIpcEntity.ProTunTls, entryPerProtocol.WireGuardTls.Ipv4);
        }
        if (!string.IsNullOrWhiteSpace(entryPerProtocol?.OpenVpnUdp?.Ipv4))
        {
            relayIpByProtocol.Add(VpnProtocolIpcEntity.OpenVpnUdp, entryPerProtocol.OpenVpnUdp.Ipv4);
        }
        if (!string.IsNullOrWhiteSpace(entryPerProtocol?.OpenVpnTcp?.Ipv4))
        {
            relayIpByProtocol.Add(VpnProtocolIpcEntity.OpenVpnTcp, entryPerProtocol.OpenVpnTcp.Ipv4);
        }

        return relayIpByProtocol;
    }

    public GuestHoleServerContract Map(VpnServerIpcEntity rightEntity)
    {
        return rightEntity is null
            ? null
            : new GuestHoleServerContract()
            {
                Host = rightEntity.Name,
                Ip = rightEntity.Ip,
                Label = rightEntity.Label,
                Signature = rightEntity.Signature,
                X25519PublicKey = null,
            };
    }
}