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
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Servers.Mappers;

public class LogicalServerMapper : IMapper<LogicalServerResponse, Server>
{
    private readonly IEntityMapper _entityMapper;

    public LogicalServerMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public Server Map(LogicalServerResponse leftEntity)
    {
        return leftEntity is null
            ? null
            : new Server
            {
                Id = leftEntity.Id,
                Name = leftEntity.Name,
                City = leftEntity.City,
                State = leftEntity.State,
                EntryCountry = leftEntity.EntryCountry,
                ExitCountry = leftEntity.ExitCountry,
                HostCountry = leftEntity.HostCountry,
                Domain = leftEntity.Domain,
                Latitude = leftEntity.Location?.Lat ?? 0,
                Longitude = leftEntity.Location?.Long ?? 0,
                Status = leftEntity.Status,
                Tier = (ServerTiers)leftEntity.Tier,
                Features = (ServerFeatures)leftEntity.Features,
                Load = leftEntity.Load,
                Score = leftEntity.Score,
                Servers = _entityMapper.Map<PhysicalServerResponse, PhysicalServer>(leftEntity.Servers),
                IsVirtual = !string.IsNullOrEmpty(leftEntity.HostCountry),
                GatewayName = leftEntity.GatewayName,
            };
    }

    public LogicalServerResponse Map(Server rightEntity)
    {
        throw new NotImplementedException("We don't need to map to API responses.");
    }
}