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

using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

public class VpnConfigMapper : IMapper<VpnConfig, VpnConfigIpcEntity>
{
    private readonly IEntityMapper _entityMapper;

    public VpnConfigMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public VpnConfigIpcEntity Map(VpnConfig leftEntity)
    {
        if (leftEntity is null)
        {
            throw new ArgumentNullException(nameof(VpnConfig),
                $"The {nameof(VpnConfig)} parameter cannot be mapped from null to {nameof(VpnConfigIpcEntity)}.");
        }
        Dictionary<VpnProtocolIpcEntity, int[]> portConfig = leftEntity.Ports.ToDictionary(
            p => _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(p.Key),
            p => p.Value.ToArray());

        return new VpnConfigIpcEntity
        {
            Ports = portConfig,
            CustomDns = leftEntity.CustomDns.ToList(),
            SplitTunnelMode = _entityMapper.Map<SplitTunnelMode, SplitTunnelModeIpcEntity>(leftEntity.SplitTunnelMode),
            SplitTunnelIPs = leftEntity.SplitTunnelIPs.ToList(),
            NetShieldMode = leftEntity.NetShieldMode,
            VpnProtocol = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(leftEntity.VpnProtocol),
            ModerateNat = leftEntity.ModerateNat,
            PreferredProtocols = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(leftEntity.PreferredProtocols),
            SplitTcp = leftEntity.SplitTcp,
            PortForwarding = leftEntity.PortForwarding,
            IsIpv6Enabled = leftEntity.IsIpv6Enabled,
            WireGuardConnectionTimeout = leftEntity.WireGuardConnectionTimeout,
        };
    }

    public VpnConfig Map(VpnConfigIpcEntity rightEntity)
    {
        if (rightEntity is null)
        {
            throw new ArgumentNullException(nameof(VpnConfigIpcEntity),
                $"The {nameof(VpnConfigIpcEntity)} parameter cannot be mapped from null to {nameof(VpnConfig)}.");
        }
        Dictionary<VpnProtocol, IReadOnlyCollection<int>> portConfig = rightEntity.Ports.ToDictionary(
            p => _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(p.Key),
            p => (IReadOnlyCollection<int>)p.Value.ToList());

        return new VpnConfig(
            new VpnConfigParameters
            {
                Ports = portConfig,
                CustomDns = rightEntity.CustomDns,
                SplitTunnelMode = _entityMapper.Map<SplitTunnelModeIpcEntity, SplitTunnelMode>(rightEntity.SplitTunnelMode),
                SplitTunnelIPs = rightEntity.SplitTunnelIPs,
                VpnProtocol = _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(rightEntity.VpnProtocol),
                PreferredProtocols = _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(rightEntity.PreferredProtocols),
                ModerateNat = rightEntity.ModerateNat,
                NetShieldMode = rightEntity.NetShieldMode,
                SplitTcp = rightEntity.SplitTcp,
                PortForwarding = rightEntity.PortForwarding,
                IsIpv6Enabled = rightEntity.IsIpv6Enabled,
                WireGuardConnectionTimeout = rightEntity.WireGuardConnectionTimeout,
            });
    }
}