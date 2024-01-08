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

using ProtonVPN.Common.Legacy.PortForwarding;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;

namespace ProtonVPN.ProcessCommunication.EntityMapping.PortForwarding;

public class PortForwardingStateMapper : IMapper<PortForwardingState, PortForwardingStateIpcEntity>
{
    private readonly IEntityMapper _entityMapper;

    public PortForwardingStateMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public PortForwardingStateIpcEntity Map(PortForwardingState leftEntity)
    {
        return leftEntity is null
            ? null
            : new PortForwardingStateIpcEntity()
            {
                MappedPort = _entityMapper.Map<TemporaryMappedPort, TemporaryMappedPortIpcEntity>(leftEntity.MappedPort),
                Status = _entityMapper.Map<PortMappingStatus, PortMappingStatusIpcEntity>(leftEntity.Status),
                TimestampUtc = leftEntity.TimestampUtc,
            };
    }

    public PortForwardingState Map(PortForwardingStateIpcEntity rightEntity)
    {
        return rightEntity is null
            ? null
            : new PortForwardingState()
            {
                MappedPort = _entityMapper.Map<TemporaryMappedPortIpcEntity, TemporaryMappedPort>(rightEntity.MappedPort),
                Status = _entityMapper.Map<PortMappingStatusIpcEntity, PortMappingStatus>(rightEntity.Status),
                TimestampUtc = rightEntity.TimestampUtc,
            };
    }
}