/*
 * Copyright (c) 2021 Proton Technologies AG
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
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Handlers;
using ProtonVPN.Core.OS.Net.DoH;
using ProtonVPN.Core.Settings;
using RichardSzalay.MockHttp;

namespace ProtonVPN.Core.Test.Api.Handlers
{
    [TestClass]
    public class AlternativeHostHandlerTest
    {
        private MockHttpMessageHandler _innerHandler;
        private IAppSettings _appSettings;
        private readonly Uri _baseAddress = new Uri("https://api.protonvpn.ch");

        [TestInitialize]
        public void TestInitialize()
        {
            _innerHandler = new MockHttpMessageHandler();
            _appSettings = Substitute.For<IAppSettings>();
        }

        [TestMethod]
        public async Task SendAsync_ShouldNotTriggerAlternativeHostIfItIsDisabled()
        {
            // Arrange
            _appSettings.DoHEnabled.Returns(false);
            _appSettings.ActiveAlternativeApiBaseUrl = "alternative.api.url";
            _appSettings.LastPrimaryApiFail = DateTime.Now;

            AlternativeHostHandler handler = new(
                Substitute.For<ILogger>(),
                new DohClients(new List<string> {"provider1", "provider2"}, TimeSpan.FromSeconds(10)),
                new MainHostname("https://api.protonvpn.ch"),
                _appSettings,
                new GuestHoleState(),
                "api.protonvpn.ch") { InnerHandler = _innerHandler };

            HttpClient client = new(handler) { BaseAddress = _baseAddress };

            _innerHandler.Expect(HttpMethod.Get, _baseAddress.ToString()).Respond(HttpStatusCode.OK);

            HttpResponseMessage result = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, _baseAddress));
            result.RequestMessage.RequestUri.Should().Be(_baseAddress);
        }
    }
}