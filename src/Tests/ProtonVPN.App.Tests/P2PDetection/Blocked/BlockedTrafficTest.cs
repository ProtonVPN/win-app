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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Config.Url;
using ProtonVPN.P2PDetection;
using ProtonVPN.P2PDetection.Blocked;

namespace ProtonVPN.App.Tests.P2PDetection.Blocked
{
    [TestClass]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public class BlockedTrafficTest
    {
        private IHttpClients _httpClients;
        private IHttpClient _httpClient;
        private IActiveUrls _activeUrls;
        private IP2PDetectionTimeout _p2PDetectionTimeout;
        private IOsProcesses _osProcesses;

        [TestInitialize]
        public void TestInitialize()
        {
            _httpClients = Substitute.For<IHttpClients>();
            _httpClient = Substitute.For<IHttpClient>();
            _activeUrls = Substitute.For<IActiveUrls>();
            _p2PDetectionTimeout = Substitute.For<IP2PDetectionTimeout>();
            _osProcesses = Substitute.For<IOsProcesses>();

            _httpClients.Client(default).ReturnsForAnyArgs(_httpClient);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _httpClients = null;
            _httpClient = null;
            _activeUrls = null;
            _p2PDetectionTimeout = null;
            _osProcesses = null;
        }

        [TestMethod]
        public void BlockedTraffic_ShouldThrow_WhenHttpClients_IsNull()
        {
            _activeUrls.P2PStatusUrl.ReturnsForAnyArgs(new ActiveUrl(_osProcesses, "http://blablabla"));
            _p2PDetectionTimeout.GetTimeoutValue().ReturnsForAnyArgs(TimeSpan.FromSeconds(10));

            Action action = () => new BlockedTraffic(null, _activeUrls, _p2PDetectionTimeout);

            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void BlockedTraffic_ShouldThrow_WhenP2PStatusUri_IsNull()
        {
            _p2PDetectionTimeout.GetTimeoutValue().ReturnsForAnyArgs(TimeSpan.FromSeconds(10));

            Action action = () => new BlockedTraffic(_httpClients, null, _p2PDetectionTimeout);

            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void BlockedTraffic_ShouldSet_HttpClient_Timeout()
        {
            TimeSpan timeout = TimeSpan.FromSeconds(57);
            _activeUrls.P2PStatusUrl.ReturnsForAnyArgs(new ActiveUrl(_osProcesses, "http://blablabla"));
            _p2PDetectionTimeout.GetTimeoutValue().ReturnsForAnyArgs(timeout);

            new BlockedTraffic(_httpClients, _activeUrls, _p2PDetectionTimeout);

            _httpClient.Received(1).Timeout = timeout;
        }

        [TestMethod]
        public async Task Detected_ShouldBeTrue_WhenHttpResponse_ContainsPattern()
        {
            Uri p2PStatusUri = new("http://protonstatus.test.com/vpn_status_ppp");
            IHttpResponseMessage response = HttpResponseFromString("aj shhd ajh khfk  <!--P2P_WARNING--> owjd ewh e qo");
            _httpClient.GetAsync(p2PStatusUri).Returns(response);
            _activeUrls.P2PStatusUrl.ReturnsForAnyArgs(new ActiveUrl(_osProcesses, p2PStatusUri.ToString()));
            _p2PDetectionTimeout.GetTimeoutValue().ReturnsForAnyArgs(TimeSpan.FromSeconds(10));

            BlockedTraffic subject = new(_httpClients, _activeUrls, _p2PDetectionTimeout);
            bool result = await subject.Detected();

            result.Should().BeTrue();
        }

        private static IHttpResponseMessage HttpResponseFromString(string content)
        {
            IHttpResponseMessage httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(true);
            httpResponse.Content.ReadAsStringAsync().Returns(content);

            return httpResponse;
        }

        [TestMethod]
        public async Task Detected_ShouldBeFalse_WhenHttpResponse_DoesNotContainPattern()
        {
            Uri p2PStatusUri = new Uri("http://protonstatus.test.com/vpn_status_ppp");
            IHttpResponseMessage response = HttpResponseFromString("aj shhd ajh khfk  <!--No pattern here--> owjd ewh e qo");
            _httpClient.GetAsync(p2PStatusUri).Returns(response);
            _activeUrls.P2PStatusUrl.ReturnsForAnyArgs(new ActiveUrl(_osProcesses, p2PStatusUri.ToString()));
            _p2PDetectionTimeout.GetTimeoutValue().ReturnsForAnyArgs(TimeSpan.FromSeconds(11));

            BlockedTraffic subject = new BlockedTraffic(_httpClients, _activeUrls, _p2PDetectionTimeout);
            bool result = await subject.Detected();

            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task Detected_ShouldBeFalse_WhenHttpResponseCode_IsNotSuccess()
        {
            Uri p2PStatusUri = new Uri("http://protonstatus.test.com/vpn_status_ppp");
            Task<IHttpResponseMessage> response = FailedHttpResponse();
            _httpClient.GetAsync(p2PStatusUri).Returns(response);
            _activeUrls.P2PStatusUrl.ReturnsForAnyArgs(new ActiveUrl(_osProcesses, p2PStatusUri.ToString()));
            _p2PDetectionTimeout.GetTimeoutValue().ReturnsForAnyArgs(TimeSpan.FromSeconds(19));
            
            BlockedTraffic subject = new BlockedTraffic(_httpClients, _activeUrls, _p2PDetectionTimeout);
            bool result = await subject.Detected();

            result.Should().BeFalse();
        }

        private static Task<IHttpResponseMessage> FailedHttpResponse()
        {
            IHttpResponseMessage httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(false);

            return Task.FromResult(httpResponse);
        }
    }
}
