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
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Vpn.OpenVpn.Arguments;

namespace ProtonVPN.Vpn.Tests.OpenVpn.Arguments
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
            ManagementArguments subject = new(_config, 333);

            // Act
            List<string> result = subject.ToList();

            // Assert
            result.Should().HaveCount(3);
        }

        [TestMethod]
        public void Enumerable_ShouldContain_ManagementOption()
        {
            const int managementPort = 4444;

            // Arrange
            _config.ManagementHost = "127.0.0.5";
            ManagementArguments subject = new(_config, managementPort);

            // Act
            List<string> result = subject.ToList();

            // Assert
            result.Should().Contain($"--management {_config.ManagementHost} {managementPort} stdin");
        }

        [TestMethod]
        public void Enumerable_ShouldContain_ManagementQueryPasswordsOption()
        {
            // Arrange
            ManagementArguments subject = new(_config, 55);

            // Act
            List<string> result = subject.ToList();

            // Assert
            result.Should().Contain("--management-query-passwords");
        }

        [TestMethod]
        public void Enumerable_ShouldContain_ManagementHoldOption()
        {
            // Arrange
            ManagementArguments subject = new(_config, 66);

            // Act
            List<string> result = subject.ToList();

            // Assert
            result.Should().Contain($"--management-hold");
        }
    }
}
