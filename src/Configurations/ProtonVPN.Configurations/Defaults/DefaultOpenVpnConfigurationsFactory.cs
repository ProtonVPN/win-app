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

using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Configurations.Contracts.Entities;
using ProtonVPN.Configurations.Entities;

namespace ProtonVPN.Configurations.Defaults;

public static class DefaultOpenVpnConfigurationsFactory
{
    public static IOpenVpnConfigurations Create(string baseFolder, string resourcesFolderPath, string commonAppDataProtonVpnPath)
    {
        return new OpenVpnConfigurations()
        {
            ConfigPath = Path.Combine(resourcesFolderPath, "config.ovpn"),

            TapAdapterId = "tapprotonvpn",
            TapAdapterDescription = "TAP-ProtonVPN Windows Adapter V9",
            TapInstallerDir = Path.Combine(resourcesFolderPath, "tap"),

            TunAdapterId = "wintun",
            TunAdapterName = "ProtonVPN TUN",

            TlsExportCertFolder = Path.Combine(commonAppDataProtonVpnPath, "ExportCert"),
            ExePath = Path.Combine(resourcesFolderPath, "openvpn.exe"),
            TlsVerifyExePath = Path.Combine(baseFolder, "ProtonVPN.TlsVerify.exe"),

            ManagementHost = "127.0.0.1",
            ExitEventName = "ProtonVPN-Exit-Event",
            StaticKey =
                ("6acef03f62675b4b1bbd03e53b187727423cea742242106cb2916a8a4c829756" +
                 "3d22c7e5cef430b1103c6f66eb1fc5b375a672f158e2e2e936c3faa48b035a6d" +
                 "e17beaac23b5f03b10b868d53d03521d8ba115059da777a60cbfd7b2c9c57472" +
                 "78a15b8f6e68a3ef7fd583ec9f398c8bd4735dab40cbd1e3c62a822e97489186" +
                 "c30a0b48c7c38ea32ceb056d3fa5a710e10ccc7a0ddb363b08c3d2777a3395e1" +
                 "0c0b6080f56309192ab5aacd4b45f55da61fc77af39bd81a19218a79762c3386" +
                 "2df55785075f37d8c71dc8a42097ee43344739a0dd48d03025b0450cf1fb5e8c" +
                 "aeb893d9a96d1f15519bb3c4dcb40ee316672ea16c012664f8a9f11255518deb"
                ).HexStringToByteArray().Skip(192).Take(64).ToArray(),
        };
    }
}