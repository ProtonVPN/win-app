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

using System.Collections.Generic;
using System.Net;
using ARSoft.Tools.Net.Dns;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.DnsLogs;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.Contracts.Resolvers;
using ProtonVPN.Dns.HttpClients;

namespace ProtonVPN.Dns.Resolvers
{
    public class DnsOverHttpsResolver : DnsOverHttpsResolverBase, IDnsOverHttpsResolver
    {
        public DnsOverHttpsResolver(IConfiguration configuration, ILogger logger, 
            IHttpClientFactory httpClientFactory, IDnsOverHttpsProvidersManager dnsOverHttpsProvidersManager)
            : base(configuration, logger, httpClientFactory, dnsOverHttpsProvidersManager, RecordType.A)
        {
        }

        protected override bool IsNullOrEmpty(DnsResponse dnsResponse)
        {
            return dnsResponse == null || dnsResponse.IpAddresses.IsNullOrEmpty();
        }

        protected override DnsResponse ParseDnsResponseMessage(DnsOverHttpsParallelHttpRequestConfiguration config, 
            DnsMessage dnsResponseMessage)
        {
            IList<IPAddress> ipAddresses = new List<IPAddress>();
            int? timeToLiveInSeconds = null;
            foreach (DnsRecordBase record in dnsResponseMessage.AnswerRecords)
            {
                if (record is ARecord aRecord)
                {
                    ipAddresses.Add(aRecord.Address);
                    if (aRecord.TimeToLive > 0 && (timeToLiveInSeconds == null || timeToLiveInSeconds.Value > aRecord.TimeToLive))
                    {
                        timeToLiveInSeconds = aRecord.TimeToLive;
                    }
                }
            }

            Logger.Info<DnsResponseLog>($"{ipAddresses.Count} records were received for host '{config.Host}' " +
                $"with DNS over HTTPS provider '{config.ProviderUrl}'. TTL is {timeToLiveInSeconds} seconds.");

            return CreateDnsResponseWithIpAddresses(config.Host, timeToLiveInSeconds, ipAddresses);
        }
    }
}
