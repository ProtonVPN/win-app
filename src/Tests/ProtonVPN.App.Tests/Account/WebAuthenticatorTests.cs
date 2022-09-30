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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.Account;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Common.Logging;

namespace ProtonVPN.App.Tests.Account
{
    [TestClass]
    public class WebAuthenticatorTest
    {
        private IApiClient _apiClient;
        private Common.Configuration.Config _config;
        private ILogger _logger;

        [TestInitialize]
        public void Initialize()
        {
            _apiClient = Substitute.For<IApiClient>();
            //TODO: use IConfiguration
            _config = new Common.Configuration.Config();
            _logger = Substitute.For<ILogger>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _apiClient = null;
            _config = null;
            _logger = null;
        }

        [TestMethod]
        public async Task ItShouldUseAutoLoginUrl()
        {
            // Arrange
            _config.AutoLoginBaseUrl = "http://proton.url";
            string selector = "selector1";
            _apiClient.ForkAuthSessionAsync(Arg.Any<AuthForkSessionRequest>()).Returns(
                ApiResponseResult<ForkedAuthSessionResponse>.Ok(new HttpResponseMessage(),
                    new ForkedAuthSessionResponse { Code = ResponseCodes.OkResponse, Selector = selector, }));
            WebAuthenticator sut = new(_apiClient, _config, _logger);

            // Act
            string url = await sut.GetLoginUrlAsync(GetLoginUrlParams());

            // Assert
            url.Should().Be($"{_config.AutoLoginBaseUrl}?action=action&" +
                            $"fullscreen=on&" +
                            $"redirect={WebAuthenticator.CUSTOM_PROTOCOL_PREFIX}redirect&" +
                            $"start=start" +
                            $"#selector={selector}");
        }

        [TestMethod]
        public async Task ItShouldFallbackToAccountUrl_OnApiError()
        {
            _apiClient.ForkAuthSessionAsync(Arg.Any<AuthForkSessionRequest>()).Returns(
                ApiResponseResult<ForkedAuthSessionResponse>.Fail(new HttpResponseMessage(HttpStatusCode.BadRequest),
                    string.Empty));
            await ItShouldFallbackToAccountUrl();
        }

        [TestMethod]
        public async Task ItShouldFallbackToAccountUrl_OnApiException()
        {
            _apiClient.ForkAuthSessionAsync(Arg.Any<AuthForkSessionRequest>()).Throws(new HttpRequestException());
            await ItShouldFallbackToAccountUrl();
        }

        private async Task ItShouldFallbackToAccountUrl()
        {
            // Arrange
            _config.Urls.AccountUrl = "http://proton.account.url";

            WebAuthenticator sut = new(_apiClient, _config, _logger);

            // Act
            string url = await sut.GetLoginUrlAsync(GetLoginUrlParams());

            // Assert
            url.Should().Be(_config.Urls.AccountUrl);
        }

        private LoginUrlParams GetLoginUrlParams()
        {
            return new LoginUrlParams
            {
                Action = "action",
                Fullscreen = "on",
                Redirect = "redirect",
                Start = "start",
            };
        }
    }
}