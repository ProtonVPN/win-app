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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;

namespace ProtonVPN.Common.Tests.Vpn
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
            VpnHost host = new(expected, "127.0.0.1", string.Empty, null, string.Empty, false, null);

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
            VpnHost host = new("server-1.protonvpn.com", expected, string.Empty, null, string.Empty, false, null);

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
            VpnHost host = new("name.com", "0.0.0.0", string.Empty, null, string.Empty, false, null);

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
            Action action = () => new VpnHost(name, "127.0.0.1", string.Empty, null, string.Empty, false, null);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void VpnHost_ShouldNotThrow_WhenIpIsNullOrEmpty(string ip)
        {
            // Act
            VpnHost host = new("test.server.com", ip, string.Empty, null, string.Empty, false, null);

            // Act
            string result = host.Ip;

            // Assert
            result.Should().Be(ip);
        }

        [DataTestMethod]
        [DataRow("158.159.247")]
        [DataRow("127.0.0.4 ")]
        [DataRow("-127.0.0.4")]
        [DataRow("\"27.0.0.4")]
        [DataRow("227.0.0.4\"")]
        public void VpnHost_ShouldThrow_WhenIpIsNotValid(string ip)
        {
            // Act
            Action action = () => new VpnHost("test.server.com", ip, string.Empty, null, string.Empty, false, null);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [DataTestMethod]
        [DataRow("158.159.247")]
        [DataRow("127.0.0.4 ")]
        [DataRow("-127.0.0.4")]
        [DataRow("\"27.0.0.4")]
        [DataRow("227.0.0.4\"")]
        public void VpnHost_ShouldThrow_WhenRelayIpIsNotValid(string ip)
        {
            Dictionary<VpnProtocol, string> dictionary = new Dictionary<VpnProtocol, string>()
            {
                { VpnProtocol.WireGuardUdp, ip }
            };

            // Act
            Action action = () => new VpnHost("test.server.com", null, string.Empty, null, string.Empty, false, null);
        }
    }
}