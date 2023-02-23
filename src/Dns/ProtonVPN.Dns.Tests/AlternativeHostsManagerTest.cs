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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Core.Settings;
using ProtonVPN.Dns.Caching;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.Contracts.Resolvers;
using ProtonVPN.Dns.Tests.Mocks;

namespace ProtonVPN.Dns.Tests
{
    [TestClass]
    public class AlternativeHostsManagerTest
    {
        private const string HOST = "dMFYGSLTQOJXXI33OOZYG4LTDNA.protonpro.xyz";
        private const string DIFFERENT_HOST = "api.protonvpn.ch";
        private static readonly TimeSpan FAILED_DNS_REQUEST_TIMEOUT = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan NEW_TTL_ON_RESOLVE_ERROR = TimeSpan.FromMinutes(10);

        private MockOfLogger _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private IDnsOverHttpsTxtRecordsResolver _dnsOverHttpsTxtRecordsResolver;
        private IAppSettings _appSettings;
        private IConfiguration _configuration;
        private IDnsCacheManager _dnsCacheManager;
        private AlternativeHostsManager _alternativeHostsManager;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = new MockOfLogger();
            _cancellationTokenSource = new CancellationTokenSource();
            _dnsOverHttpsTxtRecordsResolver = Substitute.For<IDnsOverHttpsTxtRecordsResolver>();
            _appSettings = Substitute.For<IAppSettings>();
            _configuration = Substitute.For<IConfiguration>();
            _configuration.FailedDnsRequestTimeout.Returns(FAILED_DNS_REQUEST_TIMEOUT);
            _configuration.NewCacheTimeToLiveOnResolveError.Returns(NEW_TTL_ON_RESOLVE_ERROR);
            _dnsCacheManager = new MockOfDnsCacheManager(_appSettings);
            _alternativeHostsManager = new AlternativeHostsManager(_dnsOverHttpsTxtRecordsResolver,
                _appSettings, _configuration, _logger, _dnsCacheManager);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _logger = null;
            _cancellationTokenSource = null;
            _dnsOverHttpsTxtRecordsResolver = null;
            _appSettings = null;
            _configuration = null;
            _dnsCacheManager = null;
            _alternativeHostsManager = null;
        }

        [TestMethod]
        public async Task TestGetAsync_WhenNothingIsCachedAndAllFails()
        {
            IList<string> result = await _alternativeHostsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task TestGetAsync_WhenSpecificHostIsNotCachedAndAllFails()
        {
            _appSettings.DnsCache = CreateDnsCache(CreateDnsResponse(DIFFERENT_HOST));

            IList<string> result = await _alternativeHostsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            Assert.AreEqual(0, result.Count);
        }

        private DnsResponse CreateDnsResponse(string host)
        {
            return new DnsResponse(host, TimeSpan.FromMinutes(12), GetAlternativeHosts());
        }

        private IList<string> GetAlternativeHosts()
        {
            return new List<string>()
            {
                "protonvpn.com",
                "proton.me",
                "protonstatus.com"
            };
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
            InitializeDnsOverHttpsTxtRecordsResolver();
            _appSettings.DnsCache = CreateDnsCache(CreateDnsResponse(HOST));

            IList<string> result = await _alternativeHostsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertResultEqualsCache(result);
            Assert.AreEqual(0, _logger.Logs.Count);
            await AssertNoResolverWasCalledAsync();
        }

        private void AssertResultEqualsCache(IList<string> result)
        {
            Assert.AreEqual(_appSettings.DnsCache[HOST].AlternativeHosts.Count, result.Count);
            foreach (string resultAlternativeHost in result)
            {
                Assert.IsTrue(_appSettings.DnsCache[HOST].AlternativeHosts.Contains(resultAlternativeHost));
            }
        }

        private void InitializeDnsOverHttpsTxtRecordsResolver()
        {
            _dnsOverHttpsTxtRecordsResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(CreateDnsResolverResponse);
        }

        private DnsResponse CreateDnsResolverResponse(CallInfo arg)
        {
            string host = arg.ArgAt<string>(0);
            return CreateDnsResponse(host);
        }

        private async Task AssertNoResolverWasCalledAsync()
        {
            await _dnsOverHttpsTxtRecordsResolver.Received(0).ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task TestGetAsync_WhenNothingIsCachedAndResolveSucceeds()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            InitializeDnsOverHttpsTxtRecordsResolver();
            Assert.AreEqual(0, _logger.Logs.Count);

            IList<string> result = await _alternativeHostsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result, testStartDateTimeUtc);
            await AssertCalledResolverOnceAsync();
        }

        private async Task AssertCalledResolverOnceAsync()
        {
            await _dnsOverHttpsTxtRecordsResolver.Received(1).ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            await _dnsOverHttpsTxtRecordsResolver.Received(1).ResolveAsync(HOST, _cancellationTokenSource.Token);
        }

        private void AssertCacheAfterSuccessfulResolve(IList<string> result, DateTime testStartDateTimeUtc)
        {
            AssertResultEqualsCache(result);
            Assert.AreEqual(TimeSpan.FromMinutes(12), _appSettings.DnsCache[HOST].TimeToLive);
            Assert.IsTrue(_appSettings.DnsCache[HOST].ExpirationDateTimeUtc > DateTime.UtcNow);
            Assert.IsTrue(_appSettings.DnsCache[HOST].ResponseDateTimeUtc >= testStartDateTimeUtc);
            Assert.IsTrue(_appSettings.DnsCache[HOST].ResponseDateTimeUtc <= DateTime.UtcNow);
        }

        [TestMethod]
        public async Task TestGetAsync_WhenHasExpiredCacheAndResolveSucceeds()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            InitializeDnsOverHttpsTxtRecordsResolver();
            _appSettings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
            AssertCacheBeforeExecution();

            IList<string> result = await _alternativeHostsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result, testStartDateTimeUtc);
            await AssertCalledResolverOnceAsync();
        }

        private DnsResponse CreateExpiredDnsResponse(string host)
        {
            return new DnsResponse(host, TimeSpan.FromMinutes(15), GetWrongAlternativeHosts(), DateTime.UtcNow.AddMinutes(-16));
        }

        private IList<string> GetWrongAlternativeHosts()
        {
            return new List<string>()
            {
                "protonvpn.com",
                "proton.me",
                "protonstatus.com"
            };
        }

        private void AssertCacheBeforeExecution()
        {
            AssertCacheAlternativeHosts();
            Assert.AreEqual(TimeSpan.FromMinutes(15), _appSettings.DnsCache[HOST].TimeToLive);
        }

        private void AssertCacheAlternativeHosts()
        {
            IList<string> wrongAlternativeHosts = GetWrongAlternativeHosts();
            Assert.AreEqual(wrongAlternativeHosts.Count, _appSettings.DnsCache[HOST].AlternativeHosts.Count);
            foreach (string wrongAlternativeHost in wrongAlternativeHosts)
            {
                Assert.IsTrue(_appSettings.DnsCache[HOST].AlternativeHosts.Contains(wrongAlternativeHost));
            }
        }

        [TestMethod]
        public async Task TestGetAsync_WhenHasExpiredCacheAndResolvesFail()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            _appSettings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
            AssertCacheBeforeExecution();

            IList<string> result = await _alternativeHostsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertResultEqualsCache(result);
            AssertCacheAfterFailedResolve(testStartDateTimeUtc);
            await AssertCalledResolverOnceAsync();
        }

        private void AssertCacheAfterFailedResolve(DateTime testStartDateTimeUtc)
        {
            AssertCacheAlternativeHosts();
            Assert.AreEqual(NEW_TTL_ON_RESOLVE_ERROR, _appSettings.DnsCache[HOST].TimeToLive);
            Assert.IsTrue(_appSettings.DnsCache[HOST].ExpirationDateTimeUtc > DateTime.UtcNow);
            Assert.IsTrue(_appSettings.DnsCache[HOST].ResponseDateTimeUtc >= testStartDateTimeUtc);
            Assert.IsTrue(_appSettings.DnsCache[HOST].ResponseDateTimeUtc <= DateTime.UtcNow);
        }

        [TestMethod]
        public async Task TestGetAsync_WhenHasExpiredCacheAndUdpResolveThrows()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            _dnsOverHttpsTxtRecordsResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .ThrowsForAnyArgs(new Exception("Injected error for testing."));
            _appSettings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
            AssertCacheBeforeExecution();

            IList<string> result = await _alternativeHostsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertResultEqualsCache(result);
            AssertCacheAfterFailedResolve(testStartDateTimeUtc);
            await AssertCalledResolverOnceAsync();
        }

        [TestMethod]
        public async Task TestGetAsync_WhenNothingIsCachedAndUdpResolveThrows()
        {
            _dnsOverHttpsTxtRecordsResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .ThrowsForAnyArgs(new Exception("Injected error for testing."));

            IList<string> result = await _alternativeHostsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            Assert.AreEqual(0, result.Count);
            await AssertCalledResolverOnceAsync();
        }

        [TestMethod]
        public async Task TestGetAsync_UsesCacheOnSecondRequest_WhenFirstRequestHasNothingIsCached()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            InitializeDnsOverHttpsTxtRecordsResolver();

            IList<string> result1 = await _alternativeHostsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result1, testStartDateTimeUtc);
            await AssertCalledResolverOnceAsync();

            IList<string> result2 = await _alternativeHostsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result2, testStartDateTimeUtc);
            await AssertCalledResolverOnceAsync();
        }

        [TestMethod]
        public async Task TestGetAsync_UsesCacheOnSecondRequest_WhenFirstRequestHasExpiredCache()
        {
            DateTime testStartDateTimeUtc = DateTime.UtcNow;
            InitializeDnsOverHttpsTxtRecordsResolver();
            _appSettings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
            AssertCacheBeforeExecution();

            IList<string> result1 = await _alternativeHostsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result1, testStartDateTimeUtc);
            await AssertCalledResolverOnceAsync();

            IList<string> result2 = await _alternativeHostsManager.GetAsync(HOST, _cancellationTokenSource.Token);

            AssertCacheAfterSuccessfulResolve(result2, testStartDateTimeUtc);
            await AssertCalledResolverOnceAsync();
        }
    }
}