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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.NameServers;
using ProtonVPN.Dns.Resolvers;
using ProtonVPN.Dns.Resolvers.System;
using ProtonVPN.Dns.Tests.Mocks;

namespace ProtonVPN.Dns.Tests.Resolvers;

[TestClass]
public class DnsOverUdpResolverTest
{
    private const string HOST = "api.protonvpn.ch";
    private const string NON_EXISTING_HOST = "g5f16gfds1gdsf5g16dsfg15fs5gfds651d61s651g6516gf1s6fdgfs.vhbverhu";

    private static readonly TimeSpan DNS_RESOLVE_TIMEOUT = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DEFAULT_DNS_TTL = TimeSpan.FromMinutes(20);

    private MockOfLogger _logger;
    private CancellationTokenSource _cancellationTokenSource;
    private Stopwatch _stopwatch;
    private IConfiguration _configuration;
    private MockOfNameServersLoader _mockOfNameServersLoader;
    private DnsOverUdpResolver _resolver;
    private ISystemDnsResolver _mockOfSystemDnsResolver;

    [TestInitialize]
    public void TestInitialize()
    {
        _logger = new MockOfLogger();
        _cancellationTokenSource = new CancellationTokenSource();
        _stopwatch = new Stopwatch();

        _configuration = Substitute.For<IConfiguration>();
        _configuration.DnsResolveTimeout.Returns(DNS_RESOLVE_TIMEOUT);
        _configuration.DefaultDnsTimeToLive.Returns(DEFAULT_DNS_TTL);

        _mockOfSystemDnsResolver = Substitute.For<ISystemDnsResolver>();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _logger = null;
        _cancellationTokenSource = null;
        _stopwatch = null;
        _configuration = null;
        _mockOfNameServersLoader = null;
        _resolver = null;
        _mockOfSystemDnsResolver = null;
    }

    [TestMethod]
    public async Task TestResolveAsync()
    {
        InitializeResolverWithRealNameServersLoader();

        DnsResponse response = await ExecuteAsync();

        Assert.IsTrue(response.IpAddresses.Count > 0);
        Assert.IsTrue(response.TimeToLive > TimeSpan.Zero);
        Assert.IsTrue(response.ExpirationDateTimeUtc > DateTime.UtcNow);
        Assert.IsTrue(response.ResponseDateTimeUtc <= DateTime.UtcNow);
        Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(5));
    }

    private void InitializeResolverWithRealNameServersLoader()
    {
        NameServersResolver nameServersResolver = new();
        NameServersLoader nameServersLoader = new(nameServersResolver, _logger);
        _resolver = new(nameServersLoader, _mockOfSystemDnsResolver, _configuration, _logger);
    }

    private async Task<DnsResponse> ExecuteAsync()
    {
        return await ExecuteWithStopwatchAsync(ResolveAsync);
    }

    private async Task<DnsResponse> ResolveAsync()
    {
        return await _resolver.ResolveAsync(HOST, _cancellationTokenSource.Token);
    }

    private async Task<DnsResponse> ExecuteWithStopwatchAsync(Func<Task<DnsResponse>> task)
    {
        _stopwatch.Start();
        DnsResponse response = await task();
        _stopwatch.Stop();
        return response;
    }

    [TestMethod]
    public async Task TestResolveAsync_WithoutNameServers_AndSystemResolverWorks()
    {
        InitializeWithMockOfNameServersLoader();
        List<IPAddress> systemResult = new() { IPAddress.Parse("1.2.3.4"), IPAddress.Parse("5.6.7.8") };
        AddSystemDnsResolverResult(HOST, systemResult);

        DnsResponse response = await ExecuteAsync();

        AssertValidSystemResponse(systemResult, response);
        Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(1));
    }

    private void AssertValidSystemResponse(List<IPAddress> expectedSystemResult, DnsResponse response)
    {
        Assert.IsNotNull(response);
        Assert.AreEqual(expectedSystemResult.Count, response.IpAddresses.Count);
        foreach (IPAddress ipAddress in expectedSystemResult)
        {
            Assert.IsNotNull(response.IpAddresses.Single(ip => ip.ToString() == ipAddress.ToString()));
        }
        Assert.AreEqual(TimeSpan.FromSeconds(DnsOverUdpResolver.DEFAULT_DNS_TTL_IN_SECONDS), response.TimeToLive);
        Assert.IsTrue(response.ExpirationDateTimeUtc > DateTime.UtcNow);
        Assert.IsTrue(response.ResponseDateTimeUtc <= DateTime.UtcNow);
        Assert.AreEqual(response.TimeToLive, response.ExpirationDateTimeUtc - response.ResponseDateTimeUtc);
    }

    private void InitializeWithMockOfNameServersLoader()
    {
        _mockOfNameServersLoader = new MockOfNameServersLoader();
        _resolver = new DnsOverUdpResolver(_mockOfNameServersLoader, _mockOfSystemDnsResolver, _configuration, _logger);
    }

    [TestMethod]
    public async Task TestResolveAsync_WithoutNameServers_AndSystemResolverFails()
    {
        InitializeWithMockOfNameServersLoader();
        AddSystemDnsResolverResult(HOST, new List<IPAddress>());

        DnsResponse response = await ExecuteAsync();

        Assert.AreEqual(null, response);
        Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public async Task TestResolveAsync_WithNonWorkingNameServers_AndSystemResolverWorks()
    {
        InitializeWithMockOfNameServersLoader();
        SetNonWorkingNameServers();
        List<IPAddress> systemResult = new() { IPAddress.Parse("8.8.8.8"), IPAddress.Parse("1.1.1.1") };
        AddSystemDnsResolverResult(HOST, systemResult);

        DnsResponse response = await ExecuteAsync();

        AssertValidSystemResponse(systemResult, response);
        Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(20));
    }

    private void SetNonWorkingNameServers()
    {
        _mockOfNameServersLoader.Set(IPAddress.Loopback, IPAddress.Parse("192.168.153.153"));
    }

    [TestMethod]
    public async Task TestResolveAsync_WithNonWorkingNameServers_AndSystemResolverFails()
    {
        InitializeWithMockOfNameServersLoader();
        SetNonWorkingNameServers();
        AddSystemDnsResolverResult(HOST, new List<IPAddress>());

        DnsResponse response = await ExecuteAsync();

        Assert.AreEqual(null, response);
        Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(20));
    }

    [TestMethod]
    public async Task TestResolveAsync_WhenCancelled()
    {
        InitializeWithMockOfNameServersLoader();
        SetNonWorkingNameServers();

        DnsResponse response = await ExecuteWithStopwatchAsync(StartResolveAndCancelAsync);

        Assert.AreEqual(null, response);
        Assert.IsTrue(_stopwatch.Elapsed > TimeSpan.FromSeconds(2) && _stopwatch.Elapsed < TimeSpan.FromSeconds(5));
    }

    private async Task<DnsResponse> StartResolveAndCancelAsync()
    {
        Task<DnsResponse> task = Task.Run(() => _resolver.ResolveAsync(HOST, _cancellationTokenSource.Token));
        Task.Delay(TimeSpan.FromSeconds(3)).ContinueWith(_ => _cancellationTokenSource.Cancel());
        return await task;
    }

    [TestMethod]
    public async Task TestResolveAsync_WithCancelledToken()
    {
        InitializeWithMockOfNameServersLoader();
        SetNonWorkingNameServers();

        _cancellationTokenSource.Cancel();
        DnsResponse response = await ExecuteAsync();

        Assert.AreEqual(null, response);
        Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public async Task TestResolveAsync_WithNonExistentHost()
    {
        InitializeResolverWithRealNameServersLoader();
        AddSystemDnsResolverResult(NON_EXISTING_HOST, []);

        DnsResponse response = await ExecuteWithStopwatchAndCustomHostAsync(NON_EXISTING_HOST);

        Assert.AreEqual(null, response);
        Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(20));
    }

    [TestMethod]
    public async Task TestResolveAsync_WithNonPublicIpAddresses()
    {
        InitializeResolverWithRealNameServersLoader();
        AddSystemDnsResolverResult(NON_EXISTING_HOST, [
            IPAddress.Parse("127.9.9.9"),
            IPAddress.Parse("10.2.0.1")
        ]);

        DnsResponse response = await ExecuteWithStopwatchAndCustomHostAsync(NON_EXISTING_HOST);

        Assert.AreEqual(null, response);
        Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(20));
    }

    private void AddSystemDnsResolverResult(string host, List<IPAddress> ipAddresses)
    {
        _mockOfSystemDnsResolver.ResolveWithSystemAsync(host, Arg.Any<CancellationToken>())
                                .ReturnsForAnyArgs(ipAddresses);
    }

    private async Task<DnsResponse> ExecuteWithStopwatchAndCustomHostAsync(string host)
    {
        return await ExecuteWithStopwatchAsync(() => ResolveWithSpecificHostAsync(host));
    }

    private async Task<DnsResponse> ResolveWithSpecificHostAsync(string host)
    {
        return await _resolver.ResolveAsync(host, _cancellationTokenSource.Token);
    }

    [TestMethod]
    public async Task TestResolveAsync_WithEmptyHost()
    {
        InitializeResolverWithRealNameServersLoader();

        DnsResponse response = await ExecuteWithStopwatchAndCustomHostAsync(string.Empty);

        Assert.AreEqual(null, response);
        Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public async Task TestResolveAsync_WithNullHost()
    {
        InitializeResolverWithRealNameServersLoader();

        DnsResponse response = await ExecuteWithStopwatchAndCustomHostAsync(null);

        Assert.AreEqual(null, response);
        Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(1));
    }
}