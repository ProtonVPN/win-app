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

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Servers;
using PhysicalServer = ProtonVPN.Core.Servers.Models.PhysicalServer;
using Server = ProtonVPN.Core.Servers.Models.Server;

namespace ProtonVPN.Core.Test.Servers
{
    [TestClass]
    public class ServerTest
    {
        [DataTestMethod]
        [DataRow(ServerTiers.Free, true)]
        [DataRow(ServerTiers.Basic, false)]
        [DataRow(ServerTiers.Plus, false)]
        public void IsFree_ShouldBeTrue_WhenFreeTier(int tier, bool expected)
        {
            // Arrange
            var server = GetServer(tier);
            // Act
            var result = server.IsFree();
            // Assert
            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(ServerTiers.Free, true)]
        [DataRow(ServerTiers.Basic, false)]
        [DataRow(ServerTiers.Plus, false)]
        public void IsPhysicalFree_ShouldBeTrue_WhenFreeTier(int tier, bool expected)
        {
            // Arrange
            var server = GetServer(tier);
            // Act
            var result = server.IsPhysicalFree();
            // Assert
            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow("", false)]
        [DataRow("Server", false)]
        [DataRow("Server #", false)]
        [DataRow("Server #1", false)]
        [DataRow("Server #99", false)]
        [DataRow("Server #100", true)]
        [DataRow("Server #101", true)]
        [DataRow("Server #333", true)]
        public void IsPhysicalFree_ShouldBeTrue_WhenNameContains_NumberGreaterOrEqualTo_100(string name, bool expected)
        {
            // Arrange
            var server = GetServer(ServerTiers.Plus, name);
            // Act
            var result = server.IsPhysicalFree();
            // Assert
            result.Should().Be(expected);
        }

        private Server GetServer(int tier, string name = "")
        {
            return new Server(
                "",
                name,
                "",
                "ZZ",
                "ZZ",
                "",
                0,
                tier,
                0,
                0,
                0.0F,
                new Location { Lat = 0f, Long = 0f },
                new List<PhysicalServer>(0),
                null);
        }
    }
}
