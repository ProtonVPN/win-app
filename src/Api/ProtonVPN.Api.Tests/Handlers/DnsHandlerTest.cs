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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Api.Handlers;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Networking;
using ProtonVPN.Dns.Contracts;
using RichardSzalay.MockHttp;

namespace ProtonVPN.Api.Tests.Handlers
{
    [TestClass]
    public class DnsHandlerTest
    {
        private const string TEST_URL = "https://protonvpn.com/test";

        private ILogger _logger;
        private IDnsManager _dnsManager;
        private MockHttpMessageHandler _mockHttpMessageHandler;
        private DnsHandler _dnsHandler;
        private HttpClient _httpClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _dnsManager = Substitute.For<IDnsManager>();
            _dnsManager.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                       .Returns(CreateIpAddressList());
            _mockHttpMessageHandler = new MockHttpMessageHandler();
            _dnsHandler = new(_logger, _dnsManager) { InnerHandler = _mockHttpMessageHandler };
            _httpClient = new(_dnsHandler);
        }

        private IList<IpAddress> CreateIpAddressList()
        {
            return new List<IpAddress>()
            {
                new(IPAddress.Parse("192.168.1.1")),
                new(IPAddress.Parse("192.168.2.2")),
                new(IPAddress.Parse("192.168.3.3")),
            };
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _logger = null;
            _dnsManager = null;
            _mockHttpMessageHandler = null;
            _dnsHandler = null;
            _httpClient = null;
        }

        [TestMethod]
        public async Task Test_WithIpAddress()
        {
            string url = "http://127.0.0.1/auth";
            HttpRequestMessage request = new(HttpMethod.Get, url);
            MockedRequest mockedRequest = _mockHttpMessageHandler.When("http://127.0.0.1/auth")
                                                                 .Respond(_ => new(HttpStatusCode.OK));

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(url, request.RequestUri.ToString());
            await AssertDnsManagerWasNotCalledAsync();
            _mockHttpMessageHandler.GetMatchCount(mockedRequest).Should().Be(1);
        }

        private async Task AssertDnsManagerWasNotCalledAsync()
        {
            await _dnsManager.Received(0).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            await _dnsManager.Received(0).ResolveWithoutCacheAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            _dnsManager.Received(0).GetFromCache(Arg.Any<string>());
        }

        [TestMethod]
        public async Task Test_WithDomain()
        {
            HttpRequestMessage request = new(HttpMethod.Get, TEST_URL);
            MockedRequest mockedRequest = _mockHttpMessageHandler.When("https://192.168.1.1/test")
                                                                 .Respond(_ => new(HttpStatusCode.OK));

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("https://192.168.1.1/test", request.RequestUri.ToString());
            await _dnsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            _mockHttpMessageHandler.GetMatchCount(mockedRequest).Should().Be(1);
        }

        [TestMethod]
        public async Task Test_WithDomainAndFirstIpAddressFails()
        {
            HttpRequestMessage request = new(HttpMethod.Get, TEST_URL);
            MockedRequest mockedRequest1 = _mockHttpMessageHandler.When("https://192.168.1.1/test")
                                                                  .Respond(_ => throw new("Test exception"));
            MockedRequest mockedRequest2 = _mockHttpMessageHandler.When("https://192.168.2.2/test")
                                                                 .Respond(_ => new(HttpStatusCode.OK));

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("https://192.168.2.2/test", request.RequestUri.ToString());
            await _dnsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            _mockHttpMessageHandler.GetMatchCount(mockedRequest1).Should().Be(1);
            _mockHttpMessageHandler.GetMatchCount(mockedRequest2).Should().Be(1);
        }

        [TestMethod]
        public async Task Test_WithDomainAndFirstTwoIpAddressesFail()
        {
            HttpRequestMessage request = new(HttpMethod.Get, TEST_URL);
            MockedRequest mockedRequest1 = _mockHttpMessageHandler.When("https://192.168.1.1/test")
                                                                  .Respond(_ => throw new("Test exception"));
            MockedRequest mockedRequest2 = _mockHttpMessageHandler.When("https://192.168.2.2/test")
                                                                  .Respond(_ => throw new("Test exception"));
            MockedRequest mockedRequest3 = _mockHttpMessageHandler.When("https://192.168.3.3/test")
                                                                  .Respond(_ => new(HttpStatusCode.OK));

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("https://192.168.3.3/test", request.RequestUri.ToString());
            await _dnsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            _mockHttpMessageHandler.GetMatchCount(mockedRequest1).Should().Be(1);
            _mockHttpMessageHandler.GetMatchCount(mockedRequest2).Should().Be(1);
            _mockHttpMessageHandler.GetMatchCount(mockedRequest3).Should().Be(1);
        }

        [TestMethod]
        public async Task Test_WithDomainAndAllIpAddressesFail()
        {
            string expectedExceptionMessage = "Test exception 3";
            HttpRequestMessage request = new(HttpMethod.Get, TEST_URL);
            MockedRequest mockedRequest1 = _mockHttpMessageHandler.When("https://192.168.1.1/test")
                                                                  .Respond(_ => throw new("Test exception 1"));
            MockedRequest mockedRequest2 = _mockHttpMessageHandler.When("https://192.168.2.2/test")
                                                                  .Respond(_ => throw new("Test exception 2"));
            MockedRequest mockedRequest3 = _mockHttpMessageHandler.When("https://192.168.3.3/test")
                                                                  .Respond(_ => throw new(expectedExceptionMessage));

            Exception exception = await Assert.ThrowsExceptionAsync<Exception>(
                async () => await _httpClient.SendAsync(request));
            
            Assert.AreEqual(expectedExceptionMessage, exception.Message);
            Assert.AreEqual(TEST_URL, request.RequestUri.ToString());
            await _dnsManager.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            _mockHttpMessageHandler.GetMatchCount(mockedRequest1).Should().Be(1);
            _mockHttpMessageHandler.GetMatchCount(mockedRequest2).Should().Be(1);
            _mockHttpMessageHandler.GetMatchCount(mockedRequest3).Should().Be(1);
        }
    }
}