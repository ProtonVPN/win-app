/*
 * Copyright (c) 2022 Proton Technologies AG
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
using ProtonVPN.P2PDetection.Blocked;

namespace ProtonVPN.App.Tests.P2PDetection.Blocked
{
    [TestClass]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public class BlockedTrafficTest
    {
        private IHttpClients _httpClients;
        private IHttpClient _httpClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _httpClients = Substitute.For<IHttpClients>();
            _httpClient = Substitute.For<IHttpClient>();

            _httpClients.Client(default).ReturnsForAnyArgs(_httpClient);
        }

        [TestMethod]
        public void BlockedTraffic_ShouldThrow_WhenHttpClients_IsNull()
        {
            // Act
            Action action = () => new BlockedTraffic(null, new Uri("http://blablabla"), TimeSpan.FromSeconds(10));
            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void BlockedTraffic_ShouldThrow_WhenP2PStatusUri_IsNull()
        {
            // Act
            Action action = () => new BlockedTraffic(_httpClients, null, TimeSpan.FromSeconds(10));
            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void BlockedTraffic_ShouldSet_HttpClient_Timeout()
        {
            // Arrange
            TimeSpan timeout = TimeSpan.FromSeconds(57);
            // Act
            new BlockedTraffic(_httpClients, new Uri("http://blablabla"), timeout);
            // Assert
            _httpClient.Received(1).Timeout = timeout;
        }

        [TestMethod]
        public async Task Detected_ShouldBeTrue_WhenHttpResponse_ContainsPattern()
        {
            // Arrange
            Uri p2PStatusUri = new Uri("http://protonstatus.test.com/vpn_status_ppp");
            IHttpResponseMessage response = HttpResponseFromString("aj shhd ajh khfk  <!--P2P_WARNING--> owjd ewh e qo");
            _httpClient.GetAsync(p2PStatusUri).Returns(response);
            BlockedTraffic subject = new BlockedTraffic(_httpClients, p2PStatusUri, TimeSpan.FromSeconds(10));
            // Act
            bool result = await subject.Detected();
            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task Detected_ShouldBeFalse_WhenHttpResponse_DoesNotContainPattern()
        {
            // Arrange
            Uri p2PStatusUri = new Uri("http://protonstatus.test.com/vpn_status_ppp");
            IHttpResponseMessage response = HttpResponseFromString("aj shhd ajh khfk  <!--No pattern here--> owjd ewh e qo");
            _httpClient.GetAsync(p2PStatusUri).Returns(response);
            BlockedTraffic subject = new BlockedTraffic(_httpClients, p2PStatusUri, TimeSpan.FromSeconds(11));
            // Act
            bool result = await subject.Detected();
            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task Detected_ShouldBeFalse_WhenHttpResponseCode_IsNotSuccess()
        {
            // Arrange
            Uri p2PStatusUri = new Uri("http://protonstatus.test.com/vpn_status_ppp");
            Task<IHttpResponseMessage> response = FailedHttpResponse();
            _httpClient.GetAsync(p2PStatusUri).Returns(response);
            BlockedTraffic subject = new BlockedTraffic(_httpClients, p2PStatusUri, TimeSpan.FromSeconds(19));
            // Act
            bool result = await subject.Detected();
            // Assert
            result.Should().BeFalse();
        }

        #region Helpers

        private static Task<IHttpResponseMessage> FailedHttpResponse()
        {
            IHttpResponseMessage httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(false);

            return Task.FromResult(httpResponse);
        }

        private static IHttpResponseMessage HttpResponseFromString(string content)
        {
            IHttpResponseMessage httpResponse = Substitute.For<IHttpResponseMessage>();
            httpResponse.IsSuccessStatusCode.Returns(true);
            httpResponse.Content.ReadAsStringAsync().Returns(content);

            return httpResponse;
        }

        #endregion
    }
}
