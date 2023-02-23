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
using System.Threading.Tasks;
using ProtonVPN.Core.Settings;
using ProtonVPN.Dns.Caching;
using ProtonVPN.Dns.Contracts;

namespace ProtonVPN.Dns.Tests.Mocks
{
    public class MockOfDnsCacheManager : IDnsCacheManager
    {
        private readonly IAppSettings _appSettings;

        public MockOfDnsCacheManager(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public async Task<bool> AddOrReplaceAsync(string host, DnsResponse dnsResponse)
        {
            if (_appSettings.DnsCache is null)
            {
                _appSettings.DnsCache = new ConcurrentDictionary<string, DnsResponse>() { [host] = dnsResponse };
                return true;
            }
            else
            {
                return _appSettings.DnsCache.AddOrUpdate(host, dnsResponse, (_, _) => dnsResponse) == dnsResponse;
            }
        }

        public async Task<DnsResponse> UpdateAsync(string host, Func<DnsResponse, DnsResponse> dnsResponseUpdateFactory)
        {
            DnsResponse result = null;
            if (_appSettings.DnsCache is not null)
            {
                if (_appSettings.DnsCache.TryGetValue(host, out DnsResponse oldDnsResponse))
                {
                    DnsResponse newDnsResponse = dnsResponseUpdateFactory(oldDnsResponse);
                    if (_appSettings.DnsCache.TryUpdate(host, newDnsResponse, oldDnsResponse))
                    {
                        result = newDnsResponse;
                    }
                }
            }

            return result;
        }
    }
}