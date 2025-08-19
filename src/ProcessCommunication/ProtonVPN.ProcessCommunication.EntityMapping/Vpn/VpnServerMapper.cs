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

using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

public class VpnServerMapper : IMapper<VpnHost, VpnServerIpcEntity>
{
    private readonly IEntityMapper _entityMapper;

    public VpnServerMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public VpnServerIpcEntity Map(VpnHost leftEntity)
    {
        return new()
        {
            Name = leftEntity.Name,
            Ip = leftEntity.Ip,
            Label = leftEntity.Label,
            X25519PublicKey = _entityMapper.Map<PublicKey, ServerPublicKeyIpcEntity>(leftEntity.X25519PublicKey),
            Signature = leftEntity.Signature,
            IsIpv6Supported = leftEntity.IsIpv6Supported,
            RelayIpByProtocol = leftEntity.RelayIpByProtocol?.ToDictionary(
                kvp => _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(kvp.Key),
                kvp => kvp.Value)
        };
    }

    public VpnHost Map(VpnServerIpcEntity rightEntity)
    {
        if (rightEntity is null)
        {
            throw new ArgumentNullException(nameof(VpnServerIpcEntity), 
                $"The {nameof(VpnServerIpcEntity)} parameter cannot be mapped from null to {nameof(VpnHost)}.");
        }

        Dictionary<VpnProtocol, string> relayIpByProtocol = rightEntity.RelayIpByProtocol?.ToDictionary(
            kvp => _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(kvp.Key),
            kvp => kvp.Value);

        return new(rightEntity.Name, rightEntity.Ip, rightEntity.Label,
            _entityMapper.Map<ServerPublicKeyIpcEntity, PublicKey>(rightEntity.X25519PublicKey),
            rightEntity.Signature, rightEntity.IsIpv6Supported, relayIpByProtocol);
    }
}