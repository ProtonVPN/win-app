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
using ProtonVPN.Client.Logic.Servers.Contracts;

namespace ProtonVPN.Client.Logic.Servers.Tests;

[TestClass]
public class ServerFeatureExtensionsTest
{
    [TestMethod]
    [DataRow(ServerFeatures.Standard, ServerFeatures.Standard, true)]
    [DataRow(ServerFeatures.Standard, ServerFeatures.SecureCore, false)]
    [DataRow(ServerFeatures.Standard, ServerFeatures.SecureCore | ServerFeatures.Tor | ServerFeatures.B2B, false)]
    [DataRow(ServerFeatures.SecureCore, ServerFeatures.Standard, false)]
    [DataRow(ServerFeatures.SecureCore, ServerFeatures.P2P, false)]
    [DataRow(ServerFeatures.SecureCore, ServerFeatures.SecureCore, true)]
    [DataRow(ServerFeatures.SecureCore, ServerFeatures.P2P | ServerFeatures.Tor | ServerFeatures.B2B, false)]
    [DataRow(ServerFeatures.SecureCore, ServerFeatures.SecureCore | ServerFeatures.Tor | ServerFeatures.B2B, true)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.B2B, ServerFeatures.Standard, false)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.B2B, ServerFeatures.SecureCore, false)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.B2B, ServerFeatures.P2P, true)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.B2B, ServerFeatures.B2B, true)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.B2B, ServerFeatures.P2P | ServerFeatures.B2B, true)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.B2B, ServerFeatures.SecureCore | ServerFeatures.Tor | ServerFeatures.Ipv6, false)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.B2B, ServerFeatures.SecureCore | ServerFeatures.Tor | ServerFeatures.B2B, true)]
    [DataRow(ServerFeatures.P2P | ServerFeatures.B2B, ServerFeatures.P2P | ServerFeatures.Tor | ServerFeatures.B2B, true)]
    public void TestIsSupported(ServerFeatures serverFeatures, ServerFeatures expectedFeatures, bool expectedResult)
    {
        bool result = ServerFeatureExtensions.IsSupported(serverFeatures, expectedFeatures);
        Assert.AreEqual(expectedResult, result);
    }
}