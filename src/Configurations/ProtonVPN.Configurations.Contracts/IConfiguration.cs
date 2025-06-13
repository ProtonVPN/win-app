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

namespace ProtonVPN.Configurations.Contracts;

public interface IConfiguration : IStaticConfiguration
{
    string ClientVersion { get; }
    string ApiClientId { get; }
    string UserAgent { get; }
    string ApiVersion { get; }
    string ServerValidationPublicKey { get; }

    string GuestHoleVpnUsername { get; }
    string GuestHoleVpnPassword { get; }
    string VpnUsernameSuffix { get; }
    string DoHVerifyApiHost { get; }

    string NtpServerUrl { get; }

    int MaximumProfileNameLength { get; }

    long BugReportingMaxFileSize { get; }
    int MaxClientLogsAttached { get; }
    int MaxServiceLogsAttached { get; }
    int MaxDiagnosticLogsAttached { get; }

    int ApiRetries { get; }
    int MaxGuestHoleRetries { get; }

    decimal? DeviceRolloutProportion { get; }

    bool IsCertificateValidationEnabled { get; }

    TimeSpan ServiceCheckInterval { get; }
    TimeSpan ClientConfigUpdateInterval { get; }
    TimeSpan FeatureFlagsUpdateInterval { get; }
    TimeSpan ConnectionCertificateUpdateInterval { get; }
    TimeSpan ServerUpdateInterval { get; }
    TimeSpan ServerLoadUpdateInterval { get; }
    TimeSpan MinimumServerLoadUpdateInterval { get; }
    TimeSpan AnnouncementsUpdateInterval { get; }
    TimeSpan AlternativeRoutingCheckInterval { get; }
    TimeSpan UpdateCheckInterval { get; }
    TimeSpan ApiUploadTimeout { get; }
    TimeSpan ApiTimeout { get; }
    TimeSpan FailedDnsRequestTimeout { get; }
    TimeSpan NewCacheTimeToLiveOnResolveError { get; }
    TimeSpan DnsResolveTimeout { get; }
    TimeSpan DefaultDnsTimeToLive { get; }
    TimeSpan DnsOverHttpsPerProviderTimeout { get; }
    TimeSpan DohClientTimeout { get; }
    TimeSpan VpnStatePollingInterval { get; }
    TimeSpan VpnPlanRequestInterval { get; }
    TimeSpan VpnPlanMinimumRequestInterval { get; }
    TimeSpan NetShieldStatisticRequestInterval { get; }
    TimeSpan P2PTrafficDetectionInterval { get; }
    TimeSpan StatisticalEventMinimumWaitInterval { get; }
    TimeSpan StatisticalEventSendTriggerInterval { get; }

    IList<string> DohProviders { get; }
    IUrlsConfiguration Urls { get; }
    ITlsPinningConfiguration TlsPinning { get; }
}