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

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Settings;
using RichardSzalay.MockHttp;

namespace ProtonVPN.Api.Tests
{
    [TestClass]
    public class ApiClientTest
    {
        private ILogger _logger;
        private IAppSettings _appSettings;
        private IApiAppVersion _appVersion;
        private IApiHttpClientFactory _apiHttpClientFactory;
        private IApiClient _apiClient;
        private readonly MockHttpMessageHandler _fakeHttpMessageHandler = new();
        private IAppLanguageCache _appLanguageCache;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _appLanguageCache = Substitute.For<IAppLanguageCache>();

            _appVersion = Substitute.For<IApiAppVersion>();
            _appVersion.Value().Returns(string.Empty);
            _appVersion.UserAgent().Returns("User agent");

            _appSettings = Substitute.For<IAppSettings>();
            _appSettings.AccessToken.Returns(string.Empty);
            _appSettings.Uid.Returns(string.Empty);

            HttpClient httpClient = _fakeHttpMessageHandler.ToHttpClient();
            httpClient.BaseAddress = new("http://127.0.0.1");

            _apiHttpClientFactory = Substitute.For<IApiHttpClientFactory>();
            _apiHttpClientFactory.GetApiHttpClientWithoutCache().Returns(httpClient);
            _apiHttpClientFactory.GetApiHttpClientWithCache().Returns(httpClient);

            IConfiguration config = new Config();

            _apiClient = new ApiClient(_apiHttpClientFactory, _logger, _appSettings, _appVersion, _appLanguageCache, config);
        }

        [TestMethod]
        public async Task ServerListDownloaded()
        {
            _fakeHttpMessageHandler.When("*").Respond(req => new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'Code' : '1000', 'Servers': []}")
            });

            ApiResponseResult<ServersResponse> response = await _apiClient.GetServersAsync("127.0.0.0");

            response.Success.Should().BeTrue();
        }
    }
}