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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Migrations;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Logic.Auth.Tests;

[TestClass]
public class UserAuthenticatorTest
{
    private const string API_AUTH_ERROR = "auth failed";
    private const string USERNAME = "username";
    private readonly SecureString _password = new NetworkCredential("", "password").SecurePassword;

    private IApiClient _apiClient;
    private ILogger _logger;
    private ISettings _settings;
    private IConnectionCertificateManager _connectionCertificateManager;
    private IEventMessageSender _eventMessageSender;
    private IGuestHoleManager _guestHoleManager;
    private ITokenClient _tokenClient;
    private IConnectionManager _connectionManager;
    private IServersUpdater _serversUpdater;
    private IUserSettingsMigrator _userSettingsMigrator;
    private IVpnPlanUpdater _vpnPlanUpdater;

    [TestInitialize]
    public void Initialize()
    {
        _apiClient = Substitute.For<IApiClient>();
        _logger = Substitute.For<ILogger>();
        _settings = Substitute.For<ISettings>();
        _connectionCertificateManager = Substitute.For<IConnectionCertificateManager>();
        _eventMessageSender = Substitute.For<IEventMessageSender>();
        _guestHoleManager = Substitute.For<IGuestHoleManager>();
        _tokenClient = Substitute.For<ITokenClient>();
        _connectionManager = Substitute.For<IConnectionManager>();
        _serversUpdater = Substitute.For<IServersUpdater>();
        _userSettingsMigrator = Substitute.For<IUserSettingsMigrator>();
        _vpnPlanUpdater = Substitute.For<IVpnPlanUpdater>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _apiClient = null;
        _logger = null;
        _settings = null;
        _connectionCertificateManager = null;
        _eventMessageSender = null;
        _guestHoleManager = null;
        _tokenClient = null;
        _connectionManager = null;
        _serversUpdater = null;
        _userSettingsMigrator = null;
        _vpnPlanUpdater = null;
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
        return new(_logger, _apiClient, _connectionCertificateManager, _settings, _eventMessageSender,
            _guestHoleManager, _tokenClient, _connectionManager, _serversUpdater,
            _userSettingsMigrator, _vpnPlanUpdater);
    }
}