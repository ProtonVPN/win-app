/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Settings;
using RichardSzalay.MockHttp;

namespace ProtonVPN.Core.Test.Api
{
    [TestClass]
    public class ApiClientTest
    {
        private ILogger _logger;
        private ITokenStorage _tokenStorage;
        private IApiAppVersion _appVersion;
        private HttpClient _httpClient;
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

            _tokenStorage = Substitute.For<ITokenStorage>();
            _tokenStorage.AccessToken.Returns(string.Empty);
            _tokenStorage.Uid.Returns(string.Empty);

            _httpClient = _fakeHttpMessageHandler.ToHttpClient();
            _httpClient.BaseAddress = new("http://127.0.0.1");


            _apiClient = new ApiClient(_httpClient, _httpClient, _logger, _tokenStorage, _appVersion, _appLanguageCache, null);
        }

        [TestMethod]
        public async Task ServerListDownloaded()
        {
            _fakeHttpMessageHandler.When("*").Respond(req => new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{'Code' : '1000', 'Servers': []}")
            });

            ApiResponseResult<ServerList> response = await _apiClient.GetServersAsync("127.0.0.0");

            response.Success.Should().BeTrue();
        }
    }
}
