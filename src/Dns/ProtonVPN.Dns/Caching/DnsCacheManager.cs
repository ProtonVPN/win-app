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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.DnsLogs;

namespace ProtonVPN.Dns.Caching
{
    public class DnsCacheManager : IDnsCacheManager
    {
        private readonly ISettings _settings;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public DnsCacheManager(ISettings settings, ILogger logger)
        {
            _settings = settings;
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
            ConcurrentDictionary<string, DnsResponse> dnsCache = _settings.DnsCache;
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

            // When setting the DnsCache, it needs to be a new entity to trigger the Settings.Set()
            // code to actually recognize the value change and write it to the settings file
            _settings.DnsCache = new ConcurrentDictionary<string, DnsResponse>(dnsCache);

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
            ConcurrentDictionary<string, DnsResponse> dnsCache = _settings.DnsCache;
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
                        _settings.DnsCache = dnsCache;
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