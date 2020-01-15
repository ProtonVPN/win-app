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

using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Vpn.OpenVpn.Arguments;

namespace ProtonVPN.Vpn.Test.OpenVpn.Arguments
{
    [TestClass]
    public class LocalHostArgumentsTest
    {
        [TestMethod]
        public void Enumerable_ShouldContain_ExpectedNumberOfOptions()
        {
            // Arrange
            var subject = new LocalHostArguments("44.55.66.77");

            // Act
            var result = subject.ToList();

            // Assert
            result.Should().HaveCount(2);
        }

        [TestMethod]
        public void Enumerable_ShouldContain_LocalOption()
        {
            const string localIp = "192.168.0.15";

            // Arrange
            var subject = new LocalHostArguments(localIp);

            // Act
            var result = subject.ToList();

            // Assert
            result.Should().Contain($"--local {localIp}");
        }

        [TestMethod]
        public void Enumerable_ShouldContain_LPortOption()
        {
            // Arrange
            var subject = new LocalHostArguments("1.2.3.4");

            // Act
            var result = subject.ToList();

            // Assert
            result.Should().Contain($"--lport 0");
        }
    }
}
