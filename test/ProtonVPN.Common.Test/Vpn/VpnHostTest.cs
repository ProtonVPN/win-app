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

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Vpn;

namespace ProtonVPN.Common.Test.Vpn
{
    [TestClass]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public class VpnHostTest
    {
        [TestMethod]
        public void Name_ShouldBe_Name()
        {
            // Arrange
            const string expected = "server-1.protonvpn.com";
            var host = new VpnHost(expected, "127.0.0.1", string.Empty, null);

            // Act
            string result = host.Name;

            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void Ip_ShouldBe_Ip()
        {
            // Arrange
            const string expected = "44.55.66.77";
            var host = new VpnHost("server-1.protonvpn.com", expected, string.Empty, null);

            // Act
            string result = host.Ip;

            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void IsEmpty_ShouldBeTrue_WhenDefault()
        {
            // Arrange
            VpnHost host = default;

            // Act
            bool result = host.IsEmpty();

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsEmpty_ShouldBeTrue_WhenNew()
        {
            // Arrange
            var host = new VpnHost("name.com", "0.0.0.0", string.Empty, null);

            // Act
            bool result = host.IsEmpty();

            // Assert
            result.Should().BeFalse();
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("server.\"proton.com")]
        [DataRow("server.proton.com\"")]
        public void VpnHost_ShouldThrow_WhenNameIsNotValid(string name)
        {
            // Act
            Action action = () => new VpnHost(name, "127.0.0.1", string.Empty, null);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("158.159.247")]
        [DataRow("127.0.0.4 ")]
        [DataRow("-127.0.0.4")]
        [DataRow("\"27.0.0.4")]
        [DataRow("227.0.0.4\"")]
        public void VpnHost_ShouldThrow_WhenIpIsNotValid(string ip)
        {
            // Act
            Action action = () => new VpnHost("test.server.com", ip, string.Empty, null);

            // Assert
            action.Should().Throw<ArgumentException>();
        }
    }
}