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

public class PortMappingStatusMapper : IMapper<PortMappingStatus, PortMappingStatusIpcEntity>
{
    public PortMappingStatusIpcEntity Map(PortMappingStatus leftEntity)
    {
        return (PortMappingStatusIpcEntity)leftEntity;
    }

    public PortMappingStatus Map(PortMappingStatusIpcEntity rightEntity)
    {
        return (PortMappingStatus)rightEntity;
    }
}