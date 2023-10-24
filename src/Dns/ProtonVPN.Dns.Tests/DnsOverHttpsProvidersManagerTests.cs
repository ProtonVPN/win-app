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
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Dns.Caching;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.Contracts.Resolvers;
using ProtonVPN.Dns.Tests.Mocks;

namespace ProtonVPN.Dns.Tests;

[TestClass]
public class DnsOverHttpsProvidersManagerTests
{
    private const string HOST = "dns.protonvpn.ch";
    private const string DIFFERENT_HOST = "differentdns.protonvpn.ch";
    private static readonly TimeSpan FAILED_DNS_REQUEST_TIMEOUT = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan NEW_TTL_ON_RESOLVE_ERROR = TimeSpan.FromMinutes(10);

    private MockOfLogger _logger;
    private CancellationTokenSource _cancellationTokenSource;
    private IDnsOverUdpResolver _dnsOverUdpResolver;
    private ISettings _settings;
    private IConfiguration _config;
    private IDnsCacheManager _dnsCacheManager;
    private DnsOverHttpsProvidersManager _dnsOverHttpsProvidersManager;

    [TestInitialize]
    public void TestInitialize()
    {
        _logger = new MockOfLogger();
        _cancellationTokenSource = new CancellationTokenSource();
        _dnsOverUdpResolver = Substitute.For<IDnsOverUdpResolver>();
        _settings = Substitute.For<ISettings>();
        _config = Substitute.For<IConfiguration>();
        _config.FailedDnsRequestTimeout.Returns(FAILED_DNS_REQUEST_TIMEOUT);
        _config.NewCacheTimeToLiveOnResolveError.Returns(NEW_TTL_ON_RESOLVE_ERROR);
        _dnsCacheManager = new MockOfDnsCacheManager(_settings);
        _dnsOverHttpsProvidersManager = new DnsOverHttpsProvidersManager(_dnsOverUdpResolver,
            _settings, _config, _logger, _dnsCacheManager);
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
        _settings = null;
        _config = null;
        _dnsOverHttpsProvidersManager = null;
    }

    [TestMethod]
    public async Task TestGetAsync_WhenNothingIsCachedAndAllFails()
    {
        IList<IpAddress> result = await _dnsOverHttpsProvidersManager.GetAsync(HOST, _cancellationTokenSource.Token);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task TestGetAsync_WhenSpecificHostIsNotCachedAndAllFails()
    {
        _settings.DnsCache = CreateDnsCache(CreateDnsResponse(DIFFERENT_HOST));

        IList<IpAddress> result = await _dnsOverHttpsProvidersManager.GetAsync(HOST, _cancellationTokenSource.Token);

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
        _settings.DnsCache = CreateDnsCache(CreateDnsResponse(HOST));

        IList<IpAddress> result = await _dnsOverHttpsProvidersManager.GetAsync(HOST, _cancellationTokenSource.Token);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(0, _logger.Logs.Count);
        await AssertResolverWasNotCalledAsync();
    }

    private void InitializeDnsOverUdpResolver()
    {
        _dnsOverUdpResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(CreateDnsResolverResponse);
    }

    private async Task AssertResolverWasNotCalledAsync()
    {
        await _dnsOverUdpResolver.Received(0).ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task TestGetAsync_WhenNothingIsCachedAndResolveSucceeds()
    {
        DateTime testStartDateTimeUtc = DateTime.UtcNow;
        InitializeDnsOverUdpResolver();
        Assert.AreEqual(0, _logger.Logs.Count);

        IList<IpAddress> result = await _dnsOverHttpsProvidersManager.GetAsync(HOST, _cancellationTokenSource.Token);

        AssertCacheAfterSuccessfulResolve(result, testStartDateTimeUtc);
        await AssertResolverWasCalledAsync();
    }

    private async Task AssertResolverWasCalledAsync()
    {
        await _dnsOverUdpResolver.Received(1).ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dnsOverUdpResolver.Received(1).ResolveAsync(HOST, _cancellationTokenSource.Token);
    }

    [TestMethod]
    public async Task TestGetAsync_WhenHasExpiredCacheAndResolveSucceeds()
    {
        DateTime testStartDateTimeUtc = DateTime.UtcNow;
        InitializeDnsOverUdpResolver();
        _settings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
        AssertCacheBeforeExecution();

        IList<IpAddress> result = await _dnsOverHttpsProvidersManager.GetAsync(HOST, _cancellationTokenSource.Token);

        AssertCacheAfterSuccessfulResolve(result, testStartDateTimeUtc);
        await AssertResolverWasCalledAsync();
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
        Assert.AreEqual("192.168.2.1", _settings.DnsCache[HOST].IpAddresses[0].ToString());
        Assert.AreEqual("192.168.2.2", _settings.DnsCache[HOST].IpAddresses[1].ToString());
        Assert.AreEqual(TimeSpan.FromMinutes(15), _settings.DnsCache[HOST].TimeToLive);
    }

    private void AssertCacheAfterSuccessfulResolve(IList<IpAddress> result, DateTime testStartDateTimeUtc)
    {
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("192.168.1.1", _settings.DnsCache[HOST].IpAddresses[0].ToString());
        Assert.AreEqual("192.168.1.2", _settings.DnsCache[HOST].IpAddresses[1].ToString());
        Assert.AreEqual(TimeSpan.FromMinutes(12), _settings.DnsCache[HOST].TimeToLive);
        Assert.IsTrue(_settings.DnsCache[HOST].ExpirationDateTimeUtc > DateTime.UtcNow);
        Assert.IsTrue(_settings.DnsCache[HOST].ResponseDateTimeUtc >= testStartDateTimeUtc);
        Assert.IsTrue(_settings.DnsCache[HOST].ResponseDateTimeUtc <= DateTime.UtcNow);
    }

    [TestMethod]
    public async Task TestGetAsync_WhenHasExpiredCacheAndResolveFails()
    {
        DateTime testStartDateTimeUtc = DateTime.UtcNow;
        _settings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
        AssertCacheBeforeExecution();

        IList<IpAddress> result = await _dnsOverHttpsProvidersManager.GetAsync(HOST, _cancellationTokenSource.Token);

        Assert.AreEqual(2, result.Count);
        AssertCacheAfterFailedResolve(testStartDateTimeUtc);
        await AssertResolverWasCalledAsync();
    }

    private void AssertCacheAfterFailedResolve(DateTime testStartDateTimeUtc)
    {
        Assert.AreEqual("192.168.2.1", _settings.DnsCache[HOST].IpAddresses[0].ToString());
        Assert.AreEqual("192.168.2.2", _settings.DnsCache[HOST].IpAddresses[1].ToString());
        Assert.AreEqual(NEW_TTL_ON_RESOLVE_ERROR, _settings.DnsCache[HOST].TimeToLive);
        Assert.IsTrue(_settings.DnsCache[HOST].ExpirationDateTimeUtc > DateTime.UtcNow);
        Assert.IsTrue(_settings.DnsCache[HOST].ResponseDateTimeUtc >= testStartDateTimeUtc);
        Assert.IsTrue(_settings.DnsCache[HOST].ResponseDateTimeUtc <= DateTime.UtcNow);
    }

    [TestMethod]
    public async Task TestGetAsync_WhenHasExpiredCacheAndResolveThrows()
    {
        DateTime testStartDateTimeUtc = DateTime.UtcNow;
        _dnsOverUdpResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsForAnyArgs(new Exception("Injected error for testing."));
        _settings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
        AssertCacheBeforeExecution();

        IList<IpAddress> result = await _dnsOverHttpsProvidersManager.GetAsync(HOST, _cancellationTokenSource.Token);

        Assert.AreEqual(2, result.Count);
        AssertCacheAfterFailedResolve(testStartDateTimeUtc);
        await AssertResolverWasCalledAsync();
    }

    [TestMethod]
    public async Task TestGetAsync_WhenNothingIsCachedAndResolveThrows()
    {
        _dnsOverUdpResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsForAnyArgs(new Exception("Injected error for testing."));

        IList<IpAddress> result = await _dnsOverHttpsProvidersManager.GetAsync(HOST, _cancellationTokenSource.Token);

        Assert.AreEqual(0, result.Count);
        await AssertResolverWasCalledAsync();
    }

    [TestMethod]
    public async Task TestGetAsync_UsesCacheOnSecondRequest_WhenFirstRequestHasNothingIsCached()
    {
        DateTime testStartDateTimeUtc = DateTime.UtcNow;
        InitializeDnsOverUdpResolver();

        IList<IpAddress> result1 = await _dnsOverHttpsProvidersManager.GetAsync(HOST, _cancellationTokenSource.Token);

        AssertCacheAfterSuccessfulResolve(result1, testStartDateTimeUtc);
        await AssertResolverWasCalledAsync();

        IList<IpAddress> result2 = await _dnsOverHttpsProvidersManager.GetAsync(HOST, _cancellationTokenSource.Token);

        AssertCacheAfterSuccessfulResolve(result2, testStartDateTimeUtc);
        await AssertResolverWasCalledAsync();
    }

    [TestMethod]
    public async Task TestGetAsync_UsesCacheOnSecondRequest_WhenFirstRequestHasExpiredCache()
    {
        DateTime testStartDateTimeUtc = DateTime.UtcNow;
        InitializeDnsOverUdpResolver();
        _settings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
        AssertCacheBeforeExecution();

        IList<IpAddress> result1 = await _dnsOverHttpsProvidersManager.GetAsync(HOST, _cancellationTokenSource.Token);

        AssertCacheAfterSuccessfulResolve(result1, testStartDateTimeUtc);
        await AssertResolverWasCalledAsync();

        IList<IpAddress> result2 = await _dnsOverHttpsProvidersManager.GetAsync(HOST, _cancellationTokenSource.Token);

        AssertCacheAfterSuccessfulResolve(result2, testStartDateTimeUtc);
        await AssertResolverWasCalledAsync();
    }

    [TestMethod]
    public async Task TestResolveWithoutCacheAsync_WhenAllFails()
    {
        _settings.DnsCache = CreateDnsCache(CreateDnsResponse(HOST));

        IList<IpAddress> result = await _dnsOverHttpsProvidersManager.ResolveWithoutCacheAsync(HOST, _cancellationTokenSource.Token);

        Assert.AreEqual(0, result.Count);
        await AssertResolverWasCalledAsync();
    }

    [TestMethod]
    public async Task TestResolveWithoutCacheAsync_WhenResolveSucceeds()
    {
        DateTime testStartDateTimeUtc = DateTime.UtcNow;
        InitializeDnsOverUdpResolver();
        Assert.AreEqual(0, _logger.Logs.Count);

        IList<IpAddress> result = await _dnsOverHttpsProvidersManager.ResolveWithoutCacheAsync(HOST, _cancellationTokenSource.Token);

        AssertCacheAfterSuccessfulResolve(result, testStartDateTimeUtc);
        await AssertResolverWasCalledAsync();
    }

    [TestMethod]
    public async Task TestResolveWithoutCacheAsync_WhendResolveThrows()
    {
        _dnsOverUdpResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsForAnyArgs(new Exception("Injected error for testing."));

        IList<IpAddress> result = await _dnsOverHttpsProvidersManager.ResolveWithoutCacheAsync(HOST, _cancellationTokenSource.Token);

        Assert.AreEqual(0, result.Count);
        await AssertResolverWasCalledAsync();
    }

    [TestMethod]
    public async Task TestGetFromCache_WithFreshCache()
    {
        _settings.DnsCache = CreateDnsCache(CreateDnsResponse(HOST));
        IList<IpAddress> expectedIpAddresses = _settings.DnsCache[HOST].IpAddresses;

        IList<IpAddress> result = _dnsOverHttpsProvidersManager.GetFromCache(HOST);

        Assert.AreEqual(expectedIpAddresses.Count, result.Count);
        foreach (IpAddress ipAddress in result)
        {
            expectedIpAddresses.Contains(ipAddress);
        }
        await AssertResolverWasNotCalledAsync();
    }

    [TestMethod]
    public async Task TestGetFromCache_WithExpiredCache()
    {
        _settings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(HOST));
        AssertCacheBeforeExecution();

        IList<IpAddress> result = _dnsOverHttpsProvidersManager.GetFromCache(HOST);

        Assert.AreEqual(2, result.Count);
        AssertCacheBeforeExecution();
        await AssertResolverWasNotCalledAsync();
    }

    [TestMethod]
    public async Task TestGetFromCache_WithNoCache()
    {
        AssertCacheIsEmpty();

        IList<IpAddress> result = _dnsOverHttpsProvidersManager.GetFromCache(HOST);

        Assert.AreEqual(0, result.Count);
        AssertCacheIsEmpty();
        await AssertResolverWasNotCalledAsync();
    }

    private void AssertCacheIsEmpty()
    {
        Assert.IsTrue(_settings.DnsCache is null || !_settings.DnsCache.ContainsKey(HOST));
    }

    [TestMethod]
    public async Task TestGetFromCache_WithNoCachedHost()
    {
        _settings.DnsCache = CreateDnsCache(CreateExpiredDnsResponse(DIFFERENT_HOST));
        AssertCacheIsEmpty();

        IList<IpAddress> result = _dnsOverHttpsProvidersManager.GetFromCache(HOST);

        Assert.AreEqual(0, result.Count);
        AssertCacheIsEmpty();
        await AssertResolverWasNotCalledAsync();
    }

    [TestMethod]
    public async Task TestGetAsync_WithParallelCallsToForceRaceCondition()
    {
        DateTime testStartDateTimeUtc = DateTime.UtcNow;
        _dnsOverUdpResolver.ResolveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(DelayedDnsResolverResponseAsync);
        Assert.AreEqual(0, _logger.Logs.Count);

        Task<IList<IpAddress>> task1 = _dnsOverHttpsProvidersManager.GetAsync(HOST, _cancellationTokenSource.Token);
        Task<IList<IpAddress>> task2 = _dnsOverHttpsProvidersManager.GetAsync(HOST, _cancellationTokenSource.Token);

        IList<IpAddress> result1 = await task1;
        IList<IpAddress> result2 = await task2;

        AssertCacheAfterSuccessfulResolve(result1, testStartDateTimeUtc);
        AssertCacheAfterSuccessfulResolve(result2, testStartDateTimeUtc);
        await AssertResolverWasCalledAsync();
    }

    private async Task<DnsResponse> DelayedDnsResolverResponseAsync(CallInfo arg)
    {
        await Task.Delay(3000);
        string host = arg.ArgAt<string>(0);
        return CreateDnsResponse(host);
    }
}