/*
 * Copyright (c) 2025 Proton AG
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
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.Logic.Auth.Tests;

[TestClass]
public class SrpAuthenticatorTest
{
    private const string API_AUTH_ERROR = "auth failed";
    private const string USERNAME = "username";
    private readonly SecureString _password = new NetworkCredential("", "password").SecurePassword;

    private IApiClient _apiClient;
    private IUnauthSessionManager _unauthSessionManager;
    private ISettings _settings;
    private IGuestHoleManager _guestHoleManager;

    [TestInitialize]
    public void Initialize()
    {
        _apiClient = Substitute.For<IApiClient>();
        _settings = Substitute.For<ISettings>();
        _guestHoleManager = Substitute.For<IGuestHoleManager>();
        _unauthSessionManager = Substitute.For<IUnauthSessionManager>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _apiClient = null;
        _settings = null;
        _guestHoleManager = null;
        _unauthSessionManager = null;
    }

    [TestMethod]
    public async Task AuthShouldFailWhenApiResponseContainsNoSaltAsync()
    {
        // Arrange
        _apiClient.GetAuthInfoResponse(Arg.Any<AuthInfoRequest>())
            .Returns(ApiResponseResult<AuthInfoResponse>.Ok(new HttpResponseMessage(),
                GetAuthInfoResponseWithEmptySalt()));
        SrpAuthenticator sut = GetSrpAuthenticator();

        // Act
        AuthResult result = await sut.LoginUserAsync(USERNAME, _password, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Failure.Should().BeTrue();
        result.Error.Should().Contain("Incorrect login credentials");
    }

    [TestMethod]
    public async Task AuthShouldFailWhenAuthInfoRequestFailsAsync()
    {
        // Arrange
        _apiClient.GetAuthInfoResponse(Arg.Any<AuthInfoRequest>())
            .Returns(ApiResponseResult<AuthInfoResponse>.Fail(new HttpResponseMessage(), API_AUTH_ERROR));
        SrpAuthenticator sut = GetSrpAuthenticator();

        // Act
        AuthResult result = await sut.LoginUserAsync(USERNAME, _password, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(AuthResult.Fail(API_AUTH_ERROR));
    }

    private AuthInfoResponse GetAuthInfoResponseWithEmptySalt()
    {
        AuthInfoResponse response = GetSuccessAuthInfoResponse();
        response.Salt = null;
        return response;
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

    private SrpAuthenticator GetSrpAuthenticator()
    {
        return new(_apiClient, _settings, _unauthSessionManager, _guestHoleManager);
    }
}