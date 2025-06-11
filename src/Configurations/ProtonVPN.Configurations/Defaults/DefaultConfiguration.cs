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

using System.Reflection;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.Configurations.Contracts.Entities;

namespace ProtonVPN.Configurations.Defaults;

public static class DefaultConfiguration
{
    // Constants
    private const string LOGS_FOLDER_NAME = "Logs";

    // Auxiliary fields
    /// <returns>C:\Program Files\Proton\VPN\v4.0.0</returns>
    private static readonly Lazy<string> _baseVersionDirectory = new(() =>
    {
        string? location = Assembly.GetEntryAssembly()?.Location;
        return (location is null ? null : new FileInfo(location).DirectoryName) ?? AppDomain.CurrentDomain.BaseDirectory;
    });

    /// <returns>C:\Program Files\Proton\VPN</returns>
    private static readonly Lazy<string> _baseDirectory = new(() => Path.GetDirectoryName(_baseVersionDirectory.Value) ?? string.Empty);

    /// <returns>C:\Program Files\Proton\VPN\v4.0.0\Resources</returns>
    private static readonly Lazy<string> _resourcesFolderPath = new(() => Path.Combine(_baseVersionDirectory.Value, "Resources"));

    /// <returns>C:\Program Files\Proton\VPN\v4.0.0\ServiceData</returns>
    private static readonly Lazy<string> _serviceDataPath = new(() => Path.Combine(_baseVersionDirectory.Value, "ServiceData"));

    /// <returns>C:\Program Files\Proton\VPN\v4.0.0\ServiceData\Logs</returns>
    private static readonly Lazy<string> _serviceLogsFolder = new(() => Path.Combine(_serviceDataPath.Value, LOGS_FOLDER_NAME));

    /// <returns>C:\Users\{user}\AppData\Local</returns>
    private static readonly Lazy<string> _localAppDataPath = new(() => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

    /// <returns>C:\Users\{user}\AppData\Local\Proton\Proton VPN</returns>
    private static readonly Lazy<string> _localAppDataProtonVpnPath = new(() => Path.Combine(_localAppDataPath.Value, "Proton", "Proton VPN"));

    /// <returns>C:\Users\{user}\AppData\Local\Proton\Proton VPN\Storage</returns>
    private static readonly Lazy<string> _storageFolder = new(() => Path.Combine(_localAppDataProtonVpnPath.Value, "Storage"));

    /// <returns>C:\Users\{user}\AppData\Local\Proton\Proton VPN\Logs</returns>
    private static readonly Lazy<string> _clientLogsFolder = new(() => Path.Combine(_localAppDataProtonVpnPath.Value, LOGS_FOLDER_NAME));

    /// <returns>C:\Users\{user}\AppData\Local\Proton\Proton VPN\DiagnosticLogs</returns>
    private static readonly Lazy<string> _diagnosticLogsFolder = new(() => Path.Combine(_localAppDataProtonVpnPath.Value, "DiagnosticLogs"));

    // Properties
    public static string ClientVersion => AssemblyVersion.Get();
    public static string ApiClientId => "windows-vpn";
    public static string UserAgent => "ProtonVPN";
    public static string ApiVersion => "3";
    public static string ClientName => "ProtonVPN.Client";
    public static string ServiceName => "ProtonVPN Service";
    public static string CalloutServiceName => "ProtonVPNCallout";
    public static string BaseFilteringEngineServiceName => "BFE";

    /// <returns>C:\Users\{user}\AppData\Local\Proton\Proton VPN</returns>
    public static string LocalAppDataProtonVpnPath => _localAppDataProtonVpnPath.Value;

    /// <returns>C:\Program Files\Proton\VPN\ProtonVPN.Launcher.exe</returns>
    public static string ClientLauncherExePath => Path.Combine(_baseDirectory.Value, "ProtonVPN.Launcher.exe");

    /// <returns>C:\Program Files\Proton\VPN\v4.0.0\Resources\ProtonVPN.InstallActions.dll</returns>
    public static string InstallActionsPath => Path.Combine(_baseVersionDirectory.Value, "ProtonVPN.InstallActions.dll");

    /// <returns>C:\Program Files\Proton\VPN\v4.0.0\ProtonVPN.Client.exe</returns>
    public static string ClientExePath => Path.Combine(_baseVersionDirectory.Value, "ProtonVPN.Client.exe");

    /// <returns>C:\Program Files\Proton\VPN\v4.0.0\ProtonVPNService.exe</returns>
    public static string ServiceExePath => Path.Combine(_baseVersionDirectory.Value, "ProtonVPNService.exe");

    public static string ProtocolActivationScheme = "proton-vpn";

    public static string LegacyProtocolActivationScheme = "protonvpn";

    /// <returns>C:\Users\{user}\AppData\Local\Proton\Proton VPN\Storage</returns>
    public static string StorageFolder => _storageFolder.Value;

    /// <returns>C:\Users\{user}\AppData\Local\Proton\Proton VPN\Logs</returns>
    public static string ClientLogsFolder => _clientLogsFolder.Value;

    /// <returns>C:\Program Files\Proton\VPN\v4.0.0\ServiceData\Logs</returns>
    public static string ServiceLogsFolder => _serviceLogsFolder.Value;

    /// <returns>C:\Users\{user}\AppData\Local\Proton\Proton VPN\DiagnosticLogs</returns>
    public static string DiagnosticLogsFolder => _diagnosticLogsFolder.Value;

    /// <returns>C:\Users\{user}\AppData\Local\Proton\Proton VPN\Images</returns>
    public static string ImageCacheFolder => Path.Combine(_localAppDataProtonVpnPath.Value, "Images");

    /// <returns>C:\Program Files\Proton\VPN\v4.0.0\ServiceData\Updates</returns>
    public static string UpdatesFolder => Path.Combine(_serviceDataPath.Value, "Updates");

    /// <returns>C:\Users\{user}\AppData\Local\Proton\Proton VPN\WebView2</returns>
    public static string WebViewFolder => Path.Combine(_localAppDataProtonVpnPath.Value, "WebView2");

    /// <returns>C:\Program Files\Proton\VPN\v4.0.0\ProtonVPN.Client.Common.UI\Assets</returns>
    public static string AssetsFolder => Path.Combine(_baseVersionDirectory.Value, "ProtonVPN.Client.Common.UI", "Assets");

    /// <returns>C:\Users\{user}\AppData\Local\Proton\Proton VPN\Logs\client-logs.txt</returns>
    public static string ClientLogsFilePath => Path.Combine(ClientLogsFolder, "client-logs.txt");

    /// <returns>C:\Program Files\Proton\VPN\v4.0.0\ServiceData\Logs\service-logs.txt</returns>
    public static string ServiceLogsFilePath => Path.Combine(ServiceLogsFolder, "service-logs.txt");

    /// <returns>C:\Program Files\Proton\VPN\Install.log.txt</returns>
    public static string InstallLogsFilePath => Path.Combine(_baseDirectory.Value, "Install.log.txt");

    /// <returns>C:\Users\{user}\AppData\Local\Proton\Proton VPN\DiagnosticLogs\diagnostic_logs.zip</returns>
    public static string DiagnosticLogsZipFilePath => Path.Combine(DiagnosticLogsFolder, "diagnostic_logs.zip");

    /// <returns>C:\Program Files\Proton\VPN\v4.0.0\Resources\GuestHoleServers.json</returns>
    public static string GuestHoleServersJsonFilePath => Path.Combine(_resourcesFolderPath.Value, "GuestHoleServers.json");

    /// <returns>C:\Program Files\Proton\VPN\v4.0.0\ServiceData\ServiceSettings.json</returns>
    public static string ServiceSettingsFilePath => Path.Combine(_serviceDataPath.Value, "ServiceSettings.json");

    /// <returns>C:\Users\{user}\AppData\Local\ProtonVPN</returns>
    public static string LegacyAppLocalData => Path.Combine(_localAppDataPath.Value, "ProtonVPN");

    // C:\Program Files\Proton\VPN\v4.0.0\wintun.dll
    public static string WintunDriverPath => Path.Combine(_baseVersionDirectory.Value, "wintun.dll");

    public static string WintunAdapterName => "ProtonVPN TUN";

    public static string ServerValidationPublicKey => "MCowBQYDK2VwAyEANpYpt/FlSRwEuGLMoNAGOjy1BTyEJPJvKe00oln7LZk=";
    public static string VpnUsernameSuffix => "+pw"; // p - proton, w - windows
    public static string DoHVerifyApiHost => "verify-api.protonvpn.com";

    public static string NtpServerUrl => "time.windows.com";

    public static int MaximumProfileNameLength => 25;

    public static long BugReportingMaxFileSize => 488 * 1024;
    public static int MaxClientLogsAttached => 3;
    public static int MaxServiceLogsAttached => 3;
    public static int MaxDiagnosticLogsAttached => 4;

    public static int ApiRetries => 2;
    public static int MaxGuestHoleRetries => 5;

    public static decimal? DeviceRolloutProportion => null;

    public static bool IsCertificateValidationEnabled => true;

    public static TimeSpan ServiceCheckInterval => TimeSpan.FromSeconds(10);
    public static TimeSpan ClientConfigUpdateInterval => TimeSpan.FromHours(12);
    public static TimeSpan FeatureFlagsUpdateInterval => TimeSpan.FromHours(2);
    public static TimeSpan ConnectionCertificateUpdateInterval => TimeSpan.FromMinutes(5);
    public static TimeSpan ServerUpdateInterval => TimeSpan.FromHours(12);
    public static TimeSpan ServerLoadUpdateInterval => TimeSpan.FromMinutes(15);
    public static TimeSpan MinimumServerLoadUpdateInterval => TimeSpan.FromMinutes(15);
    public static TimeSpan AnnouncementsUpdateInterval => TimeSpan.FromMinutes(150);
    public static TimeSpan AlternativeRoutingCheckInterval => TimeSpan.FromMinutes(30);
    public static TimeSpan UpdateCheckInterval => TimeSpan.FromHours(3);
    public static TimeSpan ApiUploadTimeout => TimeSpan.FromSeconds(30);
    public static TimeSpan ApiTimeout => TimeSpan.FromSeconds(10);
    public static TimeSpan FailedDnsRequestTimeout => TimeSpan.FromSeconds(5);
    public static TimeSpan NewCacheTimeToLiveOnResolveError => TimeSpan.FromMinutes(10);
    public static TimeSpan DnsResolveTimeout => TimeSpan.FromSeconds(30);
    public static TimeSpan DefaultDnsTimeToLive => TimeSpan.FromMinutes(20);
    public static TimeSpan DnsOverHttpsPerProviderTimeout => TimeSpan.FromSeconds(20);
    public static TimeSpan DohClientTimeout => TimeSpan.FromSeconds(10);
    public static TimeSpan VpnStatePollingInterval => TimeSpan.FromSeconds(3);
    public static TimeSpan VpnPlanRequestInterval => TimeSpan.FromHours(12);
    public static TimeSpan VpnPlanMinimumRequestInterval => TimeSpan.FromMinutes(5);
    public static TimeSpan NetShieldStatisticRequestInterval => TimeSpan.FromSeconds(60);
    public static TimeSpan P2PTrafficDetectionInterval => TimeSpan.FromSeconds(60);
    public static TimeSpan StatisticalEventSendTriggerInterval => TimeSpan.FromMinutes(15);
    public static TimeSpan StatisticalEventMinimumWaitInterval => TimeSpan.FromMinutes(10);

    public static IOpenVpnConfigurations OpenVpn => DefaultOpenVpnConfigurationsFactory.Create(
        baseFolder: _baseVersionDirectory.Value,
        resourcesFolderPath: _resourcesFolderPath.Value, 
        commonAppDataProtonVpnPath: _serviceDataPath.Value);
    public static IWireGuardConfigurations WireGuard => DefaultWireGuardConfigurationsFactory.Create(
        baseDirectory: _baseVersionDirectory.Value,
        commonAppDataProtonVpnPath: _serviceDataPath.Value);

    public static IList<string> DohProviders => DefaultDohProvidersFactory.Create();
    public static IUrlsConfiguration Urls => DefaultUrlsConfigurationFactory.Create();
    public static ITlsPinningConfiguration TlsPinning => DefaultTlsPinningConfigurationFactory.Create();
}