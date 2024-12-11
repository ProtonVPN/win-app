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
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

public class ConnectionRequestMapper : IMapper<VpnConnectionRequest, ConnectionRequestIpcEntity>
{
    private readonly IEntityMapper _entityMapper;

    public ConnectionRequestMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public ConnectionRequestIpcEntity Map(VpnConnectionRequest leftEntity)
    {
        return leftEntity is null
            ? null
            : new()
            {
                Servers = _entityMapper.Map<VpnHost, VpnServerIpcEntity>(leftEntity.Servers).ToArray(),
                Protocol = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(leftEntity.VpnProtocol),
                Config = _entityMapper.Map<VpnConfig, VpnConfigIpcEntity>(leftEntity.Config),
                Credentials = _entityMapper.Map<VpnCredentials, VpnCredentialsIpcEntity>(leftEntity.Credentials)
            };
    }

    public VpnConnectionRequest Map(ConnectionRequestIpcEntity rightEntity)
    {
        return rightEntity is null
            ? null
            : new(
            servers: _entityMapper.Map<VpnServerIpcEntity, VpnHost>(rightEntity.Servers),
            vpnProtocol: _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(rightEntity.Protocol),
            config: _entityMapper.Map<VpnConfigIpcEntity, VpnConfig>(rightEntity.Config),
            credentials: _entityMapper.Map<VpnCredentialsIpcEntity, VpnCredentials>(rightEntity.Credentials));
    }
}