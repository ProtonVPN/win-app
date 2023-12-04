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
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Configurations.Contracts.Entities;

namespace ProtonVPN.Configurations.Defaults;

public static class DefaultConfiguration
{
    // Constants
    private const string PROTON_FOLDER_RELATIVE_PATH = "Proton/Proton VPN";
    private const string LOGS_FOLDER_NAME = "Logs";

    // Auxiliary fields
    private static readonly Lazy<string> _baseDirectory = new(() =>
    {
        string? location = Assembly.GetEntryAssembly()?.Location;
        return (location is null ? null : new FileInfo(location).DirectoryName) ?? AppDomain.CurrentDomain.BaseDirectory;
    });
    private static readonly Lazy<string> _launcherDirectoryPath = new(() => Path.GetDirectoryName(_baseDirectory.Value) ?? string.Empty);
    private static readonly Lazy<string> _resourcesFolderPath = new(() => Path.Combine(_baseDirectory.Value, "Resources"));

    private static readonly Lazy<string> _localAppDataProtonVpnPath = new(() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PROTON_FOLDER_RELATIVE_PATH));
    private static readonly Lazy<string> _commonAppDataProtonVpnPath = new(() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), PROTON_FOLDER_RELATIVE_PATH));

    private static readonly Lazy<string> _clientLogsFolder = new(() => Path.Combine(_localAppDataProtonVpnPath.Value, LOGS_FOLDER_NAME));
    private static readonly Lazy<string> _serviceLogsFolder = new(() => Path.Combine(_commonAppDataProtonVpnPath.Value, LOGS_FOLDER_NAME));
    private static readonly Lazy<string> _diagnosticLogsFolder = new(() => Path.Combine(_localAppDataProtonVpnPath.Value, "DiagnosticLogs"));

    // Properties
    public static string ClientVersion => (Assembly.GetExecutingAssembly().GetName().Version ?? new()).Truncate().ToString();
    public static string ApiClientId => "windows-vpn";
    public static string UserAgent => "ProtonVPN";
    public static string ApiVersion => "3";
    public static string ClientName => "ProtonVPN.Client";
    public static string ServiceName => "ProtonVPN Service";
    public static string CalloutServiceName => "ProtonVPNCallout";

    public static string ClientLauncherExePath => Path.Combine(_launcherDirectoryPath.Value, "ProtonVPN.Launcher.exe");
    public static string InstallActionsPath => Path.Combine(_resourcesFolderPath.Value, "ProtonVPN.InstallActions.dll");
    public static string ClientExePath => Path.Combine(_baseDirectory.Value, "ProtonVPN.Client.exe");
    public static string ServiceExePath => Path.Combine(_baseDirectory.Value, "ProtonVPNService.exe");

    public static string ClientLogsFolder => _clientLogsFolder.Value;
    public static string ServiceLogsFolder => _serviceLogsFolder.Value;
    public static string DiagnosticLogsFolder => _diagnosticLogsFolder.Value;
    public static string ImageCacheFolder => Path.Combine(_localAppDataProtonVpnPath.Value, "Images");
    public static string UpdatesFolder => Path.Combine(_commonAppDataProtonVpnPath.Value, "Updates");
    public static string WebViewFolder => Path.Combine(_localAppDataProtonVpnPath.Value, "WebView2");

    public static string ClientLogsFilePath => Path.Combine(ClientLogsFolder, "client-logs.txt");
    public static string ServiceLogsFilePath => Path.Combine(ServiceLogsFolder, "service-logs.txt");
    public static string DiagnosticLogsZipFilePath => Path.Combine(DiagnosticLogsFolder, "diagnostic_logs.zip");
    public static string GuestHoleServersJsonFilePath => Path.Combine(_resourcesFolderPath.Value, "GuestHoleServers.json");
    public static string ServiceSettingsFilePath => Path.Combine(_commonAppDataProtonVpnPath.Value, "ServiceSettings.json");

    public static string ServersJsonCacheFilePath => Path.Combine(_localAppDataProtonVpnPath.Value, "Servers.json");

    public static string WintunDriverPath => Path.Combine(_resourcesFolderPath.Value, "wintun.dll");
    public static string WintunAdapterName => "ProtonVPN TUN";

    public static string ServerValidationPublicKey => "MCowBQYDK2VwAyEANpYpt/FlSRwEuGLMoNAGOjy1BTyEJPJvKe00oln7LZk=";

    public static string GuestHoleVpnUsername => "guest";
    public static string GuestHoleVpnPassword => "guest";
    public static string VpnUsernameSuffix => "+pw"; // p - proton, w - windows

    public static long BugReportingMaxFileSize => 488 * 1024;
    public static int MaxClientLogsAttached => 3;
    public static int MaxServiceLogsAttached => 3;
    public static int MaxDiagnosticLogsAttached => 4;

    public static int ApiRetries => 2;
    public static int MaxGuestHoleRetries => 5;

    public static bool IsCertificateValidationEnabled => true;

    public static TimeSpan ServiceCheckInterval => TimeSpan.FromSeconds(30);
    public static TimeSpan ClientConfigUpdateInterval => TimeSpan.FromHours(12);
    public static TimeSpan FeatureFlagsUpdateInterval => TimeSpan.FromHours(2);
    public static TimeSpan AuthCertificateUpdateInterval => TimeSpan.FromMinutes(5);
    public static TimeSpan ServerUpdateInterval => TimeSpan.FromHours(3);
    public static TimeSpan AnnouncementUpdateInterval => TimeSpan.FromMinutes(150);
    public static TimeSpan AlternativeRoutingCheckInterval => TimeSpan.FromMinutes(30);
    public static TimeSpan ApiUploadTimeout => TimeSpan.FromSeconds(30);
    public static TimeSpan ApiTimeout => TimeSpan.FromSeconds(10);
    public static TimeSpan FailedDnsRequestTimeout => TimeSpan.FromSeconds(5);
    public static TimeSpan NewCacheTimeToLiveOnResolveError => TimeSpan.FromMinutes(10);
    public static TimeSpan DnsResolveTimeout => TimeSpan.FromSeconds(30);
    public static TimeSpan DefaultDnsTimeToLive => TimeSpan.FromMinutes(20);
    public static TimeSpan DnsOverHttpsPerProviderTimeout => TimeSpan.FromSeconds(20);
    public static TimeSpan DohClientTimeout => TimeSpan.FromSeconds(10);

    public static IOpenVpnConfigurations OpenVpn => DefaultOpenVpnConfigurationsFactory.Create(
        baseFolder: _baseDirectory.Value,
        resourcesFolderPath: _resourcesFolderPath.Value, 
        commonAppDataProtonVpnPath: _commonAppDataProtonVpnPath.Value);
    public static IWireGuardConfigurations WireGuard => DefaultWireGuardConfigurationsFactory.Create(
        baseDirectory: _baseDirectory.Value,
        commonAppDataProtonVpnPath: _commonAppDataProtonVpnPath.Value);

    public static IList<string> DohProviders => DefaultDohProvidersFactory.Create();
    public static IUrlsConfiguration Urls => DefaultUrlsConfigurationFactory.Create();
    public static ITlsPinningConfiguration TlsPinning => DefaultTlsPinningConfigurationFactory.Create();
}