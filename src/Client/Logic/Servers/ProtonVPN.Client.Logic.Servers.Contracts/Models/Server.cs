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

using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Logic.Servers.Contracts.Models;

public class Server : ILocation
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string EntryCountry { get; init; }
    public required string ExitCountry { get; init; }
    public required string HostCountry { get; init; }
    public required string Domain { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public string? ExitIp { get; init; }
    public sbyte Status { get; set; }
    public ServerTiers Tier { get; init; }
    public ServerFeatures Features { get; init; }
    public int Load { get; set; }
    public float Score { get; set; }
    public required IReadOnlyList<PhysicalServer> Servers { get; init; }
    public bool IsVirtual { get; init; }
    public required string GatewayName { get; init; }

    public bool IsUnderMaintenance() => Status == 0;

    public bool IsAvailable(IList<VpnProtocol> preferredProtocols) => !IsUnderMaintenance() &&
                                                                     Servers is not null &&
                                                                     Servers.Any(s => s.IsAvailable(preferredProtocols));

    public bool IsStandard() => Features.IsStandard();

    public bool IsPaidNonB2B() => IsPaid() && IsNonB2B();
    public bool IsFreeNonB2B() => IsFree() && IsNonB2B();

    public bool IsNonB2B() => !Features.IsB2B();

    public bool IsPaid() => Tier is ServerTiers.Basic or ServerTiers.Plus;
    public bool IsFree() => Tier is ServerTiers.Free;

    public Server CopyWithoutPhysicalServers()
    {
        return new()
        {
            Id = Id,
            Name = Name,
            City = City,
            State = State,
            EntryCountry = EntryCountry,
            ExitCountry = ExitCountry,
            HostCountry = HostCountry,
            Domain = Domain,
            Latitude = Latitude,
            Longitude = Longitude,
            ExitIp = ExitIp,
            Status = Status,
            Tier = Tier,
            Features = Features,
            Load = Load,
            Score = Score,
            Servers = new List<PhysicalServer>(),
            IsVirtual = IsVirtual,
            GatewayName = GatewayName,
        };
    }
}