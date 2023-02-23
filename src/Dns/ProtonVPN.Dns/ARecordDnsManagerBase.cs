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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.DnsLogs;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Settings;
using ProtonVPN.Dns.Caching;
using ProtonVPN.Dns.Contracts;

namespace ProtonVPN.Dns
{
    public abstract class ARecordDnsManagerBase
    {
        protected TimeSpan FailedDnsRequestTimeout { get; }
        protected TimeSpan NewCacheTimeToLiveOnResolveError { get; }
        protected ILogger Logger { get; }
        protected IDnsCacheManager DnsCacheManager { get; }

        private readonly IAppSettings _appSettings;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ConcurrentDictionary<string, DateTime> _failedRequestsCache = new();

        protected ARecordDnsManagerBase(IAppSettings appSettings, IConfiguration configuration,
            ILogger logger, IDnsCacheManager dnsCacheManager)
        {
            FailedDnsRequestTimeout = configuration.FailedDnsRequestTimeout;
            NewCacheTimeToLiveOnResolveError = configuration.NewCacheTimeToLiveOnResolveError;
            Logger = logger;
            DnsCacheManager = dnsCacheManager;
            _appSettings = appSettings;
        }

        public async Task<IList<IpAddress>> GetAsync(string host, CancellationToken cancellationToken)
        {
            IList<IpAddress> ipAddresses = GetFreshIpAddressesFromCache(host);
            if (ipAddresses.Count == 0)
            {
                ipAddresses = await ResolveOrGetFromCacheAsync(host, cancellationToken);
            }

            return ipAddresses;
        }

        private async Task<IList<IpAddress>> ResolveOrGetFromCacheAsync(string host, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
            }
            catch
            {
                Logger.Warn<DnsErrorLog>($"DNS resolve of host '{host}' was cancelled while waiting.");
                return new List<IpAddress>();
            }
            IList<IpAddress> ipAddresses;
            try
            {
                ipAddresses = GetFreshIpAddressesFromCache(host);
                if (ipAddresses.Count == 0)
                {
                    if (_failedRequestsCache.TryGetValue(host, out DateTime timeoutEndDateUtc) && timeoutEndDateUtc > DateTime.UtcNow)
                    {
                        Logger.Debug<DnsLog>($"Skipping DNS resolve of host '{host}' because its under timeout.");
                        ipAddresses = GetIpAddressesFromCache(host);
                    }
                    else
                    {
                        Logger.Info<DnsLog>($"No fresh IP addresses for host '{host}' were found in the cache. Triggering a refresh.");
                        ipAddresses = await ResolveHostAsync(host, cancellationToken);
                        if (ipAddresses.Count == 0)
                        {
                            ipAddresses = await GetIpAddressesFromCacheAndSetNewTtlAsync(host);
                        }
                    }
                }
                else
                {
                    Logger.Debug<DnsLog>($"Locked re-check for a fresh DNS cache of host '{host}' was successful.");
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return ipAddresses;
        }

        private IList<IpAddress> GetFreshIpAddressesFromCache(string host)
        {
            IList<IpAddress> ipAddresses = new List<IpAddress>();
            DateTime currentDateTimeUtc = DateTime.UtcNow;

            if (_appSettings.DnsCache.TryGetValueIfDictionaryIsNotNull(host, out DnsResponse dnsResponse) &&
                dnsResponse.ExpirationDateTimeUtc > currentDateTimeUtc)
            {
                ipAddresses = dnsResponse.IpAddresses;
            }

            return ipAddresses;
        }

        private IList<IpAddress> GetIpAddressesFromCache(string host)
        {
            IList<IpAddress> ipAddresses = new List<IpAddress>();

            if (_appSettings.DnsCache.TryGetValueIfDictionaryIsNotNull(host, out DnsResponse dnsResponse))
            {
                ipAddresses = dnsResponse.IpAddresses;
            }

            return ipAddresses ?? new List<IpAddress>();
        }

        private async Task<IList<IpAddress>> GetIpAddressesFromCacheAndSetNewTtlAsync(string host)
        {
            IList<IpAddress> ipAddresses = GetIpAddressesFromCache(host);

            if (ipAddresses.Any())
            {
                DnsResponse newDnsResponse = await DnsCacheManager.UpdateAsync(host, SetDatesAndTimeToLiveFactory);
                Logger.Info<DnsLog>($"Returning cached IP addresses for host '{host}'. " +
                    $"New TTL of {newDnsResponse.TimeToLive} resulting in a " +
                    $"new expiration date of {newDnsResponse.ExpirationDateTimeUtc}.");
            }
            else
            {
                DateTime timeoutEndDateUtc = DateTime.UtcNow + FailedDnsRequestTimeout;
                _failedRequestsCache.AddOrUpdate(host, timeoutEndDateUtc, (_, _) => timeoutEndDateUtc);
                Logger.Warn<DnsErrorLog>($"No cached IP addresses exist for host '{host}'. " +
                    $"Next resolve can only be made after '{timeoutEndDateUtc}'.");
            }

            return ipAddresses;
        }

        private DnsResponse SetDatesAndTimeToLiveFactory(DnsResponse dnsResponse)
        {
            dnsResponse.SetDatesAndTimeToLive(NewCacheTimeToLiveOnResolveError);
            return dnsResponse;
        }

        protected abstract Task<IList<IpAddress>> ResolveHostAsync(string host, CancellationToken cancellationToken);

        public async Task<IList<IpAddress>> ResolveWithoutCacheAsync(string host, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
            }
            catch
            {
                Logger.Warn<DnsErrorLog>($"DNS resolve of host '{host}' was cancelled while waiting.");
                return new List<IpAddress>();
            }
            IList<IpAddress> ipAddresses = new List<IpAddress>();
            try
            {
                if (_failedRequestsCache.TryGetValue(host, out DateTime timeoutEndDateUtc) && timeoutEndDateUtc > DateTime.UtcNow)
                {
                    Logger.Debug<DnsLog>($"Skipping forced DNS resolve of host '{host}' because its under timeout.");
                }
                else
                {
                    Logger.Info<DnsLog>($"Starting resolve of IP addresses for host '{host}'.");
                    ipAddresses = await ResolveHostAsync(host, cancellationToken);
                }
            }
            finally
            {
                _semaphore.Release();
            }
            return ipAddresses;
        }

        public IList<IpAddress> GetFromCache(string host)
        {
            ConcurrentDictionary<string, DnsResponse> dnsCache = _appSettings.DnsCache;
            IList<IpAddress> ipAddresses = new List<IpAddress>();

            if (dnsCache.TryGetValueIfDictionaryIsNotNull(host, out DnsResponse dnsResponse))
            {
                ipAddresses = dnsResponse.IpAddresses;
            }

            return ipAddresses;
        }
    }
}