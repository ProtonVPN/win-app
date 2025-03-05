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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;

namespace ProtonVPN.Client.Logic.Servers.Tests;

[TestClass]
public class ServerFeatureExtensionsTest
{
    [TestMethod]
    [DataRow(ServerFeatures.SecureCore, ServerFeatures.P2P, false)]
    [DataRow(ServerFeatures.SecureCore, ServerFeatures.SecureCore, true)]
    [DataRow(ServerFeatures.SecureCore, ServerFeatures.P2P | ServerFeatures.Tor | ServerFeatures.Restricted, false)]
    [DataRow(ServerFeatures.SecureCore, ServerFeatures.SecureCore | ServerFeatures.Tor | ServerFeatures.Restricted, true)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.Restricted, ServerFeatures.SecureCore, false)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.Restricted, ServerFeatures.P2P, true)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.Restricted, ServerFeatures.Restricted, true)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.Restricted, ServerFeatures.P2P | ServerFeatures.Restricted, true)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.Restricted, ServerFeatures.SecureCore | ServerFeatures.Tor | ServerFeatures.Ipv6, false)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.Restricted, ServerFeatures.SecureCore | ServerFeatures.Tor | ServerFeatures.Restricted, true)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.Restricted, ServerFeatures.P2P | ServerFeatures.Tor | ServerFeatures.Restricted, true)]
    [DataRow(ServerFeatures.Restricted, ServerFeatures.B2B, true)]
    [DataRow(ServerFeatures.B2B, ServerFeatures.Restricted, true)]
    public void TestIsSupported(ServerFeatures serverFeatures, ServerFeatures expectedFeatures, bool expectedResult)
    {
        bool result = ServerFeatureExtensions.IsSupported(serverFeatures, expectedFeatures);
        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow(default, true)]
    [DataRow(ServerFeatures.SecureCore, false)]
    [DataRow(ServerFeatures.Tor, false)]
    [DataRow(ServerFeatures.P2P, true)]
    [DataRow(ServerFeatures.Streaming, true)]
    [DataRow(ServerFeatures.Ipv6, true)]
    [DataRow(ServerFeatures.Restricted, false)]
    [DataRow(ServerFeatures.Partner, false)]
    [DataRow(ServerFeatures.DoubleRestricted, false)]
    [DataRow(ServerFeatures.B2B, false)]
    [DataRow(ServerFeatures.NonStandard, false)]
    [DataRow(ServerFeatures.SecureCore | ServerFeatures.Tor, false)]
    [DataRow(ServerFeatures.Streaming | ServerFeatures.Ipv6, true)]
    [DataRow(ServerFeatures.Restricted | ServerFeatures.DoubleRestricted, false)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.DoubleRestricted, false)]
    [DataRow(ServerFeatures.Restricted | ServerFeatures.Partner, false)]
    [DataRow(ServerFeatures.Restricted | ServerFeatures.DoubleRestricted | ServerFeatures.B2B, false)]
    [DataRow(ServerFeatures.SecureCore | ServerFeatures.Tor | ServerFeatures.P2P | ServerFeatures.Streaming | ServerFeatures.Ipv6 | ServerFeatures.Restricted | ServerFeatures.Partner | ServerFeatures.DoubleRestricted, false)]
    public void TestIsStandard(ServerFeatures serverFeatures, bool expectedResult)
    {
        bool result = ServerFeatureExtensions.IsStandard(serverFeatures);
        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow(default, false)]
    [DataRow(ServerFeatures.SecureCore, false)]
    [DataRow(ServerFeatures.Tor, false)]
    [DataRow(ServerFeatures.P2P, false)]
    [DataRow(ServerFeatures.Streaming, false)]
    [DataRow(ServerFeatures.Ipv6, false)]
    [DataRow(ServerFeatures.Restricted, true)]
    [DataRow(ServerFeatures.Partner, false)]
    [DataRow(ServerFeatures.DoubleRestricted, true)]
    [DataRow(ServerFeatures.B2B, true)]
    [DataRow(ServerFeatures.NonStandard, true)]
    [DataRow(ServerFeatures.SecureCore | ServerFeatures.Tor, false)]
    [DataRow(ServerFeatures.Streaming | ServerFeatures.Ipv6, false)]
    [DataRow(ServerFeatures.Restricted | ServerFeatures.DoubleRestricted, true)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.DoubleRestricted, true)]
    [DataRow(ServerFeatures.Restricted | ServerFeatures.Partner, true)]
    [DataRow(ServerFeatures.Restricted | ServerFeatures.DoubleRestricted | ServerFeatures.B2B, true)]
    [DataRow(ServerFeatures.SecureCore | ServerFeatures.Tor | ServerFeatures.P2P | ServerFeatures.Streaming | ServerFeatures.Ipv6 | ServerFeatures.Restricted | ServerFeatures.Partner | ServerFeatures.DoubleRestricted, true)]
    public void TestIsB2B(ServerFeatures serverFeatures, bool expectedResult)
    {
        bool result = ServerFeatureExtensions.IsB2B(serverFeatures);
        Assert.AreEqual(expectedResult, result);
    }
}