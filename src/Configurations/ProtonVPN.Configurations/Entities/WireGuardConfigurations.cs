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
    public string ServiceName { get; set; } = string.Empty;
    public string ConfigFileName { get; set; } = string.Empty;

    public string WintunAdapterHardwareId { get; set; } = string.Empty;
    public Guid WintunAdapterGuid { get; set; }
    public Guid NtAdapterGuid { get; set; }

    public string DefaultDnsServer { get; set; } = string.Empty;
    public string DefaultClientAddress { get; set; } = string.Empty;

    public string ConfigFilePath { get; set; } = string.Empty;
    public string ServicePath { get; set; } = string.Empty;
    public string LogFilePath { get; set; } = string.Empty;
    public string PipeName { get; set; } = string.Empty;
}