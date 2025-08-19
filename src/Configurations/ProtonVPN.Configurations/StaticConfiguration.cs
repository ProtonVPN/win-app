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
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Contracts.Entities;
using ProtonVPN.Configurations.Defaults;

namespace ProtonVPN.Configurations;

public class StaticConfiguration : IStaticConfiguration
{
    public string ClientName { get; } = DefaultConfiguration.ClientName;
    public string ServiceName { get; } = DefaultConfiguration.ServiceName;
    public string CalloutServiceName { get; } = DefaultConfiguration.CalloutServiceName;
    public string BaseFilteringEngineServiceName { get; } = DefaultConfiguration.BaseFilteringEngineServiceName;

    public string ClientLauncherExePath { get; } = DefaultConfiguration.ClientLauncherExePath;
    public string InstallActionsPath { get; } = DefaultConfiguration.InstallActionsPath;
    public string ClientExePath { get; } = DefaultConfiguration.ClientExePath;
    public string ServiceExePath { get; } = DefaultConfiguration.ServiceExePath;

    public string ProtocolActivationScheme { get; } = DefaultConfiguration.ProtocolActivationScheme;
    public string LegacyProtocolActivationScheme { get; } = DefaultConfiguration.LegacyProtocolActivationScheme;

    public string StorageFolder { get; } = DefaultConfiguration.StorageFolder;
    public string ClientLogsFolder { get; } = DefaultConfiguration.ClientLogsFolder;
    public string ServiceLogsFolder { get; } = DefaultConfiguration.ServiceLogsFolder;
    public string DiagnosticLogsFolder { get; } = DefaultConfiguration.DiagnosticLogsFolder;
    public string ImageCacheFolder { get; } = DefaultConfiguration.ImageCacheFolder;
    public string UpdatesFolder { get; } = DefaultConfiguration.UpdatesFolder;
    public string WebViewFolder { get; } = DefaultConfiguration.WebViewFolder;
    public string AssetsFolder { get; } = DefaultConfiguration.AssetsFolder;

    public string ClientLogsFilePath { get; } = DefaultConfiguration.ClientLogsFilePath;
    public string ServiceLogsFilePath { get; } = DefaultConfiguration.ServiceLogsFilePath;
    public string InstallLogsFilePath { get; } = DefaultConfiguration.InstallLogsFilePath;
    public string DiagnosticLogsZipFilePath { get; } = DefaultConfiguration.DiagnosticLogsZipFilePath;
    public string GuestHoleServersJsonFilePath { get; } = DefaultConfiguration.GuestHoleServersJsonFilePath;
    public string ServiceSettingsFilePath { get; } = DefaultConfiguration.ServiceSettingsFilePath;
    public string LegacyAppLocalData { get; } = DefaultConfiguration.LegacyAppLocalData;
    public string IPv6PrefixTreeFilePath { get; } = DefaultConfiguration.IPv6PrefixTreeFilePath;
    public string IPv6PersistedDataFilePath { get; } = DefaultConfiguration.IPv6PersistedDataFilePath;

    public IOpenVpnConfigurations OpenVpn { get; } = DefaultConfiguration.OpenVpn;
    public IWireGuardConfigurations WireGuard { get; } = DefaultConfiguration.WireGuard;

    public string WintunDriverPath { get; } = DefaultConfiguration.WintunDriverPath;
    public string WintunAdapterName { get; } = DefaultConfiguration.WintunAdapterName;

    public string GetHardwareId(OpenVpnAdapter openVpnAdapter)
    {
        return openVpnAdapter switch
        {
            OpenVpnAdapter.Tap => OpenVpn.TapAdapterId,
            OpenVpnAdapter.Tun => OpenVpn.TunAdapterId,
            _ => WireGuard.WintunAdapterHardwareId
        };
    }
}