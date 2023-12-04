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

using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Client.Logic.Servers.Contracts;
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
        List<PhysicalServer> physicalServers = _entityMapper.Map<PhysicalServerResponse, PhysicalServer>(leftEntity.Servers);

        return new Server
        {
            Id = leftEntity.Id,
            Name = leftEntity.Name,
            City = leftEntity.City,
            EntryCountry = leftEntity.EntryCountry,
            ExitCountry = leftEntity.ExitCountry,
            Domain = leftEntity.Domain,
            ExitIp = ExitIp(physicalServers),
            Status = leftEntity.Status,
            Tier = leftEntity.Tier,
            Features = (ServerFeatures)leftEntity.Features,
            Load = leftEntity.Load,
            Score = leftEntity.Score,
            Servers = physicalServers,
            IsVirtual = !string.IsNullOrEmpty(leftEntity.HostCountry),
            IsUnderMaintenance = leftEntity.Status == 0,
            GatewayName = leftEntity.GatewayName,
        };
    }

    public LogicalServerResponse Map(Server rightEntity)
    {
        throw new NotImplementedException();
    }

    private string ExitIp(IEnumerable<PhysicalServer> servers)
    {
        return servers.Aggregate(
            (string)null,
            (ip, p) => ip == null || ip == p.ExitIp ? p.ExitIp : "",
            ip => !string.IsNullOrEmpty(ip) ? ip : null);
    }
}