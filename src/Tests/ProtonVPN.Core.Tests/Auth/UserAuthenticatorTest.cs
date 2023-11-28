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
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Tests.Auth
{
    [TestClass]
    public class UserAuthenticatorTest
    {
        private const string API_UNAUTH_SESSION_ERROR = "Unauthenticated session not available.";
        private const string API_AUTH_ERROR = "auth failed";
        private const string USERNAME = "username";
        private const string UNAUTH_SESSION_ACCESS_TOKEN = "hnnamrzvsgdbxvx74rjadbovyjy63vz4";
        private const string UNAUTH_SESSION_UID = "rf3c4fida9c2066e6c5669a293177cik";
        private readonly SecureString _password = new NetworkCredential("", "password").SecurePassword;

        private IApiClient _apiClient;
        private ILogger _logger;
        private IUserStorage _userStorage;
        private IAppSettings _appSettings;
        private IAuthCertificateManager _authCertificateManager;
        private ISsoAuthenticator _ssoAuthenticator;

        [TestInitialize]
        public void Initialize()
        {
            _apiClient = Substitute.For<IApiClient>();
            _logger = Substitute.For<ILogger>();
            _userStorage = Substitute.For<IUserStorage>();
            _appSettings = Substitute.For<IAppSettings>();
            _authCertificateManager = Substitute.For<IAuthCertificateManager>();
            _ssoAuthenticator = Substitute.For<ISsoAuthenticator>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _apiClient = null;
            _logger = null;
            _userStorage = null;
            _appSettings = null;
            _authCertificateManager = null;
            _ssoAuthenticator = null;
        }

        [TestMethod]
        public async Task AuthShouldFailWhenUnauthSessionRequestFails()
        {
            // Arrange
            _apiClient.PostUnauthSessionAsync()
                .Returns(ApiResponseResult<UnauthSessionResponse>.Fail(new HttpResponseMessage(), "unauth session failed"));
            UserAuthenticator sut = GetUserAuthenticator();

            // Act
            AuthResult result = await sut.AuthAsync(USERNAME, _password);

            // Assert
            result.Success.Should().BeFalse();
            result.Failure.Should().BeTrue();
            result.Error.Should().Contain(API_UNAUTH_SESSION_ERROR);
        }

        [TestMethod]
        public async Task AuthShouldFailWhenAuthInfoRequestFails()
        {
            // Arrange
            _apiClient.PostUnauthSessionAsync()
                .Returns(ApiResponseResult<UnauthSessionResponse>.Ok(new HttpResponseMessage(), GetUnauthSessionResponse()));
            _apiClient.GetAuthInfoResponse(Arg.Any<AuthInfoRequest>(), UNAUTH_SESSION_ACCESS_TOKEN, UNAUTH_SESSION_UID)
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
            _apiClient.PostUnauthSessionAsync()
                .Returns(ApiResponseResult<UnauthSessionResponse>.Ok(new HttpResponseMessage(), GetUnauthSessionResponse()));
            _apiClient.GetAuthInfoResponse(Arg.Any<AuthInfoRequest>(), UNAUTH_SESSION_ACCESS_TOKEN, UNAUTH_SESSION_UID)
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

        private UnauthSessionResponse GetUnauthSessionResponse()
        {
            return new UnauthSessionResponse() 
            { 
                AccessToken = UNAUTH_SESSION_ACCESS_TOKEN, 
                Uid = UNAUTH_SESSION_UID 
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
            return new(_apiClient, _logger, _userStorage, _appSettings, _authCertificateManager, _ssoAuthenticator);
        }
    }
}