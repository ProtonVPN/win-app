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

using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Vpn.OpenVpn.Arguments;

namespace ProtonVPN.Vpn.Test.OpenVpn.Arguments
{
    [TestClass]
    public class ManagementArgumentsTest
    {
        private OpenVpnConfig _config;

        [TestInitialize]
        public void TestInitialize()
        {
            _config = Substitute.For<OpenVpnConfig>();
        }

        [TestMethod]
        public void Enumerable_ShouldContain_ExpectedNumberOfOptions()
        {
            // Arrange
            var subject = new ManagementArguments(_config, 333);

            // Act
            var result = subject.ToList();

            // Assert
            result.Should().HaveCount(3);
        }

        [TestMethod]
        public void Enumerable_ShouldContain_ManagementOption()
        {
            const int managementPort = 4444;

            // Arrange
            _config.ManagementHost = "127.0.0.5";
            var subject = new ManagementArguments(_config, managementPort);

            // Act
            var result = subject.ToList();

            // Assert
            result.Should().Contain($"--management {_config.ManagementHost} {managementPort} stdin");
        }

        [TestMethod]
        public void Enumerable_ShouldContain_ManagementQueryPasswordsOption()
        {
            // Arrange
            var subject = new ManagementArguments(_config, 55);

            // Act
            var result = subject.ToList();

            // Assert
            result.Should().Contain("--management-query-passwords");
        }

        [TestMethod]
        public void Enumerable_ShouldContain_ManagementHoldOption()
        {
            // Arrange
            var subject = new ManagementArguments(_config, 66);

            // Act
            var result = subject.ToList();

            // Assert
            result.Should().Contain($"--management-hold");
        }
    }
}
