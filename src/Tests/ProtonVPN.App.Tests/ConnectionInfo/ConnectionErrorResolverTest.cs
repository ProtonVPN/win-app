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

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using ProtonVPN.Account;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Api.Contracts.VpnSessions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Vpn;
using ProtonVPN.ConnectionInfo;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.App.Tests.ConnectionInfo
{
    [TestClass]
    public class ConnectionErrorResolverTest
    {
        private IApiClient _apiClient;
        private IUserStorage _userStorage;
        private IServerUpdater _serverUpdater;
        private IVpnInfoUpdater _vpnInfoUpdater;
        private ServerManager _serverManager;
        private IAppSettings _appSettings;
        private ILogger _logger;

        [TestInitialize]
        public void TestInitialize()
        {
            _apiClient = Substitute.For<IApiClient>();
            _userStorage = Substitute.For<IUserStorage>();
            _serverUpdater = Substitute.For<IServerUpdater>();
            _vpnInfoUpdater = Substitute.For<IVpnInfoUpdater>();
            _appSettings = Substitute.For<IAppSettings>();
            _logger = Substitute.For<ILogger>();
            _serverManager = Substitute.For<ServerManager>(_userStorage, _appSettings, _logger);

            ApiResponseResult<VpnInfoWrapperResponse> result = ApiResponseResult<VpnInfoWrapperResponse>.Ok(
                new HttpResponseMessage(), new VpnInfoWrapperResponse()
            {
                Code = 1000,
                Error = string.Empty
            });
            _apiClient.GetVpnInfoResponse().Returns(Task.FromResult(result));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _apiClient = null;
            _userStorage = null;
            _serverUpdater = null;
            _appSettings = null;
            _logger = null;
            _serverManager = null;
        }

        [TestMethod]
        public void ResolveError_ShouldReturnSessionLimitReached()
        {
            // Arrange
            _userStorage.GetUser().Returns(new User
            {
                MaxConnect = 1
            });
            SetSessions(3);

            ConnectionErrorResolver sut = new(_userStorage, _apiClient, _serverManager, _vpnInfoUpdater, _serverUpdater);

            // Assert
            sut.ResolveError().Result.Should().Be(VpnError.SessionLimitReached);
        }

        [TestMethod]
        public void ResolveError_ShouldReturnTierTooLow()
        {
            // Arrange
            User oldUserInfo = new()
            {
                MaxConnect = 3,
                MaxTier = 2
            };

            User newUserInfo = new()
            {
                MaxConnect = 3,
                MaxTier = 1
            };

            _userStorage.GetUser().Returns(oldUserInfo, newUserInfo);

            SetSessions(0);

            ConnectionErrorResolver sut = new(_userStorage, _apiClient, _serverManager, _vpnInfoUpdater, _serverUpdater);

            // Assert
            sut.ResolveError().Result.Should().Be(VpnError.UserTierTooLowError);
        }

        [TestMethod]
        public void ResolveError_ShouldReturnUnpaid()
        {
            // Arrange
            _userStorage.GetUser().Returns(new User
            {
                Delinquent = 3
            });

            ConnectionErrorResolver sut = new(_userStorage, _apiClient, _serverManager, _vpnInfoUpdater, _serverUpdater);

            // Assert
            sut.ResolveError().Result.Should().Be(VpnError.Unpaid);
        }

        [TestMethod]
        public void ResolveError_ShouldReturnServerRemoved()
        {
            // Arrange
            _userStorage.GetUser().Returns(new User {MaxConnect = 3});
            _serverManager.GetServer(Arg.Any<ISpecification<LogicalServerResponse>>()).ReturnsNull();

            ConnectionErrorResolver sut = new(_userStorage, _apiClient, _serverManager, _vpnInfoUpdater, _serverUpdater);
            SetSessions(0);

            // Assert
            sut.ResolveError().Result.Should().Be(VpnError.ServerRemoved);
        }

        [TestMethod]
        public void ResolveError_ShouldReturnServerOffline()
        {
            // Arrange
            _userStorage.GetUser().Returns(new User { MaxConnect = 3 });
            _serverManager.GetServer(Arg.Any<ISpecification<LogicalServerResponse>>()).Returns(Server.Empty());

            ConnectionErrorResolver sut = new(_userStorage, _apiClient, _serverManager, _vpnInfoUpdater, _serverUpdater);
            SetSessions(0);

            // Assert
            sut.ResolveError().Result.Should().Be(VpnError.ServerOffline);
        }

        [TestMethod]
        public void ResolveError_ShouldReturnUnknownError()
        {
            // Arrange
            _userStorage.GetUser().Returns(new User
            {
                Delinquent = 0,
                MaxConnect = 3,
                MaxTier = 2
            });
            _serverManager.GetServer(Arg.Any<ISpecification<LogicalServerResponse>>()).Returns(GetOnlineServer());

            ConnectionErrorResolver sut = new(_userStorage, _apiClient, _serverManager, _vpnInfoUpdater, _serverUpdater);
            SetSessions(0);

            // Assert
            sut.ResolveError().Result.Should().Be(VpnError.Unknown);
        }

        private void SetSessions(int number)
        {
            List<SessionResponse> sessions = new();
            for (int i = 0; i < number; i++)
            {
                sessions.Add(new());
            }

            _apiClient.GetSessions().Returns(Task.FromResult(ApiResponseResult<SessionsResponse>.Ok(
                new HttpResponseMessage(),
                new SessionsResponse { Code = 1000, Error = string.Empty, Sessions = sessions })));
        }

        private Server GetOnlineServer()
        {
            return new Server("", "", "", "", "", "", 1, ServerTiers.Basic, 0, 0, 0, new LocationResponse(),
                new List<PhysicalServer>(), "");
        }
    }
}