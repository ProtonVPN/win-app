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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;

namespace ProtonVPN.Common.Tests.Vpn
{
    [TestClass]
    public class VpnConfigTest
    {
        [TestMethod]
        public void VpnConfig_ShouldThrow_WhenPortIsNotValid()
        {
            Dictionary<VpnProtocol, IReadOnlyCollection<int>> portConfig = GetPortConfig(new()
            {
                1,
                2,
                3,
                1080,
                80,
                1944,
                9999999
            });

            // Act
            Action action = () => new VpnConfig(new() {Ports = portConfig});

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void VpnConfig_ShouldThrow_WhenDnsIsNotValid()
        {
            Dictionary<VpnProtocol, IReadOnlyCollection<int>> portConfig = GetPortConfig(new() {1, 2, 3});

            List<string> customDns = new List<string> {"1.1.1.1", "8.8.8.8", "--invalid-ip",};

            // Act
            Action action = () =>
                new VpnConfig(new() {Ports = portConfig, CustomDns = customDns});

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        private Dictionary<VpnProtocol, IReadOnlyCollection<int>> GetPortConfig(List<int> ports)
        {
            return new() {{VpnProtocol.OpenVpnTcp, ports}};
        }
    }
}