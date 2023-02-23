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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Settings;
using ProtonVPN.Dns.Caching;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.Contracts.Resolvers;
using ProtonVPN.Dns.Tests.Mocks;

namespace ProtonVPN.Dns.Tests
{
    [TestClass]
    public class DnsManagerTest
    {
        private const string HOST = "api.protonvpn.ch";
        private const string DIFFERENT_HOST = "differentapi.protonvpn.ch";
        private static readonly TimeSpan FAILED_DNS_REQUEST_TIMEOUT = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan NEW_TTL_ON_RESOLVE_ERROR = TimeSpan.FromMinutes(10);

        private MockOfLogger _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private IDnsOverUdpResolver _dnsOverUdpResolver;
        private IDnsOverHttpsResolver _dnsOverHttpsResolver;
        private IAppSettings _appSettings;
        private IConfiguration _configuration;
        private IDnsCacheManager _dnsCacheManager;
        private DnsManager _dnsManager;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = new MockOfLogger();
            _cancellationTokenSource = new CancellationTokenSource();
            _dnsOverUdpResolver = Substitute.For<IDnsOverUdpResolver>();
            _dnsOverHttpsResolver = Substitute.For<IDnsOverHttpsResolver>();
            _appSettings = Substitute.For<IAppSettings>();
            _configuration = Substitute.For<IConfiguration>();
            _configuration.FailedDnsRequestTimeout.Returns(FAILED_DNS_REQUEST_TIMEOUT);
            _configuration.NewCacheTimeToLiveOnResolveError.Returns(NEW_TTL_ON_RESOLVE_ERROR);
            _dnsCacheManager = new MockOfDnsCacheManager(_appSettings);
            _dnsManager = new DnsManager(_dnsOverUdpResolver, _dnsOverHttpsResolver,
                _appSettings, _configuration, _logger, _dnsCacheManager);
        }

        private DnsResponse CreateDnsResolverResponse(CallInfo arg)
        {
            string host = arg.ArgAt<string>(0);
            return CreateDnsResponse(host);
        }

        private DnsResponse CreateDnsResponse(string host)
        {
            return new DnsResponse(host, TimeSpan.FromMinutes(12),
                new List<IpAddress>()
                {
                    new IpAddress(IPAddress.Parse("192.168.1.1")),
                    new IpAddress(IPAddress.Parse("192.168.1.2"))
                });
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _logger = null;
            _cancellationTokenSource = null;
            _dnsOverUdpResolver = null;
            _dnsOverHttpsResolver = null;
            _appSettings = null;
            _configuration = null;
            _dnsManager = null;
        }

        [TestMethod]
        public async Task TestGetAsync_WhenNothingIsCachedAndAllFails()
        {
            IList<IpAddress> result = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task TestGetAsync_WhenSpecificHostIsNotCachedAndAllFails()
        {
            _appSettings.DnsCache = CreateDnsCache(CreateDnsResponse(DIFFERENT_HOST));

            IList<IpAddress> result = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            Assert.AreEqual(0, result.Count);
        }

        private ConcurrentDictionary<string, DnsResponse> CreateDnsCache(params DnsResponse[] dnsResponses)
        {
            ConcurrentDictionary<string, DnsResponse> dictionary = new();
            foreach (DnsResponse dnsResponse in dnsResponses)
            {
                dictionary.TryAdd(dnsResponse.Host, dnsResponse);
            }
            return dictionary;
        }

        [TestMethod]
        public async Task TestGetAsync_WhenHasFreshCache()
        {
            InitializeDnsOverUdpResolver();
            InitializeDnsOverHttpsResolver();
            _appSettings.DnsCache = CreateDnsCache(CreateDnsResponse(HOST));

            IList<IpAddress> result = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(0, _logger.Logs.Count);
            await AssertNoResolverWasCalledAsync();
        }

        private void InitializeDnsOverUdpResolver()
        {
            _dnsOverUdpResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(CreateDnsResolverResponse);
        }

        private void InitializeDnsOverHttpsResolver()
        {
            _dnsOverHttpsResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(CreateDnsResolverResponse);
        }

        private async Task AssertNoResolverWasCalledAsync()
        {
            await _dnsOverUdpResolver.Received(0).ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            await AssertHttpsResolverWasNotCalledAsync();
        }

        private async Task AssertHttpsResolverWasNotCalledAsync()
        {
            await _dnsOverHttpsResolver.Received(0).ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task TestGetAsync_WhenNothingIsCachedAndUdpResolveSucceeds()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            InitializeDnsOverUdpResolver();
            InitializeDnsOverHttpsResolver();
            Assert.AreEqual(0, _logger.Logs.Count);

            IList<IpAddress> result = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result, testStartDateTimeUtc);
            await AssertUdpResolverWasCalledAsync();
            await AssertHttpsResolverWasNotCalledAsync();
        }

        private async Task AssertUdpResolverWasCalledAsync()
        {
            await _dnsOverUdpResolver.Received(1).ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            await _dnsOverUdpResolver.Received(1).ResolveAsync(HOST, _cancellationTokenSource.Token);
        }

        [TestMethod]
        public async Task TestGetAsync_WhenNothingIsCachedAndHttpsResolveSucceeds()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            InitializeDnsOverHttpsResolver();
            Assert.AreEqual(0, _logger.Logs.Count);

            IList<IpAddress> result = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result, testStartDateTimeUtc);
            await AssertCalledBothResolversOnceAsync();
        }

        [TestMethod]
        public async Task TestGetAsync_WhenHasExpiredCacheAndUdpResolveSucceeds()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            InitializeDnsOverUdpResolver();
            InitializeDnsOverHttpsResolver();
            _appSettings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
            AssertCacheBeforeExecution();

            IList<IpAddress> result = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result, testStartDateTimeUtc);
            await AssertUdpResolverWasCalledAsync();
            await AssertHttpsResolverWasNotCalledAsync();
        }

        private DnsResponse CreateExpiredDnsResponse(string host)
        {
            return new DnsResponse(host, TimeSpan.FromMinutes(15),
                new List<IpAddress>()
                {
                    new IpAddress(IPAddress.Parse("192.168.2.1")),
                    new IpAddress(IPAddress.Parse("192.168.2.2"))
                }, DateTime.UtcNow.AddMinutes(-16));
        }

        private void AssertCacheBeforeExecution()
        {
            Assert.AreEqual("192.168.2.1", _appSettings.DnsCache[HOST].IpAddresses[0].ToString());
            Assert.AreEqual("192.168.2.2", _appSettings.DnsCache[HOST].IpAddresses[1].ToString());
            Assert.AreEqual(TimeSpan.FromMinutes(15), _appSettings.DnsCache[HOST].TimeToLive);
        }

        private void AssertCacheAfterSuccessfulResolve(IList<IpAddress> result, DateTime testStartDateTimeUtc)
        {
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("192.168.1.1", _appSettings.DnsCache[HOST].IpAddresses[0].ToString());
            Assert.AreEqual("192.168.1.2", _appSettings.DnsCache[HOST].IpAddresses[1].ToString());
            Assert.AreEqual(TimeSpan.FromMinutes(12), _appSettings.DnsCache[HOST].TimeToLive);
            Assert.IsTrue(_appSettings.DnsCache[HOST].ExpirationDateTimeUtc > DateTime.UtcNow);
            Assert.IsTrue(_appSettings.DnsCache[HOST].ResponseDateTimeUtc >= testStartDateTimeUtc);
            Assert.IsTrue(_appSettings.DnsCache[HOST].ResponseDateTimeUtc <= DateTime.UtcNow);
        }

        [TestMethod]
        public async Task TestGetAsync_WhenHasExpiredCacheAndHttpsResolveSucceeds()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            InitializeDnsOverHttpsResolver();
            _appSettings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
            AssertCacheBeforeExecution();

            IList<IpAddress> result = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result, testStartDateTimeUtc);
            await AssertCalledBothResolversOnceAsync();
        }

        private async Task AssertCalledBothResolversOnceAsync()
        {
            await AssertUdpResolverWasCalledAsync();
            await _dnsOverHttpsResolver.Received(1).ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            await _dnsOverHttpsResolver.Received(1).ResolveAsync(HOST, _cancellationTokenSource.Token);
        }

        [TestMethod]
        public async Task TestGetAsync_WhenHasExpiredCacheAndResolvesFail()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            _appSettings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
            AssertCacheBeforeExecution();

            IList<IpAddress> result = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            Assert.AreEqual(2, result.Count);
            AssertCacheAfterFailedResolve(testStartDateTimeUtc);
            await AssertCalledBothResolversOnceAsync();
        }

        private void AssertCacheAfterFailedResolve(DateTime testStartDateTimeUtc)
        {
            Assert.AreEqual("192.168.2.1", _appSettings.DnsCache[HOST].IpAddresses[0].ToString());
            Assert.AreEqual("192.168.2.2", _appSettings.DnsCache[HOST].IpAddresses[1].ToString());
            Assert.AreEqual(NEW_TTL_ON_RESOLVE_ERROR, _appSettings.DnsCache[HOST].TimeToLive);
            Assert.IsTrue(_appSettings.DnsCache[HOST].ExpirationDateTimeUtc > DateTime.UtcNow);
            Assert.IsTrue(_appSettings.DnsCache[HOST].ResponseDateTimeUtc >= testStartDateTimeUtc);
            Assert.IsTrue(_appSettings.DnsCache[HOST].ResponseDateTimeUtc <= DateTime.UtcNow);
        }

        [TestMethod]
        public async Task TestGetAsync_WhenHasExpiredCacheAndUdpResolveThrows()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            _dnsOverUdpResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .ThrowsForAnyArgs(new Exception("Injected error for testing."));
            _appSettings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
            AssertCacheBeforeExecution();

            IList<IpAddress> result = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            Assert.AreEqual(2, result.Count);
            AssertCacheAfterFailedResolve(testStartDateTimeUtc);
            await AssertCalledBothResolversOnceAsync();
        }

        [TestMethod]
        public async Task TestGetAsync_WhenHasExpiredCacheAndHttpsResolveThrows()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            _dnsOverHttpsResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .ThrowsForAnyArgs(new Exception("Injected error for testing."));
            _appSettings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
            AssertCacheBeforeExecution();

            IList<IpAddress> result = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            Assert.AreEqual(2, result.Count);
            AssertCacheAfterFailedResolve(testStartDateTimeUtc);
            await AssertCalledBothResolversOnceAsync();
        }

        [TestMethod]
        public async Task TestGetAsync_WhenNothingIsCachedAndUdpResolveThrows()
        {
            _dnsOverUdpResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .ThrowsForAnyArgs(new Exception("Injected error for testing."));

            IList<IpAddress> result = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            Assert.AreEqual(0, result.Count);
            await AssertCalledBothResolversOnceAsync();
        }

        [TestMethod]
        public async Task TestGetAsync_WhenNothingIsCachedAndHttpsResolveThrows()
        {
            _dnsOverHttpsResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .ThrowsForAnyArgs(new Exception("Injected error for testing."));

            IList<IpAddress> result = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            Assert.AreEqual(0, result.Count);
            await AssertCalledBothResolversOnceAsync();
        }

        [TestMethod]
        public async Task TestGetAsync_UsesCacheOnSecondRequest_WhenFirstRequestHasNothingIsCached()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            InitializeDnsOverUdpResolver();
            InitializeDnsOverHttpsResolver();

            IList<IpAddress> result1 = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result1, testStartDateTimeUtc);
            await AssertUdpResolverWasCalledAsync();
            await AssertHttpsResolverWasNotCalledAsync();

            IList<IpAddress> result2 = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result2, testStartDateTimeUtc);
            await AssertUdpResolverWasCalledAsync();
            await AssertHttpsResolverWasNotCalledAsync();
        }

        [TestMethod]
        public async Task TestGetAsync_UsesCacheOnSecondRequest_WhenFirstRequestHasExpiredCache()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            InitializeDnsOverUdpResolver();
            InitializeDnsOverHttpsResolver();
            _appSettings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
            AssertCacheBeforeExecution();

            IList<IpAddress> result1 = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result1, testStartDateTimeUtc);
            await AssertUdpResolverWasCalledAsync();
            await AssertHttpsResolverWasNotCalledAsync();

            IList<IpAddress> result2 = await _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result2, testStartDateTimeUtc);
            await AssertUdpResolverWasCalledAsync();
            await AssertHttpsResolverWasNotCalledAsync();
        }

        [TestMethod]
        public async Task TestResolveWithoutCacheAsync_WhenAllFails()
        {
            _appSettings.DnsCache = CreateDnsCache(CreateDnsResponse(HOST));

            IList<IpAddress> result = await _dnsManager.ResolveWithoutCacheAsync(HOST, _cancellationTokenSource.Token);

            Assert.AreEqual(0, result.Count);
            await AssertCalledBothResolversOnceAsync();
        }

        [TestMethod]
        public async Task TestResolveWithoutCacheAsync_WhenHttpsResolveSucceeds()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            InitializeDnsOverHttpsResolver();
            Assert.AreEqual(0, _logger.Logs.Count);

            IList<IpAddress> result = await _dnsManager.ResolveWithoutCacheAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result, testStartDateTimeUtc);
            await AssertCalledBothResolversOnceAsync();
        }

        [TestMethod]
        public async Task TestResolveWithoutCacheAsync_WhenHttpsResolveThrows()
        {
            _dnsOverHttpsResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .ThrowsForAnyArgs(new Exception("Injected error for testing."));

            IList<IpAddress> result = await _dnsManager.ResolveWithoutCacheAsync(HOST, _cancellationTokenSource.Token);

            Assert.AreEqual(0, result.Count);
            await AssertCalledBothResolversOnceAsync();
        }

        [TestMethod]
        public async Task TestResolveWithoutCacheAsync_WhenUdpResolveSucceeds()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            InitializeDnsOverUdpResolver();
            InitializeDnsOverHttpsResolver();
            Assert.AreEqual(0, _logger.Logs.Count);

            IList<IpAddress> result = await _dnsManager.ResolveWithoutCacheAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result, testStartDateTimeUtc);
            await AssertUdpResolverWasCalledAsync();
            await AssertHttpsResolverWasNotCalledAsync();
        }

        [TestMethod]
        public async Task TestResolveWithoutCacheAsync_WhendUdpResolveThrows()
        {
            _dnsOverUdpResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .ThrowsForAnyArgs(new Exception("Injected error for testing."));

            IList<IpAddress> result = await _dnsManager.ResolveWithoutCacheAsync(HOST, _cancellationTokenSource.Token);

            Assert.AreEqual(0, result.Count);
            await AssertCalledBothResolversOnceAsync();
        }

        [TestMethod]
        public async Task TestGetFromCache_WithFreshCache()
        {
            _appSettings.DnsCache = CreateDnsCache(CreateDnsResponse(HOST));
            IList<IpAddress> expectedIpAddresses = _appSettings.DnsCache[HOST].IpAddresses;

            IList<IpAddress> result = _dnsManager.GetFromCache(HOST);

            Assert.AreEqual(expectedIpAddresses.Count, result.Count);
            foreach (IpAddress ipAddress in result)
            {
                expectedIpAddresses.Contains(ipAddress);
            }
            await AssertNoResolverWasCalledAsync();
        }

        [TestMethod]
        public async Task TestGetFromCache_WithExpiredCache()
        {
            _appSettings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
            AssertCacheBeforeExecution();

            IList<IpAddress> result = _dnsManager.GetFromCache(HOST);

            Assert.AreEqual(2, result.Count);
            AssertCacheBeforeExecution();
            await AssertNoResolverWasCalledAsync();
        }

        [TestMethod]
        public async Task TestGetFromCache_WithNoCache()
        {
            AssertCacheIsEmpty();

            IList<IpAddress> result = _dnsManager.GetFromCache(HOST);

            Assert.AreEqual(0, result.Count);
            AssertCacheIsEmpty();
            await AssertNoResolverWasCalledAsync();
        }

        private void AssertCacheIsEmpty()
        {
            Assert.IsTrue(_appSettings.DnsCache is null || !_appSettings.DnsCache.ContainsKey(HOST));
        }

        [TestMethod]
        public async Task TestGetFromCache_WithNoCachedHost()
        {
            _appSettings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(DIFFERENT_HOST));
            AssertCacheIsEmpty();

            IList<IpAddress> result = _dnsManager.GetFromCache(HOST);

            Assert.AreEqual(0, result.Count);
            AssertCacheIsEmpty();
            await AssertNoResolverWasCalledAsync();
        }

        [TestMethod]
        public async Task TestGetAsync_WithParallelCallsToForceRaceCondition()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            _dnsOverUdpResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(DelayedDnsResolverResponseAsync);
            Assert.AreEqual(0, _logger.Logs.Count);

            Task<IList<IpAddress>> task1 = _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);
            Task<IList<IpAddress>> task2 = _dnsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            IList<IpAddress> result1 = await task1;
            IList<IpAddress> result2 = await task2;

            AssertCacheAfterSuccessfulResolve(result1, testStartDateTimeUtc);
            AssertCacheAfterSuccessfulResolve(result2, testStartDateTimeUtc);
            await AssertUdpResolverWasCalledAsync();
            await AssertHttpsResolverWasNotCalledAsync();
        }

        private async Task<DnsResponse> DelayedDnsResolverResponseAsync(CallInfo arg)
        {
            await Task.Delay(3000);
            string host = arg.ArgAt<string>(0);
            return CreateDnsResponse(host);
        }
    }
}