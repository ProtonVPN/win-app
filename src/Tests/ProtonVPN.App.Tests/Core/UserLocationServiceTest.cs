/*
 * Copyright (c) 2024 Proton AG
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

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Api;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Core;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;

namespace ProtonVPN.App.Tests.Core
{
    [TestClass]
    public class UserLocationServiceTest
    {
        private readonly GuestHoleState _guestHoleState = new();
        private IApiClient _apiClient;
        private IUserStorage _userStorage;
        private INetworkInterfaces _networkInterfaces;
        private IUserAuthenticator _userAuthenticator;

        [TestInitialize]
        public void TestInitialize()
        {
            _apiClient = Substitute.For<IApiClient>();
            _userStorage = Substitute.For<IUserStorage>();
            _networkInterfaces = Substitute.For<INetworkInterfaces>();
            _userAuthenticator = Substitute.For<IUserAuthenticator>();
        }

        [TestMethod]
        [DataRow("85.24.60.44")]
        [DataRow("198.11.10.11")]
        public async Task Value_ShouldBe_IpWithZeroedLastOctetAsync(string ip)
        {
            // Arrange
            UserLocation userLocation = new(ip, "ISP", "ZZ");
            _userStorage.GetLocation().Returns(userLocation);

            // Act
            UserLocationService userLocationService = GetUserLocationService();

            // Assert
            (await userLocationService.GetTruncatedIpAddressAsync()).Split('.')[3].Should().Be("0");
        }

        [TestMethod]
        public async Task Value_ShouldBeEmpty_WhenIpIsNullAsync()
        {
            // Arrange
            UserLocation userLocation = new(null, "ISP", "ZZ");
            _userStorage.GetLocation().Returns(userLocation);

            // Act
            UserLocationService userLocationService = GetUserLocationService();

            // Assert
            (await userLocationService.GetTruncatedIpAddressAsync()).Should().Be(string.Empty);
        }

        [TestMethod]
        public async Task Value_ShouldBeEmpty_WhenIpIsEmptyAsync()
        {
            // Arrange
            UserLocation userLocation = new(string.Empty, "ISP", "ZZ");
            _userStorage.GetLocation().Returns(userLocation);

            // Act
            UserLocationService userLocationService = GetUserLocationService();

            // Assert
            (await userLocationService.GetTruncatedIpAddressAsync()).Should().Be(string.Empty);
        }

        private UserLocationService GetUserLocationService()
        {
            return new(_apiClient, _userStorage, _networkInterfaces, _userAuthenticator, _guestHoleState);
        }
    }
}