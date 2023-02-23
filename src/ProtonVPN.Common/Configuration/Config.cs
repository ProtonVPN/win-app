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
using System.ComponentModel.DataAnnotations;
using ProtonVPN.Common.Configuration.Api.Handlers.TlsPinning;
using ProtonVPN.Common.Configuration.Source;

namespace ProtonVPN.Common.Configuration
{
    public class Config : IConfiguration
    {
        [Range(32, 64)]
        public int OsBits { get; set; }

        [Required]
        public string AppName { get; set; }

        [Required]
        public string AppVersion { get; set; }

        [Required]
        public string AppExePath { get; set; }

        [Required]
        public string AppLogFolder { get; set; }

        [Required]
        public string AppLogDefaultFullFilePath { get; set; }

        [Required]
        public string DiagnosticsLogFolder { get; set; }

        [Required]
        public string DiagnosticsZipPath { get; set; }

        [Required]
        public string TranslationsFolder { get; set; }

        [Required]
        public string ServiceName { get; set; }

        [Required]
        public string ServiceExePath { get; set; }

        [Required]
        public string ServiceSettingsFilePath { get; set; }

        [Required]
        public string ServersJsonCacheFilePath { get; set; }

        [Required]
        public string GuestHoleServersJsonFilePath { get; set; }

        [Required]
        public string StreamingServicesFilePath { get; set; }

        [Required]
        public string PartnersFilePath { get; set; }

        [Required]
        public string ServiceLogFolder { get; set; }

        [Required]
        public string ServiceLogDefaultFullFilePath { get; set; }

        [Required]
        public string UpdateFilePath { get; set; }

        [Required]
        public string UpdatesPath { get; set; }

        [Required]
        public string CalloutServiceName { get; set; }

        [Required]
        public string LocalAppDataFolder { get; set; }

        [Required]
        public string ImageCacheFolder { get; set; }

        [Range(1, 100)]
        public int MaxAppLogsAttached { get; set; }

        [Range(1, 100)]
        public int MaxDiagnosticLogsAttached { get; set; }

        [Range(1, 100)]
        public int MaxServiceLogsAttached { get; set; }

        [Required]
        public string ApiClientId { get; set; }

        [Required]
        public string ApiVersion { get; set; }

        [Required]
        public string UserAgent { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "00:05:00")]
        public TimeSpan ApiTimeout { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "00:05:00")]
        public TimeSpan ApiUploadTimeout { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "00:01:00")]
        public TimeSpan DohClientTimeout { get; set; }

        [Range(0, 5)]
        public int ApiRetries { get; set; }

        [Required]
        public int MaxGuestHoleRetries { get; set; }

        [Required]
        public string GuestHoleVpnUsername { get; set; }

        [Required]
        public string GuestHoleVpnPassword { get; set; }

        [Required]
        public string VpnUsernameSuffix { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "10:00:00:00")]
        public TimeSpan UpdateCheckInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "10.00:00:00")]
        public TimeSpan UpdateRemindInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan ServerUpdateInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan StreamingServicesUpdateInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan PartnersUpdateInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan AnnouncementUpdateInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan ServerLoadUpdateInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan ClientConfigUpdateInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan P2PCheckInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan VpnInfoCheckInterval { get; set; }

        [Required]
        public string DefaultCurrency { get; set; }

        [Range(1, 200)]
        public int ReportBugMaxFiles { get; set; }

        [Range(1, int.MaxValue)]
        public long ReportBugMaxFileSize { get; set; }

        [Range(1, 255)]
        public int MaxProfileNameLength { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan ForcedProfileSyncInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan EventCheckInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan ServiceCheckInterval { get; set; }

        [Required]
        public int[] DefaultOpenVpnUdpPorts { get; set; }

        [Required]
        public int[] DefaultOpenVpnTcpPorts { get; set; }

        [Required]
        public int[] DefaultWireGuardPorts { get; set; }

        public UrlConfig Urls { get; } = new();

        public OpenVpnConfig OpenVpn { get; } = new();

        public WireGuardConfig WireGuard { get; } = new();

        public TlsPinningConfig TlsPinningConfig { get; set; } = new();

        public List<string> DoHProviders { get; set; } = new();

        [Required]
        public string DefaultLocale { get; set; }

        [Range(typeof(TimeSpan), "00:00:00", "23:59:59")]
        public TimeSpan MaintenanceCheckInterval { get; set; }

        public int? MaxQuickConnectServersOnReconnection { get; set; } = DefaultConfig.MAX_QUICK_CONNECT_SERVERS_ON_RECONNECTION;

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan AuthCertificateUpdateInterval { get; set; }

        public string NtpServerUrl { get; set; }

        public string ServerValidationPublicKey { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "23:59:59")]
        public TimeSpan FailedDnsRequestTimeout { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "23:59:59")]
        public TimeSpan DnsResolveTimeout { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "23:59:59")]
        public TimeSpan DnsOverHttpsPerProviderTimeout { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "23:59:59")]
        public TimeSpan NewCacheTimeToLiveOnResolveError { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "23:59:59")]
        public TimeSpan DefaultDnsTimeToLive { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "23:59:59")]
        public TimeSpan AlternativeRoutingCheckInterval { get; set; }

        public string AutoLoginBaseUrl { get; set; }
    }
}