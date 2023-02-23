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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.DnsLogs;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Settings;
using ProtonVPN.Dns.Caching;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.Contracts.Resolvers;

namespace ProtonVPN.Dns
{
    public class DnsOverHttpsProvidersManager : ARecordDnsManagerBase, IDnsOverHttpsProvidersManager
    {
        private readonly IDnsOverUdpResolver _dnsOverUdpResolver;

        public DnsOverHttpsProvidersManager(IDnsOverUdpResolver dnsOverUdpResolver,
            IAppSettings appSettings, IConfiguration configuration, ILogger logger, IDnsCacheManager dnsCacheManager)
            : base(appSettings, configuration, logger, dnsCacheManager)
        {
            _dnsOverUdpResolver = dnsOverUdpResolver;
        }

        protected override async Task<IList<IpAddress>> ResolveHostAsync(string host, CancellationToken cancellationToken)
        {
            try
            {
                Logger.Info<DnsLog>($"Attempting a UDP DNS request for host '{host}'.");
                DnsResponse dnsResponse = await _dnsOverUdpResolver.ResolveAsync(host, cancellationToken);

                if (dnsResponse != null && dnsResponse.IpAddresses.Any())
                {
                    Logger.Info<DnsLog>($"The UDP DNS request was successful for host '{host}'. Saving to cache.");
                    IList<IpAddress> ipAddresses = dnsResponse.IpAddresses;
                    await DnsCacheManager.AddOrReplaceAsync(host, dnsResponse);
                    return ipAddresses;
                }

                Logger.Error<DnsErrorLog>($"The UDP DNS request was unsuccessful for host '{host}'.");
            }
            catch (Exception e)
            {
                Logger.Error<DnsErrorLog>($"An unexpected error as occurred when resolving UDP DNS for host '{host}'.", e);
            }

            return new List<IpAddress>();
        }
    }
}