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

using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Contracts.Entities;

namespace ProtonVPN.Configurations;

public partial class Configuration : StaticConfiguration, IConfiguration
{
    public string ClientVersion => Get();
    public string ApiClientId => Get();
    public string UserAgent => Get();
    public string ApiVersion => Get();
    public string ServerValidationPublicKey => Get();

    public string GuestHoleVpnUsername => Get();
    public string GuestHoleVpnPassword => Get();
    public string VpnUsernameSuffix => Get();

    public long BugReportingMaxFileSize => Get();
    public int MaxClientLogsAttached => Get();
    public int MaxServiceLogsAttached => Get();
    public int MaxDiagnosticLogsAttached => Get();

    public int ApiRetries => Get();
    public int MaxGuestHoleRetries => Get();

    public bool IsCertificateValidationEnabled => Get();

    public TimeSpan ServiceCheckInterval => Get();
    public TimeSpan ClientConfigUpdateInterval => Get();
    public TimeSpan AuthCertificateUpdateInterval => Get();
    public TimeSpan ServerUpdateInterval => Get();
    public TimeSpan AnnouncementUpdateInterval => Get();
    public TimeSpan AlternativeRoutingCheckInterval => Get();
    public TimeSpan ApiUploadTimeout => Get();
    public TimeSpan ApiTimeout => Get();
    public TimeSpan FailedDnsRequestTimeout => Get();
    public TimeSpan NewCacheTimeToLiveOnResolveError => Get();
    public TimeSpan DnsResolveTimeout => Get();
    public TimeSpan DefaultDnsTimeToLive => Get();
    public TimeSpan DnsOverHttpsPerProviderTimeout => Get();
    public TimeSpan DohClientTimeout => Get();

    public IList<string> DohProviders => Get();
    public IUrlsConfiguration Urls => Get();
    public ITlsPinningConfiguration TlsPinning => Get();
}