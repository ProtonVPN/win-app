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
using System.Security;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Tests.Auth
{
    [TestClass]
    public class UserAuthenticatorTest
    {
        private const string API_AUTH_ERROR = "auth failed";
        private const string USERNAME = "username";
        private readonly SecureString _password = new NetworkCredential("", "password").SecurePassword;

        private IApiClient _apiClient;
        private ILogger _logger;
        private IUserStorage _userStorage;
        private IAppSettings _appSettings;
        private IAuthCertificateManager _authCertificateManager;

        [TestInitialize]
        public void Initialize()
        {
            _apiClient = Substitute.For<IApiClient>();
            _logger = Substitute.For<ILogger>();
            _userStorage = Substitute.For<IUserStorage>();
            _appSettings = Substitute.For<IAppSettings>();
            _authCertificateManager = Substitute.For<IAuthCertificateManager>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _apiClient = null;
            _logger = null;
            _userStorage = null;
            _appSettings = null;
            _authCertificateManager = null;
        }

        [TestMethod]
        public async Task AuthShouldFailWhenAuthInfoRequestFails()
        {
            // Arrange
            _apiClient.GetAuthInfoResponse(Arg.Any<AuthInfoRequest>())
                .Returns(ApiResponseResult<AuthInfoResponse>.Fail(new HttpResponseMessage(), API_AUTH_ERROR));
            UserAuthenticator sut = GetUserAuthenticator();

            // Act
            AuthResult result = await sut.AuthAsync(USERNAME, _password);

            // Assert
            result.Should().BeEquivalentTo(AuthResult.Fail(API_AUTH_ERROR));
        }

        [TestMethod]
        public async Task AuthShouldFailWhenApiResponseContainsNoSalt()
        {
            // Arrange
            _apiClient.GetAuthInfoResponse(Arg.Any<AuthInfoRequest>())
                .Returns(ApiResponseResult<AuthInfoResponse>.Ok(new HttpResponseMessage(),
                    GetAuthInfoResponseWithEmptySalt()));
            UserAuthenticator sut = GetUserAuthenticator();

            // Act
            AuthResult result = await sut.AuthAsync(USERNAME, _password);

            // Assert
            result.Success.Should().BeFalse();
            result.Failure.Should().BeTrue();
            result.Error.Should().Contain("Incorrect login credentials");
        }

        private AuthInfoResponse GetSuccessAuthInfoResponse()
        {
            return new()
            {
                Code = ResponseCodes.OkResponse,
                Details = new(),
                Error = null,
                Modulus = "modulus",
                Salt = "salt",
                ServerEphemeral = "serverEphemeral",
                SrpSession = "session",
                Version = 4,
            };
        }

        private AuthInfoResponse GetAuthInfoResponseWithEmptySalt()
        {
            AuthInfoResponse response = GetSuccessAuthInfoResponse();
            response.Salt = null;
            return response;
        }

        private UserAuthenticator GetUserAuthenticator()
        {
            return new(_apiClient, _logger, _userStorage, _appSettings, _authCertificateManager);
        }
    }
}