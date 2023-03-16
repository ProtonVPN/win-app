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
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.NameServers;
using ProtonVPN.Dns.Resolvers;
using ProtonVPN.Dns.Tests.Mocks;

namespace ProtonVPN.Dns.Tests.Resolvers
{
    [TestClass]
    public class DnsOverUdpResolverTest
    {
        private const string HOST = "api.protonvpn.ch";
        private static readonly TimeSpan DNS_RESOLVE_TIMEOUT = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan DEFAULT_DNS_TTL = TimeSpan.FromMinutes(20);

        private MockOfLogger _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private Stopwatch _stopwatch;
        private IConfiguration _configuration;
        private MockOfNameServersLoader _mockOfNameServersLoader;
        private DnsOverUdpResolver _resolver;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = new MockOfLogger();
            _cancellationTokenSource = new CancellationTokenSource();
            _stopwatch = new Stopwatch();
            _configuration = Substitute.For<IConfiguration>();
            _configuration.DnsResolveTimeout.Returns(DNS_RESOLVE_TIMEOUT);
            _configuration.DefaultDnsTimeToLive.Returns(DEFAULT_DNS_TTL);
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
            _resolver = new(nameServersLoader, _configuration, _logger);
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
        public async Task TestResolveAsync_WithoutNameServers()
        {
            InitializeWithMockOfNameServersLoader();

            DnsResponse response = await ExecuteAsync();

            Assert.AreEqual(null, response);
            Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(1));
        }

        private void InitializeWithMockOfNameServersLoader()
        {
            _mockOfNameServersLoader = new MockOfNameServersLoader();
            _resolver = new DnsOverUdpResolver(_mockOfNameServersLoader, _configuration, _logger);
        }

        [TestMethod]
        public async Task TestResolveAsync_WithNonWorkingNameServers()
        {
            InitializeWithMockOfNameServersLoader();
            SetNonWorkingNameServers();

            DnsResponse response = await ExecuteAsync();

            Assert.AreEqual(null, response);
            Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(20));
        }

        private void SetNonWorkingNameServers()
        {
            _mockOfNameServersLoader.Set(IPAddress.Loopback, IPAddress.Parse("192.168.153.153"));
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

            DnsResponse response = await ExecuteWithStopwatchAndCustomHostAsync(
                "g5f16gfds1gdsf5g16dsfg15fs5gfds651d61s651g6516gf1s6fdgfs.vhbverhu");

            Assert.AreEqual(null, response);
            Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(20));
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
}