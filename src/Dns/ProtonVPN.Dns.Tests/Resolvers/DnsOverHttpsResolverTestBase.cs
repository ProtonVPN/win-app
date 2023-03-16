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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Networking;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.Resolvers;
using ProtonVPN.Dns.Tests.Mocks;
using RichardSzalay.MockHttp;

namespace ProtonVPN.Dns.Tests.Resolvers
{
    [TestClass]
    public abstract class DnsOverHttpsResolverTestBase<T>
        where T : DnsOverHttpsResolverBase
    {
        private const string TEST_TOO_QUICK_MESSAGE =
            "The test completed too quickly, meaning the request did not took several seconds before timing out.";
        private const string TEST_CANCEL_TOOK_TOO_LONG_MESSAGE =
            "The test took too long to complete, meaning the cancellation is not working.";

        private readonly string _host;
        private readonly IList<string> _dohProviders = new List<string>()
        {
            "https://dns11.quad9.net/dns-query",
            "https://dns.google/dns-query",
        };
        private readonly IList<IpAddress> _ipAddresses = new List<IpAddress>()
        {
            new IpAddress(IPAddress.Parse("192.168.1.1")),
            new IpAddress(IPAddress.Parse("192.168.2.2")),
            new IpAddress(IPAddress.Parse("192.168.3.3")),
        };

        private MockOfLogger _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private Stopwatch _stopwatch;
        private MockHttpMessageHandler _mockHttpMessageHandler;
        private MockOfHttpClientFactory _httpClientFactory;
        private IDnsOverHttpsProvidersManager _mockOfDnsOverHttpsProvidersManager;
        private IConfiguration _configuration;
        private T _resolver;

        protected DnsOverHttpsResolverTestBase(string host)
        {
            _host = host;
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = new MockOfLogger();
            _cancellationTokenSource = new CancellationTokenSource();
            _stopwatch = new Stopwatch();
            _mockHttpMessageHandler = new MockHttpMessageHandler();
            _httpClientFactory = new MockOfHttpClientFactory(_mockHttpMessageHandler);
            _mockOfDnsOverHttpsProvidersManager = Substitute.For<IDnsOverHttpsProvidersManager>();
            _configuration = Substitute.For<IConfiguration>();
            _configuration.DohClientTimeout.Returns(TimeSpan.FromSeconds(10));
            _configuration.DnsOverHttpsPerProviderTimeout.Returns(TimeSpan.FromSeconds(20));
            _configuration.DnsResolveTimeout.Returns(TimeSpan.FromSeconds(30));
            _configuration.DefaultDnsTimeToLive.Returns(TimeSpan.FromMinutes(20));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _logger = null;
            _cancellationTokenSource = null;
            _stopwatch = null;
            _mockHttpMessageHandler = null;
            _httpClientFactory = null;
            _mockOfDnsOverHttpsProvidersManager = null;
            _configuration = null;
            _resolver = null;
        }

        [TestMethod]
        public async Task TestResolveAsync()
        {
            InitializeResolverCorrectly();
            InitializeSuccessfulRequests();

            DnsResponse response = await ExecuteAsync(_host);

            AssertCorrectResponse(response);
            Assert.IsTrue(response.TimeToLive > TimeSpan.Zero);
            Assert.IsTrue(response.ExpirationDateTimeUtc > DateTime.UtcNow);
            Assert.IsTrue(response.ResponseDateTimeUtc <= DateTime.UtcNow);
            Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(9));
        }

        private void InitializeSuccessfulRequests()
        {
            foreach (IpAddress ipAddress in _ipAddresses)
            {
                _mockHttpMessageHandler
                    .When($"https://{ipAddress}/dns-query?dns=AAABAAABAAAAAAAAA2FwaQlwcm90b252cG4CY2gAAAEAAQ")
                    .Respond(_ => new(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(
                            "AACBgAABAAEAAAAAA2FwaQlwcm90b252cG4CY2gAAAEAAcAMAAEAAQAAAfAABLmfn6o=".FromBase64String())
                    });

                _mockHttpMessageHandler
                    .When($"https://{ipAddress}/dns-query?dns=AAABAAABAAAAAAAAG2RNRllHU0xUUU9KWFhJMzNPT1pZRzRMVEROQQlwcm90b25wcm8DeHl6AAAQAAE")
                    .Respond(_ => new(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(
                            "AACBgAABAAMAAAAAG2RNRllHU0xUUU9KWFhJMzNPT1pZRzRMVEROQQlwcm90b25wcm8DeHl6AAAQAAHADAAQAAEAAAB4ADMyZWMyLTMtNjUtMjYtMTQ4LmV1LWNlbnRyYWwtMS5jb21wdXRlLmFtYXpvbmF3cy5jb23ADAAQAAEAAAB4ADY1ZWMyLTE4LTE5Mi0xMTUtMTg2LmV1LWNlbnRyYWwtMS5jb21wdXRlLmFtYXpvbmF3cy5jb23ADAAQAAEAAAB4ADQzZWMyLTMtNjgtMjMyLTIwMy5ldS1jZW50cmFsLTEuY29tcHV0ZS5hbWF6b25hd3MuY29t".FromBase64String())
                    });
            }
        }

        protected abstract void AssertCorrectResponse(DnsResponse response);

        private async Task<DnsResponse> ExecuteAsync(string host)
        {
            return await ExecuteWithStopwatchAsync(() => ResolveAsync(host));
        }

        private async Task<DnsResponse> ResolveAsync(string host)
        {
            return await _resolver.ResolveAsync(host, _cancellationTokenSource.Token);
        }

        private async Task<DnsResponse> ExecuteWithStopwatchAsync(Func<Task<DnsResponse>> task)
        {
            _stopwatch.Start();
            DnsResponse response = await task();
            _stopwatch.Stop();
            return response;
        }

        private void InitializeResolverCorrectly()
        {
            _configuration.DoHProviders.Returns(_dohProviders.ToList());
            _resolver = CreateResolver(_configuration, _logger, _httpClientFactory, _mockOfDnsOverHttpsProvidersManager);
            InitializeDnsOverHttpsProvidersManagerReturningSuccessfully();
        }

        private void InitializeDnsOverHttpsProvidersManagerReturningSuccessfully()
        {
            foreach (string dohProvider in _dohProviders)
            {
                _mockOfDnsOverHttpsProvidersManager
                    .GetAsync(dohProvider, Arg.Any<CancellationToken>())
                    .ReturnsForAnyArgs(_ipAddresses.ToList());
            }
        }

        protected abstract T CreateResolver(IConfiguration configuration, MockOfLogger logger,
            MockOfHttpClientFactory httpClientFactory, IDnsOverHttpsProvidersManager dnsOverHttpsProvidersManager);

        [TestMethod]
        public async Task TestResolveAsync_WithNonExistentHost()
        {
            string host = "g5f16gfds1gdsf5g16dsfg15fs5gfds651d61s651g6516gf1s6fdgfs.vhbverhu";
            InitializeResolverCorrectly();

            DnsResponse response = await ExecuteAsync(host);

            Assert.AreEqual(null, response);
            Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(15));
        }

        [TestMethod]
        public async Task TestResolveAsync_WithEmptyHost()
        {
            string host = string.Empty;
            InitializeResolverCorrectly();

            DnsResponse response = await ExecuteAsync(host);

            Assert.AreEqual(null, response);
            Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public async Task TestResolveAsync_WithNullHost()
        {
            string host = null;
            InitializeResolverCorrectly();

            DnsResponse response = await ExecuteAsync(host);

            Assert.AreEqual(null, response);
            Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public async Task TestResolveAsync_WithCancelledToken()
        {
            string host = null;
            InitializeResolverCorrectly();
            _cancellationTokenSource.Cancel();

            DnsResponse response = await ExecuteAsync(host);

            Assert.AreEqual(null, response);
            Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public async Task TestResolveAsync_WithWrongProviderUrls()
        {
            InitializeResolverWithWrongProviderUrls();

            DnsResponse response = await ExecuteAsync(_host);

            Assert.AreEqual(null, response);
            Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(15));
        }

        private void InitializeResolverWithWrongProviderUrls()
        {
            _configuration.DoHProviders.Returns(new List<string>()
            {
                "https://g5f16gfds1gdsf5g16dsfg15fs5gfds651d61s651g6516gf1s6fdgfs.vhbverhu/dns-query",
                "https://g5f16gfds1gdsf5g16dsfg15fs5gfds651d61s651g6516gf1s6fdgfs.vhbverhu/dns-query/",
                "https://g5f16gfds1gdsf5g16dsfg15fs5gfds651d61s651g6516gf1s6fdgfs.vhbverhu/",
                "https://g5f16gfds1gdsf5g16dsfg15fs5gfds651d61s651g6516gf1s6fdgfs.vhbverhu",
            });
            _resolver = CreateResolver(_configuration, _logger, _httpClientFactory, _mockOfDnsOverHttpsProvidersManager);
        }

        [TestMethod]
        public async Task TestResolveAsync_WithWrongProviderIpAddresses()
        {
            InitializeResolverWithWrongProviderIpAddresses();

            DnsResponse response = await ExecuteAsync(_host);

            Assert.AreEqual(null, response);
            Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(15));
        }

        private void InitializeResolverWithWrongProviderIpAddresses()
        {
            _configuration.DoHProviders.Returns(new List<string>()
            {
                "https://192.168.153.153",
                "https://192.168.154.154",
                "https://192.168.155.155",
            });
            _resolver = CreateResolver(_configuration, _logger, _httpClientFactory, _mockOfDnsOverHttpsProvidersManager);
            InitializeDnsOverHttpsProvidersManagerTimingOut();
        }

        private void InitializeDnsOverHttpsProvidersManagerTimingOut()
        {
            foreach (string dohProvider in _dohProviders)
            {
                _mockOfDnsOverHttpsProvidersManager
                    .GetAsync(dohProvider, Arg.Any<CancellationToken>())
                    .ReturnsForAnyArgs(async _ => await TimeoutHttpRequestAsync());
            }
        }

        private async Task<IList<IpAddress>> TimeoutHttpRequestAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(10), _cancellationTokenSource.Token);
            throw new TimeoutException("Unit test HTTP request timeout");
        }

        [TestMethod]
        public async Task TestResolveAsync_WhenCancelled()
        {
            InitializeResolverWithWrongProviderIpAddresses();

            _stopwatch.Start();
            Task<DnsResponse> task = Task.Run(() => _resolver.ResolveAsync(_host, _cancellationTokenSource.Token));
            Task.Delay(TimeSpan.FromSeconds(3)).ContinueWith(_ => _cancellationTokenSource.Cancel());
            DnsResponse response = await task;
            _stopwatch.Stop();

            Assert.AreEqual(null, response);
            Assert.IsTrue(_stopwatch.Elapsed > TimeSpan.FromSeconds(2), TEST_TOO_QUICK_MESSAGE);
            Assert.IsTrue(_stopwatch.Elapsed < TimeSpan.FromSeconds(5), TEST_CANCEL_TOOK_TOO_LONG_MESSAGE);
        }
    }
}