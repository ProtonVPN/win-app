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
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string EntryCountry { get; init; } = string.Empty;
    public string ExitCountry { get; init; } = string.Empty;
    public string HostCountry { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public string? ExitIp { get; init; }
    public sbyte Status { get; init; }
    public int Tier { get; init; }
    public ServerFeatures Features { get; init; }
    public int Load { get; init; }
    public float Score { get; init; }
    public IReadOnlyList<PhysicalServer> Servers { get; init; } = new List<PhysicalServer>();
    public bool IsVirtual { get; init; }
    public bool IsUnderMaintenance { get; init; }
    public string GatewayName { get; init; } = string.Empty;
}