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
    public class DnsOverHttpsTxtRecordsResolver : DnsOverHttpsResolverBase, IDnsOverHttpsTxtRecordsResolver
    {
        public DnsOverHttpsTxtRecordsResolver(IConfiguration configuration, ILogger logger, 
            IHttpClientFactory httpClientFactory, IDnsOverHttpsProvidersManager dnsOverHttpsProvidersManager) 
            : base(configuration, logger, httpClientFactory, dnsOverHttpsProvidersManager, RecordType.Txt)
        {
        }

        protected override bool IsNullOrEmpty(DnsResponse dnsResponse)
        {
            return dnsResponse == null || dnsResponse.AlternativeHosts.IsNullOrEmpty();
        }

        protected override DnsResponse ParseDnsResponseMessage(DnsOverHttpsParallelHttpRequestConfiguration config, 
            DnsMessage dnsResponseMessage)
        {
            IList<string> alternativeHosts = new List<string>();
            int? timeToLiveInSeconds = null;
            foreach (DnsRecordBase record in dnsResponseMessage.AnswerRecords)
            {
                if (record is TxtRecord txtRecord)
                {
                    alternativeHosts.Add(txtRecord.TextData);
                    if (txtRecord.TimeToLive > 0 && 
                        (timeToLiveInSeconds == null || timeToLiveInSeconds.Value > txtRecord.TimeToLive))
                    {
                        timeToLiveInSeconds = txtRecord.TimeToLive;
                    }
                }
            }

            Logger.Info<DnsResponseLog>($"{alternativeHosts.Count} TXT records were received for host '{config.Host}' " +
                $"with DNS over HTTPS provider '{config.ProviderUrl}'. TTL is {timeToLiveInSeconds} seconds.");

            return CreateDnsResponseWithAlternativeHosts(config.Host, timeToLiveInSeconds, alternativeHosts);
        }
    }
}
