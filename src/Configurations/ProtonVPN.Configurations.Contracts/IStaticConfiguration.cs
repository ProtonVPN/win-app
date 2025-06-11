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

using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts.Entities;

namespace ProtonVPN.Configurations.Contracts;

public interface IStaticConfiguration
{
    string ClientName { get; }
    string ServiceName { get; }
    string CalloutServiceName { get; }
    string BaseFilteringEngineServiceName { get; }

    string ClientLauncherExePath { get; }
    string InstallActionsPath { get; }
    string ClientExePath { get; }
    string ServiceExePath { get; }

    string ProtocolActivationScheme { get; }
    string LegacyProtocolActivationScheme { get; }

    string StorageFolder { get; }
    string ClientLogsFolder { get; }
    string ServiceLogsFolder { get; }
    string DiagnosticLogsFolder { get; }
    string ImageCacheFolder { get; }
    string UpdatesFolder { get; }
    string WebViewFolder { get; }
    string AssetsFolder { get; }

    string ClientLogsFilePath { get; }
    string ServiceLogsFilePath { get; }
    string InstallLogsFilePath { get; }
    string DiagnosticLogsZipFilePath { get; }
    string GuestHoleServersJsonFilePath { get; }
    string ServiceSettingsFilePath { get; }
    string LegacyAppLocalData { get; }

    IOpenVpnConfigurations OpenVpn { get; }
    IWireGuardConfigurations WireGuard { get; }

    string WintunDriverPath { get; }
    string WintunAdapterName { get; }

    string GetHardwareId(OpenVpnAdapter openVpnAdapter);
}