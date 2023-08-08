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
            LogicalServerResponse server = new() { EntryCountry = country };
            EntryCountryServer spec = new(country);

            spec.IsSatisfiedBy(server).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueForExitCountry()
        {
            const string country = "us";
            LogicalServerResponse server = new() { ExitCountry = country };
            ExitCountryServer spec = new(country);

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
            LogicalServerResponse server = new() { Tier = 0 };
            new MaxTierServer(1).IsSatisfiedBy(server).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueForOnlineServer()
        {
            LogicalServerResponse server = new() { Status = 1 };
            new OnlineServer().IsSatisfiedBy(server).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueForP2PServer()
        {
            LogicalServerResponse server = new() { Features = (ulong) Features.P2P };
            new P2PServer().IsSatisfiedBy(server).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueForTorServer()
        {
            LogicalServerResponse server = new() { Features = (ulong)Features.Tor };
            new TorServer().IsSatisfiedBy(server).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueForSecureCoreServer()
        {
            LogicalServerResponse server = new() { Features = (ulong)Features.SecureCore };
            new SecureCoreServer().IsSatisfiedBy(server).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueForServerById()
        {
            const string id = "server id";
            LogicalServerResponse server = new() { Id = id };
            new ServerById(id).IsSatisfiedBy(server).Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow((ulong)Features.None)]
        [DataRow((ulong)Features.P2P)]
        [DataRow((ulong)Features.Tor)]
        public void ItShouldReturnTrueForStandardServer(ulong feature)
        {
            LogicalServerResponse server = new() { Features = feature };
            new StandardServer().IsSatisfiedBy(server).Should().BeTrue();
        }
    }
}