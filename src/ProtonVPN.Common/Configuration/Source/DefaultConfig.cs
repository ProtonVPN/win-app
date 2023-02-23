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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ProtonVPN.Common.Configuration.Api.Handlers.TlsPinning;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Common.Configuration.Source
{
    public class DefaultConfig : IConfigSource
    {
        public const int MAX_QUICK_CONNECT_SERVERS_ON_RECONNECTION = 50;
        public const string ALTERNATIVE_ROUTING_HOSTNAME = "*";

        public IConfiguration Value()
        {
            string location = Assembly.GetEntryAssembly()?.Location;
            string baseFolder = (location != null ? new FileInfo(location).DirectoryName : null)
                                ?? AppDomain.CurrentDomain.BaseDirectory;
            string resourcesFolder = Path.Combine(baseFolder, "Resources");

            string localAppDataFolder =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProtonVPN");
            string commonAppDataFolder =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ProtonVPN");
            string appLogFolder = Path.Combine(localAppDataFolder, "Logs");
            string serviceLogFolder = Path.Combine(commonAppDataFolder, "Logs");
            int osBits = Environment.Is64BitOperatingSystem ? 64 : 32;

            string wireGuardConfigFilename = "ProtonVPN";

            return new Config
            {
                OsBits = osBits,

                AppName = "ProtonVPN",

                AppVersion = Assembly.GetExecutingAssembly().GetName().Version.Normalized().ToString(),

                AppExePath = Path.Combine(baseFolder, "ProtonVPN.exe"),

                AppLogFolder = appLogFolder,

                AppLogDefaultFullFilePath = Path.Combine(appLogFolder, "app-logs.txt"),

                DiagnosticsLogFolder = Path.Combine(localAppDataFolder, "DiagnosticLogs"),

                DiagnosticsZipPath = Path.Combine(localAppDataFolder, "DiagnosticLogs", "diagnostic_logs.zip"),

                TranslationsFolder = baseFolder,

                ServiceName = "ProtonVPN Service",

                ServiceExePath = Path.Combine(baseFolder, "ProtonVPNService.exe"),

                ServiceSettingsFilePath = Path.Combine(commonAppDataFolder, "ServiceSettings.json"),

                ServersJsonCacheFilePath = Path.Combine(localAppDataFolder, "Servers.json"),

                GuestHoleServersJsonFilePath = Path.Combine(resourcesFolder, "GuestHoleServers.json"),

                StreamingServicesFilePath = Path.Combine(localAppDataFolder, "StreamingServices.json"),

                PartnersFilePath = Path.Combine(localAppDataFolder, "Partners.json"),

                ServiceLogFolder = serviceLogFolder,

                ServiceLogDefaultFullFilePath = Path.Combine(serviceLogFolder, "service-logs.txt"),

                UpdateFilePath = Path.Combine(localAppDataFolder, "Updates", "update.txt"),

                UpdatesPath = Path.Combine(localAppDataFolder, "Updates"),

                CalloutServiceName = "ProtonVPNCallout",

                LocalAppDataFolder = localAppDataFolder,

                ImageCacheFolder = Path.Combine(localAppDataFolder, "Images"),

                MaxDiagnosticLogsAttached = 4,

                MaxAppLogsAttached = 3,

                MaxServiceLogsAttached = 3,

                ApiClientId = "WindowsVPN",

                ApiVersion = "3",

                UserAgent = "ProtonVPN",

                ApiTimeout = TimeSpan.FromSeconds(10),

                ApiUploadTimeout = TimeSpan.FromSeconds(30),

                DohClientTimeout = TimeSpan.FromSeconds(10),

                ApiRetries = 2,

                MaxGuestHoleRetries = 5,

                GuestHoleVpnUsername = "guest",

                GuestHoleVpnPassword = "guest",

                VpnUsernameSuffix = "+pw", // p - proton, w - windows

                UpdateCheckInterval = TimeSpan.FromHours(3),

                UpdateRemindInterval = TimeSpan.FromHours(24),

                ServerUpdateInterval = TimeSpan.FromHours(3),

                StreamingServicesUpdateInterval = TimeSpan.FromHours(3),

                PartnersUpdateInterval = TimeSpan.FromHours(3),

                AnnouncementUpdateInterval = TimeSpan.FromMinutes(150),

                ServerLoadUpdateInterval = TimeSpan.FromMinutes(15),

                P2PCheckInterval = TimeSpan.FromSeconds(60),

                VpnInfoCheckInterval = TimeSpan.FromMinutes(3),

                ClientConfigUpdateInterval = TimeSpan.FromHours(3),

                DefaultCurrency = "USD",

                ReportBugMaxFiles = 10,

                ReportBugMaxFileSize = 488 * 1024,

                MaxProfileNameLength = 25,

                ForcedProfileSyncInterval = TimeSpan.FromMinutes(3),

                EventCheckInterval = TimeSpan.FromMinutes(5),

                ServiceCheckInterval = TimeSpan.FromSeconds(30),

                DefaultOpenVpnUdpPorts = new[] { 443, 1194, 4569, 5060, 80 },

                DefaultOpenVpnTcpPorts = new[] { 443, 3389, 8080, 8443 },

                DefaultWireGuardPorts = new[] { 51820, 88, 123, 49152, 1224 },

                DefaultLocale = "en-US",

                MaintenanceCheckInterval = TimeSpan.FromMinutes(30),

                AuthCertificateUpdateInterval = TimeSpan.FromMinutes(5),

                MaxQuickConnectServersOnReconnection = MAX_QUICK_CONNECT_SERVERS_ON_RECONNECTION,

                Urls =
                {
                    BfeArticleUrl = "https://protonvpn.com/support/how-to-enable-the-base-filtering-engine",
                    PasswordResetUrl = "https://account.protonvpn.com/reset-password",
                    ForgetUsernameUrl = "https://account.protonvpn.com/forgot-username",
                    OldUpdateUrl = "https://protonvpn.com/download/win-update.json",
                    UpdateUrl = "https://protonvpn.com/download/windows-releases.json",
                    DownloadUrl = "https://protonvpn.com/download",
                    ApiUrl = "https://api.protonvpn.ch",
                    TlsReportUrl = "https://reports.protonmail.ch/reports/tls",
                    HelpUrl = "https://www.protonvpn.com/support",
                    AccountUrl = "https://account.protonvpn.com/dashboard",
                    AboutSecureCoreUrl = "https://protonvpn.com/support/secure-core-vpn",
                    RegisterUrl = "https://account.protonvpn.com/signup",
                    TroubleShootingUrl = "https://protonvpn.com/support/windows-vpn-issues",
                    P2PStatusUrl = "http://protonstatus.com/vpn_status",
                    ProtonMailPricingUrl = "https://protonmail.com/pricing",
                    PublicWifiSafetyUrl = "https://protonvpn.com/blog/public-wifi-safety",
                    ProtonStatusUrl = "https://protonstatus.com",
                    TorBrowserUrl = "https://www.torproject.org",
                    ProtonTwitterUrl = "https://twitter.com/ProtonVPN",
                    SupportFormUrl = "https://protonvpn.com/support-form",
                    AlternativeRoutingUrl = "https://protonmail.com/blog/anti-censorship-alternative-routing",
                    AboutKillSwitchUrl = "https://protonvpn.com/support/what-is-kill-switch",
                    AboutNetShieldUrl = "https://protonvpn.com/support/netshield",
                    AboutPortForwardingUrl = "https://protonvpn.com/support/port-forwarding",
                    PortForwardingRisksUrl = "https://protonvpn.com/support/port-forwarding-risks",
                    AboutModerateNatUrl = "https://protonvpn.com/support/moderate-nat",
                    StreamingUrl = "https://protonvpn.com/support/streaming-guide/",
                    SmartRoutingUrl = "https://protonvpn.com/support/smart-routing",
                    P2PUrl = "https://protonvpn.com/support/bittorrent-vpn/",
                    TorUrl = "https://protonvpn.com/support/tor-vpn/",
                    InvoicesUrl = "https://account.protonvpn.com/payments#invoices",
                    AboutSmartProtocolUrl = "https://protonvpn.com/support/how-to-change-vpn-protocols",
                    IncorrectSystemTimeArticleUrl = "https://protonvpn.com/support/update-windows-clock",
                    AssignVpnConnectionsUrl = "https://protonvpn.com/support/assign-vpn-connection",
                    NonStandardPortsUrl = "https://protonvpn.com/support/non-standard-ports",
                    LoginProblemsUrl = "https://protonvpn.com/support/login-problems",
                    RebrandingUrl = "https://protonvpn.com/blog/updated-proton-vpn",
                    RpcServerProblemUrl = "https://protonvpn.com/support/rpc-server-unavailable",
                },

                OpenVpn =
                {
                    ExePath = Path.Combine(resourcesFolder, $"{osBits}-bit", "openvpn.exe"),

                    ConfigPath = Path.Combine(resourcesFolder, "config.ovpn"),

                    ManagementHost = "127.0.0.1",

                    ExitEventName = "ProtonVPN-Exit-Event",

                    OpenVpnStaticKey = ("6acef03f62675b4b1bbd03e53b187727423cea742242106cb2916a8a4c829756" +
                                        "3d22c7e5cef430b1103c6f66eb1fc5b375a672f158e2e2e936c3faa48b035a6d" +
                                        "e17beaac23b5f03b10b868d53d03521d8ba115059da777a60cbfd7b2c9c57472" +
                                        "78a15b8f6e68a3ef7fd583ec9f398c8bd4735dab40cbd1e3c62a822e97489186" +
                                        "c30a0b48c7c38ea32ceb056d3fa5a710e10ccc7a0ddb363b08c3d2777a3395e1" +
                                        "0c0b6080f56309192ab5aacd4b45f55da61fc77af39bd81a19218a79762c3386" +
                                        "2df55785075f37d8c71dc8a42097ee43344739a0dd48d03025b0450cf1fb5e8c" +
                                        "aeb893d9a96d1f15519bb3c4dcb40ee316672ea16c012664f8a9f11255518deb")
                        .HexStringToByteArray().Skip(192).Take(64).ToArray(),

                    TlsVerifyExePath = Path.Combine(baseFolder, "ProtonVPN.TlsVerify.exe"),

                    TlsExportCertFolder = Path.Combine(commonAppDataFolder, "ExportCert"),

                    TapAdapterId = "tapprotonvpn",

                    TapAdapterDescription = "TAP-ProtonVPN Windows Adapter V9",

                    TunAdapterId = "wintun",

                    TunAdapterName = "ProtonVPN TUN",
                },

                WireGuard =
                {
                    TunAdapterHardwareId = "Wintun",

                    TunAdapterGuid = "{EAB2262D-9AB1-5975-7D92-334D06F4972B}",

                    TunAdapterName = "ProtonVPN",

                    LogFilePath = Path.Combine(commonAppDataFolder, "WireGuard", "log.bin"),

                    ConfigFilePath = Path.Combine(commonAppDataFolder, "WireGuard", $"{wireGuardConfigFilename}.conf"),

                    ServiceName = "ProtonVPN WireGuard",

                    ServicePath = Path.Combine(baseFolder, "ProtonVPN.WireGuardService.exe"),

                    ConfigFileName = wireGuardConfigFilename,

                    DefaultDnsServer = "10.2.0.1",

                    DefaultClientAddress = "10.2.0.2",
                },

                TlsPinningConfig =
                {
                    PinnedDomains = new List<TlsPinnedDomain>
                    {
                        new()
                        {
                            Name = "api.protonvpn.ch",
                            PublicKeyHashes = new HashSet<string>
                            {
                                "IEwk65VSaxv3s1/88vF/rM8PauJoIun3rzVCX5mLS3M=",
                                "drtmcR2kFkM8qJClsuWgUzxgBkePfRCkRpqUesyDmeE=",
                                "YRGlaY0jyJ4Jw2/4M8FIftwbDIQfh8Sdro96CeEel54=",
                                "AfMENBVvOS8MnISprtvyPsjKlPooqh8nMB/pvCrpJpw=",
                            },
                            Enforce = true,
                            SendReport = true,
                        },
                        new()
                        {
                            Name = "protonvpn.com",
                            PublicKeyHashes = new HashSet<string>
                            {
                                "+0dMG0qG2Ga+dNE8uktwMm7dv6RFEXwBoBjQ43GqsQ0=",
                                "8joiNBdqaYiQpKskgtkJsqRxF7zN0C0aqfi8DacknnI=",
                                "JMI8yrbc6jB1FYGyyWRLFTmDNgIszrNEMGlgy972e7w=",
                                "Iu44zU84EOCZ9vx/vz67/MRVrxF1IO4i4NIa8ETwiIY=",
                            },
                            Enforce = true,
                            SendReport = true,
                        },
                        new()
                        {
                            Name = "download.protonvpn.net",
                            PublicKeyHashes = new HashSet<string>(),
                            Enforce = false,
                            SendReport = false,
                        },
                        new()
                        {
                            Name = "[InternalReleaseHost]", //this is replaced by CI script
                            PublicKeyHashes = new HashSet<string>(),
                            Enforce = false,
                            SendReport = false,
                        },
                        new()
                        {
                            Name = ALTERNATIVE_ROUTING_HOSTNAME,
                            PublicKeyHashes = new HashSet<string>
                            {
                                "EU6TS9MO0L/GsDHvVc9D5fChYLNy5JdGYpJw0ccgetM=",
                                "iKPIHPnDNqdkvOnTClQ8zQAIKG0XavaPkcEo0LBAABA=",
                                "MSlVrBCdL0hKyczvgYVSRNm88RicyY04Q2y5qrBt0xA=",
                                "C2UxW0T1Ckl9s+8cXfjXxlEqwAfPM4HiW2y3UdtBeCw=",
                            },
                            Enforce = true,
                            SendReport = true,
                        },
                    },
                    Enforce = false,
                },

                DoHProviders = new List<string>
                {
                    "https://dns11.quad9.net/dns-query",
                    "https://dns.google/dns-query",
                },

                NtpServerUrl = "time.windows.com",

                ServerValidationPublicKey = "MCowBQYDK2VwAyEANpYpt/FlSRwEuGLMoNAGOjy1BTyEJPJvKe00oln7LZk=",

                FailedDnsRequestTimeout = TimeSpan.FromSeconds(5),
                DnsResolveTimeout = TimeSpan.FromSeconds(30),
                DnsOverHttpsPerProviderTimeout = TimeSpan.FromSeconds(20),
                NewCacheTimeToLiveOnResolveError = TimeSpan.FromMinutes(10),
                DefaultDnsTimeToLive = TimeSpan.FromMinutes(20),

                AlternativeRoutingCheckInterval = TimeSpan.FromMinutes(30),

                AutoLoginBaseUrl = "https://account.proton.me/lite",
            };
        }
    }
}