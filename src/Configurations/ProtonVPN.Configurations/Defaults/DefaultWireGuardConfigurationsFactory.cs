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

using ProtonVPN.Configurations.Contracts.Entities;
using ProtonVPN.Configurations.Entities;

namespace ProtonVPN.Configurations.Defaults;

public static class DefaultWireGuardConfigurationsFactory
{
    private const string WIREGUARD_CONFIG_FILENAME = "ProtonVPN";

    public static IWireGuardConfigurations Create(string baseDirectory, string commonAppDataProtonVpnPath)
    {
        return new WireGuardConfigurations
        {
            ServiceName = "ProtonVPN WireGuard",
            ConfigFileName = WIREGUARD_CONFIG_FILENAME,

            WintunAdapterHardwareId = "Wintun",
            WintunAdapterGuid = Guid.Parse("{AC128890-BDB1-CE5C-D1DB-EFB01DE370B2}"),
            NtAdapterGuid = Guid.Parse("{EAB2262D-9AB1-5975-7D92-334D06F4972B}"),

            DefaultServerGatewayIpv4Address = "10.2.0.1",
            DefaultClientIpv4Address = "10.2.0.2",

            DefaultServerGatewayIpv6Address = "2a07:b944::2:1",
            DefaultClientIpv6Address = "2a07:b944::2:2",

            ConfigFilePath = Path.Combine(commonAppDataProtonVpnPath, "WireGuard", $"{WIREGUARD_CONFIG_FILENAME}.conf"),
            ServicePath = Path.Combine(baseDirectory, "ProtonVPN.WireGuardService.exe"),
            LogFilePath = Path.Combine(commonAppDataProtonVpnPath, "WireGuard", "log.bin"),
            PipeName = $"ProtectedPrefix\\Administrators\\WireGuard\\{WIREGUARD_CONFIG_FILENAME}",
        };
    }
}