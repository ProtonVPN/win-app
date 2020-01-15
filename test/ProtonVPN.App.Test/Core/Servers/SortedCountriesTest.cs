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
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Servers;

namespace ProtonVPN.App.Test.Core.Servers
{
    [TestClass]
    public class SortedCountriesTest
    {
        [TestMethod]
        public void ItShouldReturnFreeServersAtTheTopForFreeUsers()
        {
            var allCountries = new List<string> {"a", "b", "c", "d"};
            var countriesWithFreeServers = new List<string> {"b", "c"};

            var userStorage = Substitute.For<IUserStorage>();
            userStorage.User().Returns(new User {MaxTier = ServerTiers.Free});

            var serverManager = Substitute.For<ServerManager>(userStorage);
            serverManager.GetCountries().Returns(allCountries);
            serverManager.GetCountriesWithFreeServers().Returns(countriesWithFreeServers);

            var countries = new SortedCountries(userStorage, serverManager);

            countries.List()[0].Should().Be("b");
            countries.List()[1].Should().Be("c");
        }

        [TestMethod]
        public void ItShouldReturnAvailableServersAtTheTop()
        {
            var userStorage = Substitute.For<IUserStorage>();
            var userTier = ServerTiers.Basic;
            userStorage.User().Returns(new User {MaxTier = userTier});

            var serverManager = new ServerManager(userStorage, GetServers());
            var servers = serverManager.GetServers(new ExitCountryServer("US"));

            for (var i = 0; i < servers.Where(s => s.Tier <= userTier).ToList().Count; i++)
            {
                servers.ElementAt(i).Tier.Should().BeLessOrEqualTo(userTier);
            }
        }

        private List<LogicalServerContract> GetServers()
        {
            return new List<LogicalServerContract>
            {
                GetServer("a", ServerTiers.Plus),
                GetServer("b", ServerTiers.Free),
                GetServer("c", ServerTiers.Basic),
                GetServer("d", ServerTiers.Plus),
                GetServer("e", ServerTiers.Free),
                GetServer("f", ServerTiers.Basic),
            };
        }

        private LogicalServerContract GetServer(string name, sbyte tier)
        {
            return new LogicalServerContract
            {
                ExitCountry = "US",
                Name = name,
                Tier = tier,
                Servers = new List<PhysicalServerContract>()
            };
        }
    }
}
