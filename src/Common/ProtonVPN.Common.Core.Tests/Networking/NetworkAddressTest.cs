/*
 * Copyright (c) 2025 Proton AG
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Common.Core.Tests.Networking;

[TestClass]
public class NetworkAddressTest
{
    [TestMethod]
    public void GetSubnetMaskString_NoSubnet_ReturnsDefaultMask()
    {
        // Arrange
        Assert.IsTrue(NetworkAddress.TryParse("192.168.1.1", out NetworkAddress networkAddress), "TryParse should succeed for a valid IPv4 without CIDR.");

        // Act
        string mask = networkAddress.GetSubnetMaskString();

        // Assert
        Assert.AreEqual("255.255.255.255", mask);
    }

    [DataTestMethod]
    [DataRow(0, "0.0.0.0")]
    [DataRow(1, "128.0.0.0")]
    [DataRow(8, "255.0.0.0")]
    [DataRow(16, "255.255.0.0")]
    [DataRow(24, "255.255.255.0")]
    [DataRow(32, "255.255.255.255")]
    public void GetSubnetMaskString_ValidIpv4Subnets_ReturnsCorrectMask(int cidr, string expectedMask)
    {
        // Arrange
        string rawAddress = $"10.0.0.0/{cidr}";
        Assert.IsTrue(NetworkAddress.TryParse(rawAddress, out NetworkAddress networkAddress), $"TryParse should succeed for {rawAddress}.");

        // Act
        string mask = networkAddress.GetSubnetMaskString();

        // Assert
        Assert.AreEqual(expectedMask, mask, $"CIDR /{cidr} should produce mask {expectedMask}.");
    }

    [TestMethod]
    public void GetSubnetMaskString_Ipv6Address_ThrowsInvalidOperationException()
    {
        // Arrange
        Assert.IsTrue(NetworkAddress.TryParse("2001:db8::/64", out NetworkAddress networkAddress), "TryParse should succeed for valid IPv6.");

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(
            () => networkAddress.GetSubnetMaskString(),
            "Calling GetSubnetMaskString on IPv6 should throw InvalidOperationException."
        );
    }
}