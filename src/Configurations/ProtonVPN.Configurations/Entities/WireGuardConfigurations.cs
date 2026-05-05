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

using ProtonVPN.Configurations.Contracts.Entities;

namespace ProtonVPN.Configurations.Entities;

public class WireGuardConfigurations : IWireGuardConfigurations
{
    public required string ServiceName { get; init; }
    public required string ConfigFileName { get; init; }

    public required string WintunAdapterHardwareId { get; init; }
    public required Guid WintunAdapterGuid { get; init; }
    public required Guid NtAdapterGuid { get; init; }

    public required string DefaultServerGatewayIpv4Address { get; init; }
    public required string DefaultClientIpv4Address { get; init; }

    public required string DefaultServerGatewayIpv6Address { get; init; }
    public required string DefaultClientIpv6Address { get; init; }

    public required string ConfigFilePath { get; init; }
    public required string ServicePath { get; init; }
    public required string LogFilePath { get; init; }
    public required string PipeName { get; init; }
}