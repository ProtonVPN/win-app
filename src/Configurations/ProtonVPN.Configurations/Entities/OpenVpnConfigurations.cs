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

public class OpenVpnConfigurations : IOpenVpnConfigurations
{
    public required string ConfigPath { get; init; }

    public required string TapAdapterId { get; init; }
    public required string TapAdapterDescription { get; init; }
    public required string TapInstallerDir { get; init; }

    public required string TunAdapterId { get; init; }
    public required string TunAdapterName { get; init; }

    public required string TlsExportCertFolder { get; init; }
    public required string ExePath { get; init; }
    public required string TlsVerifyExePath { get; init; }

    public required string ManagementHost { get; init; }
    public required string ExitEventName { get; init; }
    public required byte[] StaticKey { get; init; }
}