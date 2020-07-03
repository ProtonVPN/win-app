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

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using ProtonVPN.Common.Vpn;
using ProtonVPN.ConnectionInfo;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.App.Test.ConnectionInfo
{
    [TestClass]
    public class ConnectionErrorResolverTest
    {
        private IApiClient _apiClient;
        private IUserStorage _userStorage;
        private IServerUpdater _serverUpdater;
        private ServerManager _serverManager;

        [TestInitialize]
        public void TestInitialize()
        {
            _apiClient = Substitute.For<IApiClient>();
            _userStorage = Substitute.For<IUserStorage>();
            _serverUpdater = Substitute.For<IServerUpdater>();
            _serverManager = Substitute.For<ServerManager>(_userStorage);

            var result = ApiResponseResult<VpnInfoResponse>.Ok(new VpnInfoResponse
            {
                Code = 1000,
                Error = string.Empty
            });
            _apiClient.GetVpnInfoResponse().Returns(Task.FromResult(result));
        }

        [TestMethod]
        public void ResolveError_ShouldReturnSessionLimitReached()
        {
            // Arrange
            _userStorage.User().Returns(new User
            {
                MaxConnect = 1
            });
            SetSessions(3);

            var sut = new ConnectionErrorResolver(_userStorage, _apiClient, _serverManager, _serverUpdater);

            // Assert
            sut.ResolveError().Result.Should().Be(VpnError.SessionLimitReached);
        }

        [TestMethod]
        public void ResolveError_ShouldReturnPasswordChanged()
        {
            // Arrange
            var oldUserInfo = new User
            {
                VpnPassword = "old password",
                MaxConnect = 3
            };

            var newUserInfo = new User
            {
                VpnPassword = "new password",
                MaxConnect = 3
            };

            _userStorage.User().Returns(oldUserInfo, newUserInfo);

            SetSessions(0);

            var sut = new ConnectionErrorResolver(_userStorage, _apiClient, _serverManager, _serverUpdater);

            // Assert
            sut.ResolveError().Result.Should().Be(VpnError.PasswordChanged);
        }

        [TestMethod]
        public void ResolveError_ShouldReturnTierTooLow()
        {
            // Arrange
            var oldUserInfo = new User
            {
                VpnPassword = "old password",
                MaxConnect = 3,
                MaxTier = 2
            };

            var newUserInfo = new User
            {
                VpnPassword = "old password",
                MaxConnect = 3,
                MaxTier = 1
            };

            _userStorage.User().Returns(oldUserInfo, newUserInfo);

            SetSessions(0);

            var sut = new ConnectionErrorResolver(_userStorage, _apiClient, _serverManager, _serverUpdater);

            // Assert
            sut.ResolveError().Result.Should().Be(VpnError.UserTierTooLowError);
        }

        [TestMethod]
        public void ResolveError_ShouldReturnUnpaid()
        {
            // Arrange
            _userStorage.User().Returns(new User
            {
                Delinquent = 3
            });

            var sut = new ConnectionErrorResolver(_userStorage, _apiClient, _serverManager, _serverUpdater);

            // Assert
            sut.ResolveError().Result.Should().Be(VpnError.Unpaid);
        }

        [TestMethod]
        public void ResolveError_ShouldReturnServerRemoved()
        {
            // Arrange
            _userStorage.User().Returns(new User {MaxConnect = 3});
            _serverManager.GetServer(Arg.Any<ISpecification<LogicalServerContract>>()).ReturnsNull();

            var sut = new ConnectionErrorResolver(_userStorage, _apiClient, _serverManager, _serverUpdater);
            SetSessions(0);

            // Assert
            sut.ResolveError().Result.Should().Be(VpnError.ServerRemoved);
        }

        [TestMethod]
        public void ResolveError_ShouldReturnServerOffline()
        {
            // Arrange
            _userStorage.User().Returns(new User { MaxConnect = 3 });
            _serverManager.GetServer(Arg.Any<ISpecification<LogicalServerContract>>()).Returns(Server.Empty());

            var sut = new ConnectionErrorResolver(_userStorage, _apiClient, _serverManager, _serverUpdater);
            SetSessions(0);

            // Assert
            sut.ResolveError().Result.Should().Be(VpnError.ServerOffline);
        }

        [TestMethod]
        public void ResolveError_ShouldReturnUnknownError()
        {
            // Arrange
            _userStorage.User().Returns(new User
            {
                Delinquent = 0,
                MaxConnect = 3,
                MaxTier = 2
            });
            _serverManager.GetServer(Arg.Any<ISpecification<LogicalServerContract>>()).Returns(GetOnlineServer());

            var sut = new ConnectionErrorResolver(_userStorage, _apiClient, _serverManager, _serverUpdater);
            SetSessions(0);

            // Assert
            sut.ResolveError().Result.Should().Be(VpnError.Unknown);
        }

        private void SetSessions(int number)
        {
            var sessions = new List<Session>();
            for (var i = 0; i < number; i++)
            {
                sessions.Add(new Session());
            }

            _apiClient.GetSessions().Returns(Task.FromResult(ApiResponseResult<SessionsResponse>.Ok(new SessionsResponse
            {
                Code = 1000,
                Error = string.Empty,
                Sessions = sessions
            })));
        }

        private Server GetOnlineServer()
        {
            return new Server("", "", "", "", "", "", 1, ServerTiers.Basic, 0, 0, 0, new Location(),
                new List<PhysicalServer>(), "");
        }
    }
}
