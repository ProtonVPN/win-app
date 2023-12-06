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

namespace ProtonVPN.Client.Logic.Servers.Contracts;

public class Server
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string City { get; init; }
    public required string EntryCountry { get; init; }
    public required string ExitCountry { get; init; }
    public required string HostCountry { get; init; }
    public required string Domain { get; init; }
    public string? ExitIp { get; init; }
    public sbyte Status { get; init; }
    public int Tier { get; init; }
    public ServerFeatures Features { get; init; }
    public int Load { get; init; }
    public float Score { get; init; }
    public required IReadOnlyList<PhysicalServer> Servers { get; init; }
    public bool IsVirtual { get; init; }
    public bool IsUnderMaintenance { get; init; }
    public required string GatewayName { get; init; }
}