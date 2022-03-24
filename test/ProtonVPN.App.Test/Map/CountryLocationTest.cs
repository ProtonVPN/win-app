/*
 * Copyright (c) 2022 Proton Technologies AG
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
using ProtonVPN.Map;

namespace ProtonVPN.App.Test.Map
{
    [TestClass]
    public class CountryLocationTest
    {
        [TestMethod]
        public void Coordinates_Should_NotBeZero_WhenKnownCountry()
        {
            // Arrange
            var subject = new CountryLocation("GB");

            // Act
            var coordinates = subject.Coordinates();

            // Assert
            coordinates.X.Should().NotBe(0.0);
            coordinates.Y.Should().NotBe(0.0);
        }

        [TestMethod]
        public void Coordinates_Should_BeZero_WhenUnknownCountry()
        {
            // Arrange
            var subject = new CountryLocation("00");

            // Act
            var coordinates = subject.Coordinates();

            // Assert
            coordinates.X.Should().Be(0.0);
            coordinates.Y.Should().Be(0.0);
        }
    }
}
