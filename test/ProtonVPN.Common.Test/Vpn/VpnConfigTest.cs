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
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Vpn;

namespace ProtonVPN.Common.Test.Vpn
{
    [TestClass]
    public class VpnConfigTest
    {
        [TestMethod]
        public void VpnConfig_ShouldThrow_WhenPortIsNotValid()
        {
            var portConfig = GetPortConfig(new List<int>
            {
                1,
                2,
                3,
                1080,
                80,
                1944,
                9999999
            });

            var customDns = new List<string>
            {
                "1.1.1.1",
                "8.8.8.8",
            };

            // Act
            Action action = () => new VpnConfig(portConfig, customDns, false);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void VpnConfig_ShouldThrow_WhenDnsIsNotValid()
        {
            var portConfig = GetPortConfig(new List<int>
            {
                1,
                2,
                3,
            });

            var customDns = new List<string>
            {
                "1.1.1.1",
                "8.8.8.8",
                "--invalid-ip",
            };

            // Act
            Action action = () => new VpnConfig(portConfig, customDns, false);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        private Dictionary<VpnProtocol, IReadOnlyCollection<int>> GetPortConfig(List<int> ports)
        {
            return new Dictionary<VpnProtocol, IReadOnlyCollection<int>>
            {
                {
                    VpnProtocol.OpenVpnTcp, ports
                }
            };
        }
    }
}
