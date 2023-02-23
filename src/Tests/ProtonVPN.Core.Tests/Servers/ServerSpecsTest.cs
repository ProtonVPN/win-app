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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Specs;

namespace ProtonVPN.Core.Tests.Servers
{
    [TestClass]
    public class EntryCountryServerTest
    {
        [TestMethod]
        public void ItShouldReturnTrueForEntryCountry()
        {
            const string country = "us";
            var server = new LogicalServerResponse {EntryCountry = country};
            var spec = new EntryCountryServer(country);

            spec.IsSatisfiedBy(server).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueForExitCountry()
        {
            const string country = "us";
            var server = new LogicalServerResponse { ExitCountry = country };
            var spec = new ExitCountryServer(country);

            spec.IsSatisfiedBy(server).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueForFreeServer()
        {
            new FreeServer().IsSatisfiedBy(new LogicalServerResponse { Tier = ServerTiers.Free }).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueForMaxTierServer()
        {
            var server = new LogicalServerResponse { Tier = 0};
            new MaxTierServer(1).IsSatisfiedBy(server).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueForOnlineServer()
        {
            var server = new LogicalServerResponse { Status = 1 };
            new OnlineServer().IsSatisfiedBy(server).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueForP2PServer()
        {
            var server = new LogicalServerResponse { Features = (sbyte) Features.P2P };
            new P2PServer().IsSatisfiedBy(server).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueForTorServer()
        {
            var server = new LogicalServerResponse { Features = (sbyte)Features.Tor };
            new TorServer().IsSatisfiedBy(server).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueForSecureCoreServer()
        {
            var server = new LogicalServerResponse { Features = (sbyte)Features.SecureCore };
            new SecureCoreServer().IsSatisfiedBy(server).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueForServerById()
        {
            const string id = "server id";
            var server = new LogicalServerResponse { Id = id };
            new ServerById(id).IsSatisfiedBy(server).Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow((sbyte)Features.None)]
        [DataRow((sbyte)Features.P2P)]
        [DataRow((sbyte)Features.Tor)]
        public void ItShouldReturnTrueForStandardServer(sbyte feature)
        {
            var server = new LogicalServerResponse { Features = feature };
            new StandardServer().IsSatisfiedBy(server).Should().BeTrue();
        }
    }
}