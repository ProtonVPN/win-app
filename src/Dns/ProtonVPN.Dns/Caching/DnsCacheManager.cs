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
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.DnsLogs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Dns.Contracts;

namespace ProtonVPN.Dns.Caching
{
    public class DnsCacheManager : IDnsCacheManager
    {
        private readonly IAppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public DnsCacheManager(IAppSettings appSettings, ILogger logger)
        {
            _appSettings = appSettings;
            _logger = logger;
        }
        
        /// <summary>Adds the DnsResponse if the host doesn't exist or replaces the DnsResponse if the host already
        /// exists. If the cache null, it initializes it and adds the DnsResponse. Returns true if DnsResponse was
        /// added or replaced, and returns false if an exception was handled and ignored.</summary>
        public async Task<bool> AddOrReplaceAsync(string host, DnsResponse dnsResponse)
        {
            await _semaphore.WaitAsync();
            bool result;
            try
            {
                result = AddOrReplace(host, dnsResponse) == dnsResponse;
            }
            catch (Exception e)
            {
                result = false;
                _logger.Error<DnsErrorLog>($"DNS cache failed to add or replace host '{host}'.", e);
            }
            finally
            {
                _semaphore.Release();
            }

            return result;
        }

        private DnsResponse AddOrReplace(string host, DnsResponse dnsResponse)
        {
            ConcurrentDictionary<string, DnsResponse> dnsCache = _appSettings.DnsCache;
            DnsResponse cachedValue;
            if (dnsCache is null)
            {
                dnsCache = new ConcurrentDictionary<string, DnsResponse>() { [host] = dnsResponse };
                cachedValue = dnsResponse;
            }
            else
            {
                cachedValue = dnsCache.AddOrUpdate(host, dnsResponse, (_, _) => dnsResponse);
            }
            _appSettings.DnsCache = dnsCache;
            return cachedValue;
        }

        /// <summary>Updates the value and returns the new value if successful. If it fails, returns null.</summary>
        public async Task<DnsResponse> UpdateAsync(string host, Func<DnsResponse, DnsResponse> dnsResponseUpdateFactory)
        {
            await _semaphore.WaitAsync();
            DnsResponse dnsResponse = null;
            try
            {
                dnsResponse = Update(host, dnsResponseUpdateFactory);
            }
            catch (Exception e)
            {
                _logger.Error<DnsErrorLog>($"DNS cache failed to update host '{host}'.", e);
            }
            finally
            {
                _semaphore.Release();
            }

            return dnsResponse;
        }

        private DnsResponse Update(string host, Func<DnsResponse, DnsResponse> dnsResponseUpdateFactory)
        {
            ConcurrentDictionary<string, DnsResponse> dnsCache = _appSettings.DnsCache;
            DnsResponse dnsResponse = null;
            if (dnsCache is null)
            {
                _logger.Warn<DnsLog>($"DNS cache failed to update host '{host}' because the cache is null.");
            }
            else
            {
                if (dnsCache.TryGetValue(host, out DnsResponse oldDnsResponse))
                {
                    DnsResponse newDnsResponse = dnsResponseUpdateFactory(oldDnsResponse);
                    if (dnsCache.TryUpdate(host, newDnsResponse, oldDnsResponse))
                    {
                        _appSettings.DnsCache = dnsCache;
                        dnsResponse = newDnsResponse;
                    }
                    else
                    {
                        _logger.Error<DnsErrorLog>($"DNS cache update operation failed for host '{host}'.");
                    }
                }
                else
                {
                    _logger.Warn<DnsLog>($"DNS cache failed to update host '{host}' because this host doesn't exist.");
                }
            }

            return dnsResponse;
        }
    }
}