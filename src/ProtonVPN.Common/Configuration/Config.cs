/*
 * Copyright (c) 2020 Proton Technologies AG
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
    public class Config
    {
        [Range(32, 64)]
        public int OsBits { get; internal set; }

        [Required]
        public string AppName { get; internal set; }

        [Required]
        public string AppVersion { get; internal set; }

        [Required]
        public string AppExePath { get; internal set; }

        [Required]
        public string AppLogFolder { get; internal set; }

        [Required]
        public string DiagnosticsLogFolder { get; internal set; }

        [Required]
        public string DiagnosticsZipPath { get; internal set; }

        [Required]
        public string TranslationsFolder { get; internal set; }

        [Required]
        public string ErrorMessageExePath { get; internal set; }

        [Required]
        public string ServiceName { get; internal set; }

        [Required]
        public string ServiceExePath { get; internal set; }

        [Required]
        public string ServiceSettingsFilePath { get; internal set; }

        [Required]
        public string ServersJsonCacheFilePath { get; internal set; }

        [Required]
        public string GuestHoleServersJsonFilePath { get; internal set; }

        [Required]
        public string StreamingServicesFilePath { get; internal set; }

        [Required]
        public string ServiceLogFolder { get; internal set; }

        [Required]
        public string UpdateServiceName { get; internal set; }

        [Required]
        public string UpdateServiceExePath { get; internal set; }

        [Required]
        public string UpdateServiceLogFolder { get; internal set; }

        [Required]
        public string UpdateFilePath { get; internal set; }

        [Required]
        public string UpdatesPath { get; internal set; }

        [Required]
        public string CalloutServiceName { get; internal set; }

        [Required]
        public string LocalAppDataFolder { get; internal set; }

        [Range(1, 100)]
        public int MaxAppLogsAttached { get; internal set; }

        [Range(1, 100)]
        public int MaxDiagnosticLogsAttached { get; internal set; }

        [Range(1, 100)]
        public int MaxServiceLogsAttached { get; internal set; }

        [Range(1, 100)]
        public int MaxUpdaterServiceLogsAttached { get; internal set; }

        [Required]
        public string ApiClientId { get; internal set; }

        [Required]
        public string ApiVersion { get; internal set; }

        [Required]
        public string UserAgent { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:01", "00:01:00")]
        public TimeSpan ApiTimeout { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:01", "00:01:00")]
        public TimeSpan DohClientTimeout { get; internal set; }

        [Range(0, 5)]
        public int ApiRetries { get; internal set; }

        [Required]
        public int MaxGuestHoleRetries { get; internal set; }

        [Required]
        public string GuestHoleVpnUsername { get; internal set; }

        [Required]
        public string GuestHoleVpnPassword { get; internal set; }

        [Required]
        public string VpnUsernameSuffix { get; set; }

        [Range(typeof(TimeSpan), "00:00:10", "10:00:00:00")]
        public TimeSpan UpdateCheckInterval { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:10", "10.00:00:00")]
        public TimeSpan UpdateRemindInterval { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan ServerUpdateInterval { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan StreamingServicesUpdateInterval { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan AnnouncementUpdateInterval { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan ServerLoadUpdateInterval { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan ClientConfigUpdateInterval { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan P2PCheckInterval { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan VpnInfoCheckInterval { get; internal set; }

        [Required]
        public string DefaultCurrency { get; internal set; }

        [Range(1, 200)]
        public int ReportBugMaxFiles { get; internal set; }

        [Range(1, int.MaxValue)]
        public long ReportBugMaxFileSize { get; internal set; }

        [Range(1, 255)]
        public int MaxProfileNameLength { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan ProfileSyncTimerPeriod { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan ProfileSyncPeriod { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan ForcedProfileSyncInterval { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan EventCheckInterval { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan ServiceCheckInterval { get; internal set; }

        [Required]
        public int[] DefaultOpenVpnUdpPorts { get; internal set; }

        [Required]
        public int[] DefaultOpenVpnTcpPorts { get; internal set; }

        public IReadOnlyList<string> DefaultBlackHoleIps { get; internal set; } = new List<string>();

        public UrlConfig Urls { get; } = new();

        public OpenVpnConfig OpenVpn { get; } = new();

        public WireGuardConfig WireGuard { get; } = new();

        public TlsPinningConfig TlsPinningConfig { get; } = new();

        public List<string> DoHProviders { get; internal set; } = new();

        [Required]
        public string DefaultLocale { get; internal set; }

        [Range(typeof(TimeSpan), "00:00:00", "23:59:59")]
        public TimeSpan MaintenanceCheckInterval { get; internal set; }

        public int? MaxQuickConnectServersOnReconnection { get; internal set; } = DefaultConfig.MAX_QUICK_CONNECT_SERVERS_ON_RECONNECTION;

        [Range(typeof(TimeSpan), "00:00:10", "23:59:59")]
        public TimeSpan AuthCertificateUpdateInterval { get; internal set; }
        
        [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
        public TimeSpan AuthCertificateFirstRetryInterval { get; internal set; }
        
        [Range(0, 16)]
        public int AuthCertificateMaxNumOfRetries { get; internal set; }
    }
}