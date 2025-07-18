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

using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Common.Core.Tests.Networking;

[TestClass]
public class IpAddressTest
{
    [TestMethod]
    // IPv4 - Public
    [DataRow("8.8.8.8", true, "Google Public DNS")]
    [DataRow("1.1.1.1", true, "Cloudflare Public DNS")]
    [DataRow("208.67.222.222", true, "OpenDNS Public DNS")]

    // IPv4 - RFC 1918 Private Ranges
    [DataRow("192.168.1.1", false, "Private Class C")]
    [DataRow("10.0.0.1", false, "Private Class A")]
    [DataRow("172.16.0.1", false, "Private Class B (start of range)")]
    [DataRow("172.31.255.254", false, "Private Class B (end of range)")]
    [DataRow("172.15.255.255", true, "Just before Private Class B range")]
    [DataRow("172.32.0.0", true, "Just after Private Class B range")]
    [DataRow("10.255.255.255", false, "End of Class A private range")]
    [DataRow("192.168.255.255", false, "End of Class C private range")]

    // IPv4 - Other Special-Use Ranges
    [DataRow("127.0.0.1", false, "Loopback")]
    [DataRow("169.254.1.1", false, "Link-Local (APIPA)")]
    [DataRow("0.0.0.0", false, "This Network (unspecified)")]
    [DataRow("0.1.2.3", false, "This Network (part of 0.0.0.0/8)")]
    [DataRow("255.255.255.255", false, "Broadcast")]
    [DataRow("240.0.0.1", false, "Reserved (Class E)")]
    [DataRow("224.0.0.1", false, "Multicast (Class D)")]
    [DataRow("192.0.0.1", false, "IETF Protocol Assignments (192.0.0.0/24)")]
    [DataRow("192.88.99.1", false, "6to4 Relay Anycast (192.88.99.0/24)")]
    [DataRow("198.18.0.0", false, "Benchmarking (start of 198.18.0.0/15)")]
    [DataRow("198.19.255.255", false, "Benchmarking (end of 198.18.0.0/15)")]

    // IPv4 - Carrier-Grade NAT (RFC 6598)
    [DataRow("100.64.0.1", false, "Carrier-Grade NAT")]
    [DataRow("100.127.255.254", false, "Carrier-Grade NAT (end of range)")]
    [DataRow("100.63.255.255", true, "Address just before CGNAT range")]

    // IPv4 - Documentation/Test Ranges (RFC 5737)
    [DataRow("192.0.2.1", false, "Documentation TEST-NET-1")]
    [DataRow("198.51.100.1", false, "Documentation TEST-NET-2")]
    [DataRow("203.0.113.1", false, "Documentation TEST-NET-3")]

    // IPv6 - Public
    [DataRow("2001:4860:4860::8888", true, "Google Public DNS")]
    [DataRow("2606:4700:4700::1111", true, "Cloudflare Public DNS")]

    // IPv6 - Special-Use
    [DataRow("::1", false, "Loopback")]
    [DataRow("fe80::1", false, "Link-Local")]
    [DataRow("ff02::1", false, "Multicast")]
    [DataRow("::", false, "Unspecified (IPv6None)")]

    // IPv6 - Unique Local Address (ULA / RFC 4193) - "Private" IPv6
    [DataRow("fd12:3456:789a:1::1", false, "Unique Local Address")]
    [DataRow("fc00::beef", false, "Unique Local Address")]

    // IPv6 - Documentation Range (RFC 3849)
    [DataRow("2001:db8::dead:beef", false, "Documentation Range")]

    // IPv6 - Deprecated Site-Local (still good to check)
    [DataRow("fec0::1", false, "Site-Local (deprecated)")]

    // IPv6 - Teredo Tunneling (often not a "true" public endpoint)
    [DataRow("2001::1", false, "Teredo")]

    // IPv6 - Benchmarking
    [DataRow("2001:2::1", false, "Benchmarking (2001:2::/48)")]

    // IPv6 - NAT64 Well-Known Prefix
    [DataRow("64:ff9b::1", false, "NAT64 (64:ff9b::/96)")]
    [DataRow("64:ff9b:1::1", false, "NAT64 (64:ff9b:1::/48 variant)")]

    // IPv6 - Discard-Only Block
    [DataRow("100::1", false, "Discard-Only Block (100::/64)")]

    // IPv6 - 6to4 Transition
    [DataRow("2002::1", true, "6to4 transition (2002::/16)")]

    public void IsPublicIp_ShouldMatchExpectation(string ipString, bool expected, string description)
    {
        // Arrange
        IpAddress ipAddress = new(IPAddress.Parse(ipString));

        // Act
        bool result = ipAddress.IsPublicIp();

        // Assert
        Assert.AreEqual(expected, result, $"Failed on IP: {ipString} ({description})");
    }
}