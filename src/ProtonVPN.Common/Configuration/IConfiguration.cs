/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General License for more details.
 *
 * You should have received a copy of the GNU General License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProtonVPN.Common.Configuration.Api.Handlers.TlsPinning;

namespace ProtonVPN.Common.Configuration
{
    public interface IConfiguration
    {
        [Range(32, 64)]
        int OsBits { get; set; }

        [Required]
        string AppName { get; set; }

        [Required]
        string AppVersion { get; set; }

        [Required]
        string AppExePath { get; set; }

        [Required]
        string AppLogFolder { get; set; }

        [Required]
        string AppLogDefaultFullFilePath { get; set; }

        [Required]
        string DiagnosticsLogFolder { get; set; }

        [Required]
        string DiagnosticsZipPath { get; set; }

        [Required]
        string TranslationsFolder { get; set; }

        [Required]
        string ServiceName { get; set; }

        [Required]
        string ServiceExePath { get; set; }

        [Required]
        string ServiceSettingsFilePath { get; set; }

        [Required]
        string ServersJsonCacheFilePath { get; set; }

        [Required]
        string GuestHoleServersJsonFilePath { get; set; }

        [Required]
        string StreamingServicesFilePath { get; set; }

        [Required]
        string PartnersFilePath { get; set; }

        [Required]
        string ServiceLogFolder { get; set; }

        [Required]
        string ServiceLogDefaultFullFilePath { get; set; }

        [Required]
        string UpdateFilePath { get; set; }

        [Required]
        string UpdatesPath { get; set; }

        [Required]
        string CalloutServiceName { get; set; }

        [Required]
        string LocalAppDataFolder { get; set; }

        [Required]
        public string ImageCacheFolder { get; set; }

        [Range(1, 100)]
        int MaxAppLogsAttached { get; set; }

        [Range(1, 100)]
        int MaxDiagnosticLogsAttached { get; set; }

        [Range(1, 100)]
        int MaxServiceLogsAttached { get; set; }

        [Required]
        string ApiClientId { get; set; }

        [Required]
        string ApiVersion { get; set; }

        [Required]
        string UserAgent { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "00:05:00")]
        TimeSpan ApiTimeout { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "00:05:00")]
        TimeSpan ApiUploadTimeout { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "00:01:00")]
        TimeSpan DohClientTimeout { get; set; }

        [Range(0, 5)]
        int ApiRetries { get; set; }

        [Required]
        int MaxGuestHoleRetries { get; set; }

        [Required]
        string GuestHoleVpnUsername { get; set; }

        [Required]
        string GuestHoleVpnPassword { get; set; }

        [Required]
        string VpnUsernameSuffix { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "10:00:00:00")]
        TimeSpan UpdateCheckInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "10.00:00:00")]
        TimeSpan UpdateRemindInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        TimeSpan ServerUpdateInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        TimeSpan StreamingServicesUpdateInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        TimeSpan PartnersUpdateInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        TimeSpan AnnouncementUpdateInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        TimeSpan ServerLoadUpdateInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        TimeSpan ClientConfigUpdateInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        TimeSpan P2PCheckInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        TimeSpan VpnInfoCheckInterval { get; set; }

        [Required]
        string DefaultCurrency { get; set; }

        [Range(1, 200)]
        int ReportBugMaxFiles { get; set; }

        [Range(1, int.MaxValue)]
        long ReportBugMaxFileSize { get; set; }

        [Range(1, 255)]
        int MaxProfileNameLength { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        TimeSpan ForcedProfileSyncInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        TimeSpan EventCheckInterval { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        TimeSpan ServiceCheckInterval { get; set; }

        [Required]
        int[] DefaultOpenVpnUdpPorts { get; set; }

        [Required]
        int[] DefaultOpenVpnTcpPorts { get; set; }

        [Required]
        int[] DefaultWireGuardPorts { get; set; }

        UrlConfig Urls { get; }

        OpenVpnConfig OpenVpn { get; }

        WireGuardConfig WireGuard { get; }

        TlsPinningConfig TlsPinningConfig { get; set; }

        List<string> DoHProviders { get; set; }

        [Required]
        string DefaultLocale { get; set; }

        [Range(typeof(TimeSpan), "00:00:00", "23:59:59")]
        TimeSpan MaintenanceCheckInterval { get; set; }

        int? MaxQuickConnectServersOnReconnection { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        TimeSpan AuthCertificateUpdateInterval { get; set; }

        string NtpServerUrl { get; set; }

        string ServerValidationPublicKey { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "23:59:59")]
        TimeSpan FailedDnsRequestTimeout { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "23:59:59")]
        TimeSpan DnsResolveTimeout { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "23:59:59")]
        TimeSpan DnsOverHttpsPerProviderTimeout { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "23:59:59")]
        TimeSpan NewCacheTimeToLiveOnResolveError { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "23:59:59")]
        TimeSpan DefaultDnsTimeToLive { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "23:59:59")]
        TimeSpan AlternativeRoutingCheckInterval { get; set; }

        public string AutoLoginBaseUrl { get; set; }
    }
}