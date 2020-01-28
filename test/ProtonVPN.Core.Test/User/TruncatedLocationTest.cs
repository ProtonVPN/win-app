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
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;

namespace ProtonVPN.Core.Test.User
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
            var userLocation = new UserLocation(ip, 10.0f, 10.0f, "ISP", "ZZ");
            _userStorage.Location().Returns(userLocation);
            var location = new TruncatedLocation(_userStorage);

            // Act
            var result = location.Ip();

            // Assert
            result.Split('.')[3].Should().Be("0");
        }

        [TestMethod]
        public void Value_ShouldBeEmpty_WhenIpIsNull()
        {
            // Arrange
            var userLocation = new UserLocation(null, 10.0f, 10.0f, "ISP", "ZZ");
            _userStorage.Location().Returns(userLocation);
            var location = new TruncatedLocation(_userStorage);

            // Act
            var result = location.Ip();

            // Assert
            result.Should().Be(string.Empty);
        }

        [TestMethod]
        public void Value_ShouldBeEmpty_WhenIpIsEmpty()
        {
            // Arrange
            var userLocation = new UserLocation(string.Empty, 10.0f, 10.0f, "ISP", "ZZ");
            _userStorage.Location().Returns(userLocation);
            var location = new TruncatedLocation(_userStorage);

            // Act
            var result = location.Ip();

            // Assert
            result.Should().Be(string.Empty);
        }
    }
}
