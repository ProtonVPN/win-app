/*
 * Copyright (c) 2024 Proton AG
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
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Common.Core.Tests.Extensions;

[TestClass]
public class StringExtensionsTest
{
    [TestMethod]
    [DataRow("", false)]                                                // Invalid (empty string)
    [DataRow(" ", false)]                                               // Invalid (whitespace only)
    [DataRow(null, false)]                                              // Invalid (null string)
    [DataRow("proton.me", true)]                                        // Valid URL without protocol
    [DataRow("www.proton.me", true)]                                    // Valid URL with "www"
    [DataRow("http://proton.me", true)]                                 // Valid URL with "http" protocol
    [DataRow("https://proton.me", true)]                                // Valid URL with "https" protocol
    [DataRow("https://www.protonvpn.com", true)]                        // Valid URL with "www" and subdomain
    [DataRow("https://www.protonvpn.com/features", true)]               // Valid URL with path
    [DataRow("https://www.protonvpn.com/features?q=test", true)]        // Valid URL with query string
    [DataRow("ftp://ftp.proton.me", true)]                              // Valid FTP URL
    [DataRow("customprotocol://protonapp", true)]                       // Valid custom protocol
    [DataRow("https://protonvpn.com:8080", true)]                       // Valid URL with port
    [DataRow("https://protonvpn.com/#features", true)]                  // Valid URL with fragment
    [DataRow("https://protonvpn.com/path/to/resource", true)]           // Valid URL with long path
    [DataRow("https://protonvpn.com/search?q=abc+def&l=en-US", true)]   // Valid URL with parameters
    [DataRow("https://blog.protonvpn.com", true)]                       // Valid URL with subdomain
    [DataRow("https://www.proton.vpn", true)]                           // Valid URL with uncommon TLD
    [DataRow("http://255.255.255.255", true)]                           // Valid URL with IPv4
    [DataRow("http://[2001:db8::1]", true)]                             // Valid URL with IPv6
    [DataRow("http://[2001:db8::1]:8080", true)]                        // Valid URL with IPv6 and port
    [DataRow("http://[::1]", true)]                                     // Valid URL with loopback IPv6
    [DataRow("//proton.me", false)]                                     // Invalid URL (missing protocol)
    [DataRow("https:// proton.me", false)]                              // Invalid URL (space in domain)
    [DataRow("https:/proton.me", false)]                                // Invalid URL (malformed protocol)
    [DataRow("http://.me", false)]                                      // Invalid URL (missing domain name)
    [DataRow("http://proton..me", false)]                               // Invalid URL (double dots in domain)
    [DataRow("https://protonvpn.com:abcd", false)]                      // Invalid (non-numeric port)
    [DataRow("http:/proton.me", false)]                                 // Invalid URL (single /)
    public void TestUrlValidation(string url, bool expectedResult)
    {
        bool result = url.IsValidUrl();
        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow("", false)]
    [DataRow("1.2.3", false)]
    [DataRow("1.2.3.4.5", false)]
    [DataRow("1.2.3.", false)]
    [DataRow(".1.2.3", false)]
    [DataRow("1..2.3", false)]
    [DataRow("a.b.c.d", false)]
    [DataRow("proton.com", false)]
    [DataRow("0.0.0.0", true)]
    [DataRow("0.0.512", false)]
    [DataRow("255.255.255.255", true)]
    [DataRow("256.0.0.0", false)]
    [DataRow("1.2.3.4", true)]
    [DataRow("1.2.3.0/24", false)]
    [DataRow("1.2.3.0/0", false)]
    [DataRow("1.2.3.0/32", false)]
    [DataRow("1.2.3.0/35", false)]
    [DataRow("1.2.3.0/00024", false)]
    [DataRow("1.2.3.0/", false)]
    [DataRow("1.2.3.0/abc", false)]
    [DataRow("0.0.512/32", false)]
    [DataRow("0.0.512/16", false)]
    [DataRow("1", false)]
    [DataRow("1.2.3.0/24/16", false)]
    [DataRow("1.2.3.0/24.16", false)]
    public void TestIpAddressFormatValidation(string ipAddress, bool expectedResult)
    {
        bool result = ipAddress.IsValidIpAddressFormat();
        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow("", false)]
    [DataRow("1.2.3", false)]
    [DataRow("1.2.3.4.5", false)]
    [DataRow("1.2.3.", false)]
    [DataRow(".1.2.3", false)]
    [DataRow("1..2.3", false)]
    [DataRow("a.b.c.d", false)]
    [DataRow("proton.com", false)]
    [DataRow("0.0.0.0", true)]
    [DataRow("0.0.512", false)]
    [DataRow("255.255.255.255", true)]
    [DataRow("256.0.0.0", false)]
    [DataRow("1.2.3.4", true)]
    [DataRow("1.2.3.0/24", false)]
    [DataRow("1.2.3.0/0", false)]
    [DataRow("1.2.3.0/32", true)]
    [DataRow("1.2.3.0/35", false)]
    [DataRow("1.2.3.0/00024", false)]
    [DataRow("1.2.3.0/", false)]
    [DataRow("1.2.3.0/abc", false)]
    [DataRow("0.0.512/32", false)]
    [DataRow("0.0.512/16", false)]
    [DataRow("1", false)]
    [DataRow("1.2.3.0/24/16", false)]
    [DataRow("1.2.3.0/24.16", false)]
    public void TestIpAddressValidation(string ipAddress, bool expectedResult)
    {
        bool result = ipAddress.IsValidIpAddress();
        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow("", false)]
    [DataRow("1.2.3", false)]
    [DataRow("1.2.3.4.5", false)]
    [DataRow("1.2.3.", false)]
    [DataRow(".1.2.3", false)]
    [DataRow("1..2.3", false)]
    [DataRow("a.b.c.d", false)]
    [DataRow("proton.com", false)]
    [DataRow("0.0.0.0", true)]
    [DataRow("0.0.512", false)]
    [DataRow("255.255.255.255", true)]
    [DataRow("256.0.0.0", false)]
    [DataRow("1.2.3.4", true)]
    [DataRow("1.2.3.0/24", true)]
    [DataRow("1.2.3.0/0", true)]
    [DataRow("1.2.3.0/32", true)]
    [DataRow("1.2.3.0/35", false)]
    [DataRow("1.2.3.0/00024", true)]
    [DataRow("1.2.3.0/", false)]
    [DataRow("1.2.3.0/abc", false)]
    [DataRow("0.0.512/32", false)]
    [DataRow("0.0.512/16", false)]
    [DataRow("1", false)]
    [DataRow("1.2.3.0/24/16", false)]
    [DataRow("1.2.3.0/24.16", false)]
    public void TestIpAddressOrRangeValidation(string ipAddress, bool expectedResult)
    {
        bool result = ipAddress.IsValidIpAddressOrRange();
        Assert.AreEqual(expectedResult, result);
    }
}