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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Handlers;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.OS.Net.DoH;
using ProtonVPN.Core.Settings;
using RichardSzalay.MockHttp;

namespace ProtonVPN.Api.Tests.Handlers
{
    [TestClass]
    public class AlternativeHostHandlerTest
    {
        private const string BASE_API_URL = "https://api.protonvpn.ch";

        private MockHttpMessageHandler _innerHandler;
        private IAppSettings _appSettings;
        private IApiHostProvider _apiHostProvider;
        private Config _config;
        private readonly Uri _baseAddress = new(BASE_API_URL);

        [TestInitialize]
        public void TestInitialize()
        {
            _innerHandler = new MockHttpMessageHandler();
            _appSettings = Substitute.For<IAppSettings>();
            _apiHostProvider = Substitute.For<IApiHostProvider>();
            _config = new Config { Urls = { ApiUrl = BASE_API_URL } };
        }

        [TestMethod]
        public async Task SendAsync_ShouldNotTriggerAlternativeHostIfItIsDisabled()
        {
            // Arrange
            _appSettings.DoHEnabled.Returns(false);
            _appSettings.ActiveAlternativeApiBaseUrl = "alternative.api.url";
            _appSettings.LastPrimaryApiFailDateUtc = DateTime.UtcNow;

            AlternativeHostHandler handler = new(
                new MockOfCancellingHandler(_innerHandler),
                Substitute.For<ILogger>(),
                new DohClients(new List<string> { "provider1", "provider2" }, TimeSpan.FromSeconds(10)),
                new MainHostname(BASE_API_URL),
                _appSettings,
                new GuestHoleState(),
                _apiHostProvider,
                _config) { InnerHandler = _innerHandler };

            HttpClient client = new(handler) { BaseAddress = _baseAddress };

            _innerHandler.Expect(HttpMethod.Get, _baseAddress.ToString()).Respond(HttpStatusCode.OK);

            HttpResponseMessage result = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, _baseAddress));
            result.RequestMessage.RequestUri.Should().Be(_baseAddress);
        }
    }
}