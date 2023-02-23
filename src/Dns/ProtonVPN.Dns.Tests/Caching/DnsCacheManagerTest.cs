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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Settings;
using ProtonVPN.Dns.Caching;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.Tests.Mocks;

namespace ProtonVPN.Dns.Tests.Caching
{
    [TestClass]
    public class DnsCacheManagerTest
    {
        public const int NUM_OF_PARALLEL_OPERATIONS = 100;

        private IAppSettings _appSettings;
        private MockOfLogger _logger;
        private DnsCacheManager _dnsCacheManager;

        [TestInitialize]
        public void TestInitialize()
        {
            _appSettings = Substitute.For<IAppSettings>();
            _logger = new MockOfLogger();
            _dnsCacheManager = new DnsCacheManager(_appSettings, _logger);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _appSettings = null;
            _logger = null;
            _dnsCacheManager = null;
        }

        [TestMethod]
        public async Task TestAddOrReplaceAsync_WhenCacheIsNull()
        {
            DnsResponse dnsResponse = new("host",
                TimeSpan.FromSeconds(12),
                new List<IpAddress> { new IpAddress(IPAddress.Parse("192.168.12.12")) });
            Assert.IsNull(_appSettings.DnsCache);

            await _dnsCacheManager.AddOrReplaceAsync(dnsResponse.Host, dnsResponse);

            Assert.IsNotNull(_appSettings.DnsCache);
            Assert.IsTrue(_appSettings.DnsCache.ContainsKey(dnsResponse.Host));
            Assert.AreEqual(dnsResponse, _appSettings.DnsCache[dnsResponse.Host]);
        }

        [TestMethod]
        public async Task TestAddOrReplaceAsync_WhenCacheContainsSameHost()
        {
            DnsResponse dnsResponse = new("host",
                TimeSpan.FromSeconds(12),
                new List<IpAddress> { new IpAddress(IPAddress.Parse("192.168.12.12")) });

            DnsResponse cachedDnsResponse = new("host",
                TimeSpan.FromSeconds(13),
                new List<IpAddress> { new IpAddress(IPAddress.Parse("192.168.13.13")) });
            _appSettings.DnsCache = new() { [cachedDnsResponse.Host] = cachedDnsResponse };

            await _dnsCacheManager.AddOrReplaceAsync(dnsResponse.Host, dnsResponse);

            Assert.IsNotNull(_appSettings.DnsCache);
            Assert.IsTrue(_appSettings.DnsCache.ContainsKey(dnsResponse.Host));
            Assert.AreEqual(dnsResponse, _appSettings.DnsCache[dnsResponse.Host]);
        }

        [TestMethod]
        public async Task TestAddOrReplaceAsync_WhenCacheContainsDifferentHost()
        {
            DnsResponse dnsResponse = new("host12",
                TimeSpan.FromSeconds(12),
                new List<IpAddress> { new IpAddress(IPAddress.Parse("192.168.12.12")) });

            DnsResponse cachedDnsResponse = new("host13",
                TimeSpan.FromSeconds(13),
                new List<IpAddress> { new IpAddress(IPAddress.Parse("192.168.13.13")) });
            _appSettings.DnsCache = new() { [cachedDnsResponse.Host] = cachedDnsResponse };

            await _dnsCacheManager.AddOrReplaceAsync(dnsResponse.Host, dnsResponse);

            Assert.IsNotNull(_appSettings.DnsCache);
            Assert.IsTrue(_appSettings.DnsCache.ContainsKey(dnsResponse.Host));
            Assert.AreEqual(dnsResponse, _appSettings.DnsCache[dnsResponse.Host]);
            Assert.IsTrue(_appSettings.DnsCache.ContainsKey(cachedDnsResponse.Host));
            Assert.AreEqual(cachedDnsResponse, _appSettings.DnsCache[cachedDnsResponse.Host]);
        }

        [TestMethod]
        public async Task TestAddOrReplaceAsync_WhenArgumentsAndCacheAreNull()
        {
            Assert.IsNull(_appSettings.DnsCache);

            await _dnsCacheManager.AddOrReplaceAsync(null, null);

            Assert.IsNull(_appSettings.DnsCache);
        }

        [TestMethod]
        public async Task TestAddOrReplaceAsync_WhenArgumentsAreNullAndCacheContainsDifferentHost()
        {
            DnsResponse cachedDnsResponse = new("host13",
                TimeSpan.FromSeconds(13),
                new List<IpAddress> { new IpAddress(IPAddress.Parse("192.168.13.13")) });
            _appSettings.DnsCache = new() { [cachedDnsResponse.Host] = cachedDnsResponse };

            await _dnsCacheManager.AddOrReplaceAsync(null, null);

            Assert.IsNotNull(_appSettings.DnsCache);
            Assert.IsTrue(_appSettings.DnsCache.ContainsKey(cachedDnsResponse.Host));
            Assert.AreEqual(cachedDnsResponse, _appSettings.DnsCache[cachedDnsResponse.Host]);
        }

        [TestMethod]
        public void TestAddOrReplaceAsync_ParallelWithDifferentHosts()
        {
            IList<DnsResponse> dnsResponses = new List<DnsResponse>();
            for (int i = 0; i < NUM_OF_PARALLEL_OPERATIONS; i++)
            {
                dnsResponses.Add(new DnsResponse($"host{i}",
                    TimeSpan.FromSeconds(100 + i),
                    new List<IpAddress> { new IpAddress(IPAddress.Parse($"192.168.{i}.{i}")) }));
            }
            Assert.IsNull(_appSettings.DnsCache);
            
            IList<Task<bool>> tasks = new List<Task<bool>>();
            foreach (DnsResponse dnsResponse in dnsResponses)
            {
                tasks.Add(_dnsCacheManager.AddOrReplaceAsync(dnsResponse.Host, dnsResponse));
            }

            Task.WaitAll(tasks.ToArray());

            Assert.IsNotNull(_appSettings.DnsCache);
            Assert.AreEqual(NUM_OF_PARALLEL_OPERATIONS, _appSettings.DnsCache.Count);
            foreach (DnsResponse dnsResponse in dnsResponses)
            {
                Assert.IsTrue(_appSettings.DnsCache.ContainsKey(dnsResponse.Host));
                Assert.AreEqual(dnsResponse, _appSettings.DnsCache[dnsResponse.Host]);
            }
        }

        [TestMethod]
        public void TestAddOrReplaceAsync_ParallelWithSameHost()
        {
            IList<DnsResponse> dnsResponses = new List<DnsResponse>();
            for (int i = 0; i < NUM_OF_PARALLEL_OPERATIONS; i++)
            {
                dnsResponses.Add(new DnsResponse("host",
                    TimeSpan.FromSeconds(100 + i),
                    new List<IpAddress> { new IpAddress(IPAddress.Parse($"192.168.{i}.{i}")) }));
            }
            Assert.IsNull(_appSettings.DnsCache);
            
            IList<Task<bool>> tasks = new List<Task<bool>>();
            foreach (DnsResponse dnsResponse in dnsResponses)
            {
                tasks.Add(_dnsCacheManager.AddOrReplaceAsync(dnsResponse.Host, dnsResponse));
            }

            Task.WaitAll(tasks.ToArray());

            Assert.IsNotNull(_appSettings.DnsCache);
            Assert.AreEqual(1, _appSettings.DnsCache.Count);
            Assert.IsTrue(_appSettings.DnsCache.ContainsKey("host"));
            Assert.AreEqual("host", _appSettings.DnsCache["host"].Host);
            Assert.IsTrue(_appSettings.DnsCache.Values.Single().IpAddresses.Single().ToString().StartsWith("192.168."));
        }

        [TestMethod]
        public async Task TestUpdateAsync_WhenCacheIsNull()
        {
            DnsResponse dnsResponse = new("host",
                TimeSpan.FromSeconds(12),
                new List<IpAddress> { new IpAddress(IPAddress.Parse("192.168.12.12")) });
            Assert.IsNull(_appSettings.DnsCache);

            await _dnsCacheManager.UpdateAsync(dnsResponse.Host, _ => dnsResponse);

            Assert.IsNull(_appSettings.DnsCache);
        }

        [TestMethod]
        public async Task TestUpdateAsync_WhenCacheContainsDifferentHost()
        {
            DnsResponse dnsResponse = new("host12",
                TimeSpan.FromSeconds(12),
                new List<IpAddress> { new IpAddress(IPAddress.Parse("192.168.12.12")) });

            DnsResponse cachedDnsResponse = new("host13",
                TimeSpan.FromSeconds(13),
                new List<IpAddress> { new IpAddress(IPAddress.Parse("192.168.13.13")) });
            _appSettings.DnsCache = new() { [cachedDnsResponse.Host] = cachedDnsResponse };
            
            await _dnsCacheManager.UpdateAsync(dnsResponse.Host, _ => dnsResponse);

            Assert.IsNotNull(_appSettings.DnsCache);
            Assert.IsTrue(_appSettings.DnsCache.ContainsKey(cachedDnsResponse.Host));
            Assert.AreEqual(cachedDnsResponse, _appSettings.DnsCache[cachedDnsResponse.Host]);
            Assert.IsFalse(_appSettings.DnsCache.ContainsKey(dnsResponse.Host));
        }

        [TestMethod]
        public async Task TestUpdateAsync_WhenCacheContainsSameHost()
        {
            DnsResponse dnsResponse = new("host",
                TimeSpan.FromSeconds(12),
                new List<IpAddress> { new IpAddress(IPAddress.Parse("192.168.12.12")) });

            DnsResponse cachedDnsResponse = new("host",
                TimeSpan.FromSeconds(13),
                new List<IpAddress> { new IpAddress(IPAddress.Parse("192.168.13.13")) });
            _appSettings.DnsCache = new() { [cachedDnsResponse.Host] = cachedDnsResponse };
            
            await _dnsCacheManager.UpdateAsync(dnsResponse.Host, _ => dnsResponse);

            Assert.IsNotNull(_appSettings.DnsCache);
            Assert.IsTrue(_appSettings.DnsCache.ContainsKey(dnsResponse.Host));
            Assert.AreEqual(dnsResponse, _appSettings.DnsCache[dnsResponse.Host]);
        }

        [TestMethod]
        public async Task TestUpdateAsync_WhenArgumentsAndCacheAreNull()
        {
            Assert.IsNull(_appSettings.DnsCache);

            await _dnsCacheManager.UpdateAsync(null, null);

            Assert.IsNull(_appSettings.DnsCache);
        }

        [TestMethod]
        public async Task TestUpdateAsync_WhenArgumentsAreNullAndCacheContainsDifferentHost()
        {
            DnsResponse cachedDnsResponse = new("host13",
                TimeSpan.FromSeconds(13),
                new List<IpAddress> { new IpAddress(IPAddress.Parse("192.168.13.13")) });
            _appSettings.DnsCache = new() { [cachedDnsResponse.Host] = cachedDnsResponse };

            await _dnsCacheManager.UpdateAsync(null, null);

            Assert.IsNotNull(_appSettings.DnsCache);
            Assert.IsTrue(_appSettings.DnsCache.ContainsKey(cachedDnsResponse.Host));
            Assert.AreEqual(cachedDnsResponse, _appSettings.DnsCache[cachedDnsResponse.Host]);
        }

        [TestMethod]
        public void TestUpdateAsync_ParallelWithDifferentHosts()
        {
            _appSettings.DnsCache = new();
            IList<DnsResponse> dnsResponses = new List<DnsResponse>();
            for (int i = 0; i < NUM_OF_PARALLEL_OPERATIONS; i++)
            {
                DnsResponse cachedDnsResponse = new($"host{i}",
                    TimeSpan.FromSeconds(1000 + i),
                    new List<IpAddress> { new IpAddress(IPAddress.Parse($"172.16.{i}.{i}")) });
                _appSettings.DnsCache.TryAdd(cachedDnsResponse.Host, cachedDnsResponse);
                
                DnsResponse dnsResponse = new(cachedDnsResponse.Host,
                    TimeSpan.FromSeconds(100 + i),
                    new List<IpAddress> { new IpAddress(IPAddress.Parse($"192.168.{i}.{i}")) });
                dnsResponses.Add(dnsResponse);
            }
            Assert.AreEqual(NUM_OF_PARALLEL_OPERATIONS, _appSettings.DnsCache.Count);
            foreach (DnsResponse cachedDnsResponse in _appSettings.DnsCache.Values)
            {
                DnsResponse newDnsResponse = dnsResponses.Single(dr => dr.Host == cachedDnsResponse.Host);
                Assert.AreNotEqual(cachedDnsResponse, newDnsResponse);
                Assert.AreNotEqual(cachedDnsResponse.TimeToLive, newDnsResponse.TimeToLive);
                Assert.AreNotEqual(cachedDnsResponse.IpAddresses, newDnsResponse.IpAddresses);
            }
            
            IList<Task<DnsResponse>> tasks = new List<Task<DnsResponse>>();
            foreach (DnsResponse dnsResponse in dnsResponses)
            {
                tasks.Add(_dnsCacheManager.UpdateAsync(dnsResponse.Host, _ => dnsResponse));
            }

            Task.WaitAll(tasks.ToArray());

            Assert.AreEqual(NUM_OF_PARALLEL_OPERATIONS, _appSettings.DnsCache.Count);
            foreach (DnsResponse dnsResponse in dnsResponses)
            {
                Assert.IsTrue(_appSettings.DnsCache.ContainsKey(dnsResponse.Host));
                Assert.AreEqual(dnsResponse, _appSettings.DnsCache[dnsResponse.Host]);
            }
        }

        [TestMethod]
        public void TestUpdateAsync_ParallelWithSameHost()
        {
            DnsResponse cachedDnsResponse = new("host",
                TimeSpan.FromSeconds(13),
                new List<IpAddress> { new IpAddress(IPAddress.Parse("172.16.1.1")) });
            _appSettings.DnsCache = new() { [cachedDnsResponse.Host] = cachedDnsResponse };
            
            IList<DnsResponse> dnsResponses = new List<DnsResponse>();
            for (int i = 0; i < NUM_OF_PARALLEL_OPERATIONS; i++)
            {
                dnsResponses.Add(new DnsResponse("host",
                    TimeSpan.FromSeconds(100 + i),
                    new List<IpAddress> { new IpAddress(IPAddress.Parse($"192.168.{i}.{i}")) }));
            }
            Assert.IsNotNull(_appSettings.DnsCache);
            Assert.AreEqual(1, _appSettings.DnsCache.Count);
            Assert.AreEqual(cachedDnsResponse, _appSettings.DnsCache.Values.Single());
            
            IList<Task<DnsResponse>> tasks = new List<Task<DnsResponse>>();
            foreach (DnsResponse dnsResponse in dnsResponses)
            {
                tasks.Add(_dnsCacheManager.UpdateAsync(dnsResponse.Host, _ => dnsResponse));
            }

            Task.WaitAll(tasks.ToArray());

            Assert.IsNotNull(_appSettings.DnsCache);
            Assert.AreEqual(1, _appSettings.DnsCache.Count);
            Assert.IsTrue(_appSettings.DnsCache.ContainsKey("host"));
            Assert.AreEqual("host", _appSettings.DnsCache["host"].Host);
            Assert.IsTrue(_appSettings.DnsCache.Values.Single().IpAddresses.Single().ToString().StartsWith("192.168."));
        }
    }
}