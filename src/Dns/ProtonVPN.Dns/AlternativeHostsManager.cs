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
using ProtonVPN.Core.Settings;
using ProtonVPN.Dns.Caching;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.Contracts.Resolvers;

namespace ProtonVPN.Dns
{
    public class AlternativeHostsManager : IAlternativeHostsManager
    {
        private readonly IDnsOverHttpsTxtRecordsResolver _dnsOverHttpsTxtRecordsResolver;
        private readonly IAppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly IDnsCacheManager _dnsCacheManager;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ConcurrentDictionary<string, DateTime> _failedRequestsCache = new();
        private readonly TimeSpan _failedDnsRequestTimeout;
        private readonly TimeSpan _newCacheTimeToLiveOnResolveError;

        public AlternativeHostsManager(IDnsOverHttpsTxtRecordsResolver dnsOverHttpsTxtRecordsResolver, 
            IAppSettings appSettings, IConfiguration configuration, ILogger logger, IDnsCacheManager dnsCacheManager)
        {
            _dnsOverHttpsTxtRecordsResolver = dnsOverHttpsTxtRecordsResolver;
            _appSettings = appSettings;
            _logger = logger;
            _dnsCacheManager = dnsCacheManager;
            _failedDnsRequestTimeout = configuration.FailedDnsRequestTimeout;
            _newCacheTimeToLiveOnResolveError  = configuration.NewCacheTimeToLiveOnResolveError;
        }
        
        public async Task<IList<string>> GetAsync(string host, CancellationToken cancellationToken)
        {
            IList<string> alternativeHosts = GetFreshAlternativeHostsFromCache(host);
            if (alternativeHosts.Count == 0)
            {
                alternativeHosts = await ResolveOrGetFromCacheAsync(host, cancellationToken);
            }

            return alternativeHosts;
        }

        private async Task<IList<string>> ResolveOrGetFromCacheAsync(string host, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
            }
            catch
            {
                _logger.Warn<DnsErrorLog>($"Alternative hosts resolve of host '{host}' was cancelled while waiting.");
                return new List<string>();
            }
            IList<string> alternativeHosts;
            try
            {
                alternativeHosts = GetFreshAlternativeHostsFromCache(host);
                if (alternativeHosts.Count == 0)
                {
                    if (_failedRequestsCache.TryGetValue(host, out DateTime timeoutEndDateUtc) && timeoutEndDateUtc > DateTime.UtcNow)
                    {
                        _logger.Debug<DnsLog>($"Skipping alternative hosts resolve of host '{host}' because its under timeout.");
                        alternativeHosts = GetAlternativeHostsFromCache(host);
                    }
                    else
                    {
                        _logger.Info<DnsLog>($"No fresh alternative hosts for host '{host}' were found in the cache. " +
                            $"Triggering a refresh.");
                        alternativeHosts = await ResolveHostAsync(host, cancellationToken);
                        if (alternativeHosts.Count == 0)
                        {
                            alternativeHosts = await GetAlternativeHostsFromCacheAndSetNewTtlAsync(host);
                        }
                    }
                }
                else
                {
                    _logger.Debug<DnsLog>($"Locked re-check for a fresh alternative " +
                        $"hosts cache of host '{host}' was successful.");
                }
            }
            finally
            {
                _semaphore.Release();
                _logger.Debug<DnsLog>($"Released semaphore of alternative hosts DNS resolve of host '{host}'.");
            }

            return alternativeHosts;
        }

        private IList<string> GetFreshAlternativeHostsFromCache(string host)
        {
            IList<string> alternativeHosts = new List<string>();
            DateTime currentDateTimeUtc = DateTime.UtcNow;

            if (_appSettings.DnsCache.TryGetValueIfDictionaryIsNotNull(host, out DnsResponse dnsResponse) &&
                dnsResponse.ExpirationDateTimeUtc > currentDateTimeUtc)
            {
                alternativeHosts = dnsResponse.AlternativeHosts;
            }

            return alternativeHosts;
        }

        private IList<string> GetAlternativeHostsFromCache(string host)
        {
            IList<string> alternativeHosts = new List<string>();

            if (_appSettings.DnsCache.TryGetValueIfDictionaryIsNotNull(host, out DnsResponse dnsResponse))
            {
                alternativeHosts = dnsResponse.AlternativeHosts;
            }

            return alternativeHosts ?? new List<string>();
        }

        private async Task<IList<string>> GetAlternativeHostsFromCacheAndSetNewTtlAsync(string host)
        {
            IList<string> alternativeHosts = GetAlternativeHostsFromCache(host);

            if (alternativeHosts.Any())
            {
                DnsResponse newDnsResponse = await _dnsCacheManager.UpdateAsync(host, SetDatesAndTimeToLiveFactory);
                _logger.Info<DnsLog>($"Returning cached alternative hosts for host '{host}'. " +
                    $"New TTL of {newDnsResponse.TimeToLive} resulting in a " +
                    $"new expiration date of {newDnsResponse.ExpirationDateTimeUtc}.");
            }
            else
            {
                DateTime timeoutEndDateUtc = DateTime.UtcNow + _failedDnsRequestTimeout;
                _failedRequestsCache.AddOrUpdate(host, timeoutEndDateUtc, (_, _) => timeoutEndDateUtc);
                _logger.Warn<DnsErrorLog>($"No cached alternative hosts exist for host '{host}'. " +
                    $"Next resolve can only be made after '{timeoutEndDateUtc}'.");
            }

            return alternativeHosts;
        }

        private DnsResponse SetDatesAndTimeToLiveFactory(DnsResponse dnsResponse)
        {
            dnsResponse.SetDatesAndTimeToLive(_newCacheTimeToLiveOnResolveError);
            return dnsResponse;
        }

        private async Task<IList<string>> ResolveHostAsync(string host, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Info<DnsLog>($"Attempting a HTTPS DNS request for TXT records of host '{host}'.");
                DnsResponse dnsResponse = await _dnsOverHttpsTxtRecordsResolver.ResolveAsync(host, cancellationToken);

                if (dnsResponse != null && dnsResponse.AlternativeHosts.Any())
                {
                    _logger.Info<DnsLog>($"The HTTPS DNS request for TXT records of host '{host}' was successful. " +
                        "Saving to cache.");
                    IList<string> alternativeHosts = dnsResponse.AlternativeHosts;
                    await _dnsCacheManager.AddOrReplaceAsync(host, dnsResponse);
                    return alternativeHosts;
                }
                
                _logger.Error<DnsErrorLog>($"The HTTPS DNS request for TXT records of host '{host}' was unsuccessful.");
            }
            catch (Exception e)
            {
                _logger.Error<DnsErrorLog>($"An unexpected error as occurred when resolving " +
                    $"HTTPS DNS for TXT records of host '{host}'.", e);
            }

            return new List<string>();
        }
    }
}