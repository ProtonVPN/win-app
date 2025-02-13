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

public class VpnStateChangedEventArgsMapper : IMapper<VpnStateChangedEventArgs, VpnStateIpcEntity>
{
    private readonly IEntityMapper _entityMapper;

    public VpnStateChangedEventArgsMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public VpnStateIpcEntity Map(VpnStateChangedEventArgs leftEntity)
    {
        return new VpnStateIpcEntity()
        {
            Status = _entityMapper.Map<VpnStatus, VpnStatusIpcEntity>(leftEntity.State.Status),
            Error = _entityMapper.Map<VpnError, VpnErrorTypeIpcEntity>(leftEntity.Error),
            NetworkBlocked = leftEntity.NetworkBlocked,
            EndpointIp = leftEntity.State.RemoteIp,
            EndpointPort = leftEntity.State.EndpointPort,
            OpenVpnAdapterType = _entityMapper.MapNullableStruct<OpenVpnAdapter, OpenVpnAdapterIpcEntity>(leftEntity.State.OpenVpnAdapter),
            VpnProtocol = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(leftEntity.State.VpnProtocol),
            Label = leftEntity.State.Label
        };
    }

    public VpnStateChangedEventArgs Map(VpnStateIpcEntity rightEntity)
    {
        if (rightEntity is null)
        {
            throw new ArgumentNullException(nameof(VpnStateIpcEntity),
                $"The {nameof(VpnStateIpcEntity)} parameter cannot be mapped from null to {nameof(VpnStateChangedEventArgs)}.");
        }
        return new VpnStateChangedEventArgs(
            status: _entityMapper.Map<VpnStatusIpcEntity, VpnStatus>(rightEntity.Status),
            error: _entityMapper.Map<VpnErrorTypeIpcEntity, VpnError>(rightEntity.Error),
            endpointIp: rightEntity.EndpointIp,
            endpointPort: rightEntity.EndpointPort,
            networkBlocked: rightEntity.NetworkBlocked,
            vpnProtocol: _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(rightEntity.VpnProtocol),
            networkAdapterType: _entityMapper.MapNullableStruct<OpenVpnAdapterIpcEntity, OpenVpnAdapter>(rightEntity.OpenVpnAdapterType),
            label: rightEntity.Label);
    }
}