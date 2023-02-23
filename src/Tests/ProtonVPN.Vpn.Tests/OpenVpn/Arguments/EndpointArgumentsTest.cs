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
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.OpenVpn.Arguments;

namespace ProtonVPN.Vpn.Tests.OpenVpn.Arguments
{
    [TestClass]
    [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
    public class EndpointArgumentsTest
    {
        [TestMethod]
        public void Enumerable_ShouldContain_ExpectedNumberOfOptions()
        {
            // Arrange
            VpnEndpoint endpoint =
                new(new VpnHost("abc.com", "4.5.6.7", string.Empty, null, string.Empty), VpnProtocol.OpenVpnUdp, 48965);
            OpenVpnEndpointArguments subject = new(endpoint);

            // Act
            List<string> result = subject.ToList();

            // Assert
            result.Should().HaveCount(1);
        }

        [TestMethod]
        public void Enumerable_ShouldContain_RemoteOption()
        {
            // Arrange
            VpnEndpoint endpoint =
                new(new VpnHost("abc.com", "11.22.33.44", string.Empty, null, string.Empty), VpnProtocol.OpenVpnUdp, 61874);
            OpenVpnEndpointArguments subject = new(endpoint);

            // Act
            List<string> result = subject.ToList();

            // Assert
            result.First().Should().StartWith($"--remote {endpoint.Server.Ip} {endpoint.Port}");
        }

        [DataTestMethod]
        [DataRow(VpnProtocol.OpenVpnUdp, "udp")]
        [DataRow(VpnProtocol.OpenVpnTcp, "tcp")]
        public void Enumerable_ShouldMap_VpnProtocol(VpnProtocol protocol, string expected)
        {
            // Arrange
            VpnEndpoint endpoint =
                new(new VpnHost("abc.com", "7.7.7.7", string.Empty, null, string.Empty), protocol, 44444);
            OpenVpnEndpointArguments subject = new(endpoint);

            // Act
            List<string> result = subject.ToList();

            // Assert
            result.Should().Contain($"--remote {endpoint.Server.Ip} {endpoint.Port} {expected}");
        }

        [DataTestMethod]
        [DataRow(VpnProtocol.Smart)]
        public void Enumerable_ShouldThrow_WhenProtocolIsNotSupported(VpnProtocol protocol)
        {
            // Arrange
            VpnEndpoint endpoint =
                new(new VpnHost("abc.com", "1.2.3.4", string.Empty, null, string.Empty), protocol, 54321);
            OpenVpnEndpointArguments subject = new(endpoint);

            // Act
            Action action = () => subject.ToList();

            // Assert
            action.Should().Throw<ArgumentException>();
        }
    }
}