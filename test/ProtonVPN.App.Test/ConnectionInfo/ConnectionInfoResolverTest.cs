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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Vpn;
using ProtonVPN.ConnectionInfo;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProtonVPN.App.Test.ConnectionInfo
{
    [TestClass]
    public class ConnectionErrorResolverTest
    {
        private IApiClient _apiClient;
        private IUserStorage _userStorage;

        [TestInitialize]
        public void TestInitialize()
        {
            _apiClient = Substitute.For<IApiClient>();
            _userStorage = Substitute.For<IUserStorage>();

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

            var sut = new ConnectionErrorResolver(_userStorage, _apiClient);

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

            var sut = new ConnectionErrorResolver(_userStorage, _apiClient);

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

            var sut = new ConnectionErrorResolver(_userStorage, _apiClient);

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

            var sut = new ConnectionErrorResolver(_userStorage, _apiClient);

            // Assert
            sut.ResolveError().Result.Should().Be(VpnError.Unpaid);
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

            var sut = new ConnectionErrorResolver(_userStorage, _apiClient);
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
    }
}
