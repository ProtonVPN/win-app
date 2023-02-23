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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.DnsLogs;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.Contracts.NameServers;
using ProtonVPN.Dns.Contracts.Resolvers;

namespace ProtonVPN.Dns.Resolvers
{
    public class DnsOverUdpResolver : DnsResolverBase, IDnsOverUdpResolver
    {
        private readonly INameServersLoader _nameServersLoader;

        public DnsOverUdpResolver(INameServersLoader nameServersLoader, IConfiguration configuration, ILogger logger)
            : base(configuration, logger)
        {
            _nameServersLoader = nameServersLoader;
        }

        protected override async Task<DnsResponse> StartTasksAndWaitAnySuccessAsync(string host,
            CancellationToken cancellationToken)
        {
            IEnumerable<IPEndPoint> nameServersIpAddresses = _nameServersLoader.Get();

            List<Func<CancellationToken, Task<DnsResponse>>> resolveFuncs = new();
            foreach (IPEndPoint nameServerIpAddress in nameServersIpAddresses)
            {
                resolveFuncs.Add(ct => TryResolveAsync(nameServerIpAddress, host, ct));
            }

            return await WaitAnySuccessfulResolveAsync(resolveFuncs, DnsResolveTimeout, cancellationToken);
        }

        private async Task<DnsResponse> TryResolveAsync(IPEndPoint nameServerIpAddress, string host,
            CancellationToken cancellationToken)
        {
            Logger.Info<DnsResolveLog>($"Attempting to resolve host '{host}' through '{nameServerIpAddress}'.");
            DnsResponse dnsResponse = null;
            try
            {
                dnsResponse = await ResolveAsync(nameServerIpAddress, host, cancellationToken);
            }
            catch (Exception e)
            {
                if (e.IsOrAnyInnerIsOfExceptionType<OperationCanceledException>())
                {
                    LogOperationCancelled($"The DNS over UDP resolver through '{nameServerIpAddress}' " +
                        $"was canceled when resolving host '{host}'.");
                }
                else if (e is DnsResponseException)
                {
                    Logger.Warn<DnsErrorLog>($"DNS failed to get a response from '{nameServerIpAddress}'.", e);
                }
                else
                {
                    Logger.Error<DnsErrorLog>("Unexpected error in DNS task wait.", e);
                }
            }

            return dnsResponse;
        }

        private async Task<DnsResponse> ResolveAsync(IPEndPoint nameServerIpAddress, string host,
            CancellationToken cancellationToken)
        {
            LookupClientOptions lookupClientOptions = CreateLookupClientOptions(nameServerIpAddress);
            ILookupClient lookupClient = new LookupClient(lookupClientOptions);
            IDnsQueryResponse dnsQueryResponse =
                await lookupClient.QueryAsync(host, QueryType.A, cancellationToken: cancellationToken);
            if (dnsQueryResponse.HasError)
            {
                throw new DnsResponseException((DnsResponseCode)dnsQueryResponse.Header.ResponseCode, dnsQueryResponse.ErrorMessage);
            }

            Logger.Info<DnsResponseLog>($"The endpoint '{nameServerIpAddress}' responded successfully to the DNS query of host '{host}'.");
            IList<ARecord> aRecords = dnsQueryResponse.Answers.ARecords().ToList();
            IList<IPAddress> ipAddresses = aRecords.Select(ar => ar.Address.MapToIPv4())
                .Where(ia => !Equals(ia, IPAddress.None) && !Equals(ia, IPAddress.Loopback))
                .ToList();
            int timeToLiveInSeconds = aRecords.Select(ar => ar.InitialTimeToLive).Where(ttl => ttl > 0).DefaultIfEmpty().Min();

            Logger.Info<DnsResponseLog>($"{ipAddresses.Count} records were received for host '{host}' " +
                $"with DNS over UDP endpoint '{nameServerIpAddress}'. TTL is {timeToLiveInSeconds} seconds.");

            return CreateDnsResponseWithIpAddresses(host, timeToLiveInSeconds, ipAddresses);
        }

        private LookupClientOptions CreateLookupClientOptions(IPEndPoint nameServerIpAddress)
        {
            return new LookupClientOptions(nameServerIpAddress)
            {
                UseTcpOnly = false,
                UseTcpFallback = true,
                Timeout = TimeSpan.FromSeconds(5),
                UseCache = false,
                CacheFailedResults = true,
                FailedResultsCacheDuration = TimeSpan.FromSeconds(10),
                Retries = 2,
                Recursion = true,
                UseRandomNameServer = false,
                ThrowDnsErrors = true,
            };
        }

        protected override bool IsNullOrEmpty(DnsResponse dnsResponse)
        {
            return dnsResponse == null || dnsResponse.IpAddresses.IsNullOrEmpty();
        }
    }
}