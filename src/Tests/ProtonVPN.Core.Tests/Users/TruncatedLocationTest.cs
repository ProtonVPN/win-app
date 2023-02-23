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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;

namespace ProtonVPN.Core.Tests.Users
{
    [TestClass]
    public class TruncatedLocationTest
    {
        private IUserStorage _userStorage;

        [TestInitialize]
        public void TestInitialize()
        {
            _userStorage = Substitute.For<IUserStorage>();
        }

        [TestMethod]
        [DataRow("85.24.60.44")]
        [DataRow("198.11.10.11")]
        public void Value_ShouldBe_IpWithZeroedLastOctet(string ip)
        {
            // Arrange
            UserLocation userLocation = new(ip, "ISP", "ZZ");
            _userStorage.GetLocation().Returns(userLocation);
            TruncatedLocation location = new(_userStorage);

            // Act
            string result = location.Ip();

            // Assert
            result.Split('.')[3].Should().Be("0");
        }

        [TestMethod]
        public void Value_ShouldBeEmpty_WhenIpIsNull()
        {
            // Arrange
            UserLocation userLocation = new(null, "ISP", "ZZ");
            _userStorage.GetLocation().Returns(userLocation);
            TruncatedLocation location = new(_userStorage);

            // Act
            string result = location.Ip();

            // Assert
            result.Should().Be(string.Empty);
        }

        [TestMethod]
        public void Value_ShouldBeEmpty_WhenIpIsEmpty()
        {
            // Arrange
            UserLocation userLocation = new(string.Empty, "ISP", "ZZ");
            _userStorage.GetLocation().Returns(userLocation);
            TruncatedLocation location = new(_userStorage);

            // Act
            string result = location.Ip();

            // Assert
            result.Should().Be(string.Empty);
        }
    }
}