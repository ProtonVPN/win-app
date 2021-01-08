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
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Settings;
using ProtonVPN.Servers;

namespace ProtonVPN.App.Test.Servers
{
    [TestClass]
    public class ServerListFactoryTest
    {
        private IUserStorage _userStorage;
        private ServerManager _serverManager;
        private ISortedCountries _sortedCountries;
        private ServerListFactory _serverListFactory;

        private User _user;
        private List<string> _countries;

        [TestInitialize]
        public void TestInitialize()
        {
            InitializeUserStorage();

            List<LogicalServerContract> servers = new List<LogicalServerContract>
            {
                CreateServer("IT#1", Features.None, "IT"),
                CreateServer("FR#1", Features.None, "FR"),
                CreateServer("CH#1", Features.None, "CH"),
                CreateServer("SE#1", Features.None, "SE"),
                CreateServer("IS#1", Features.None, "IS"),
                CreateServer("US-TX#1", Features.None, "US"),
                CreateServer("IT#2", Features.SecureCore, "IT", "SE"),
                CreateServer("FR#2", Features.SecureCore, "FR", "IS"),
                CreateServer("CH#2", Features.P2P, "CH"),
                CreateServer("CH#3", Features.Tor, "CH"),
                CreateServer("IS#2", Features.Tor, "IS"),
                CreateServer("US-TX#2", Features.P2P, "US")
            };

            _serverManager = Substitute.For<ServerManager>(_userStorage, servers);
            InitializeSortedCountries();
            _serverListFactory = new ServerListFactory(_serverManager, _userStorage, _sortedCountries);
        }

        private LogicalServerContract CreateServer(string name, Features features, string exitCountryCode, string entryCountryCode = null)
        {
            return new LogicalServerContract() 
            { 
                Name = name, 
                EntryCountry = entryCountryCode ?? exitCountryCode, 
                ExitCountry = exitCountryCode, 
                Features = (sbyte)features,
                Servers = new List<PhysicalServerContract>()
            };
        }

        private void InitializeUserStorage()
        {
            _userStorage = Substitute.For<IUserStorage>();
            _user = new User();
            _userStorage.User().Returns(_user);
        }

        private void InitializeSortedCountries()
        {
            _sortedCountries = Substitute.For<ISortedCountries>();
            _countries = new List<string> { "IT", "CH", "SE", "IS", "US", "FR" };
            _sortedCountries.List().Returns(_countries);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _userStorage = null;
            _serverManager = null;
            _sortedCountries = null;
            _serverListFactory = null;

            _user = null;
            _countries = null;
        }

        [TestMethod]
        public void BuildServerList()
        {
            // Act
            ObservableCollection<IServerListItem> result = _serverListFactory.BuildServerList();

            // Assert
            IList<string> countries = _countries.ToList();
            Assert.AreEqual(countries.Count, result.Count);
            foreach (IServerListItem serverListItem in result)
            {
                Assert.IsInstanceOfType(serverListItem, typeof(ServersByCountryViewModel));
                ServersByCountryViewModel viewModel = (ServersByCountryViewModel)serverListItem;
                Assert.IsTrue(countries.Contains(viewModel.CountryCode));
                countries.Remove(viewModel.CountryCode);
            }
        }

        [TestMethod]
        public void BuildServerList_WithCountryNameInSearchQuery()
        {
            // Arrange
            string searchQuery = "Switzerland";

            // Act
            ObservableCollection<IServerListItem> result = _serverListFactory.BuildServerList(searchQuery);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsInstanceOfType(result[0], typeof(ServersByCountryViewModel));
            ServersByCountryViewModel viewModel = (ServersByCountryViewModel)result[0];
            Assert.AreEqual("CH", viewModel.CountryCode);
        }

        [TestMethod]
        public void BuildServerList_WithCountryCodeInSearchQuery()
        {
            // Arrange
            string searchQuery = "TX";

            // Act
            ObservableCollection<IServerListItem> result = _serverListFactory.BuildServerList(searchQuery);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsInstanceOfType(result[0], typeof(ServersByCountryViewModel));
            ServersByCountryViewModel viewModel = (ServersByCountryViewModel)result[0];
            Assert.AreEqual("US", viewModel.CountryCode);
            foreach (IServerListItem server in viewModel.Servers)
            {
                Assert.IsTrue(server.Name.StartsWith("US-TX#"));
            }
        }

        [TestMethod]
        public void BuildSecureCoreList()
        {
            // Act
            ObservableCollection<IServerListItem> result = _serverListFactory.BuildSecureCoreList();

            // Assert
            IList<string> secureCoreCountries = new List<string> { "FR", "IT" };
            Assert.AreEqual(secureCoreCountries.Count, result.Count);
            foreach (IServerListItem serverListItem in result)
            {
                Assert.IsInstanceOfType(serverListItem, typeof(ServersByExitNodeViewModel));
                ServersByExitNodeViewModel viewModel = (ServersByExitNodeViewModel)serverListItem;
                Assert.IsTrue(secureCoreCountries.Contains(viewModel.CountryCode));
                secureCoreCountries.Remove(viewModel.CountryCode);
            }
        }

        [TestMethod]
        public void BuildPortForwardingList()
        {
            // Act
            ObservableCollection<IServerListItem> result = _serverListFactory.BuildPortForwardingList();

            // Assert
            IList<string> p2pCountries = new List<string> { "CH", "US" };
            IList<string> nonP2Pcountries = new List<string> { "IT", "SE", "IS", "FR" };
            int numOfSeparators = 2;
            Assert.AreEqual(p2pCountries.Count + nonP2Pcountries.Count + numOfSeparators, result.Count);

            AssertCountrySeparator(result[0], "P2P");
            AssertCountries(result.Skip(1).Take(p2pCountries.Count), p2pCountries);
            AssertCountrySeparator(result[p2pCountries.Count+1], "OTHERS");
            AssertCountries(result.Skip(p2pCountries.Count + 2).Take(nonP2Pcountries.Count), nonP2Pcountries);
        }

        private void AssertCountrySeparator(IServerListItem serverListItem, string expectedName)
        {
            Assert.IsInstanceOfType(serverListItem, typeof(CountrySeparatorViewModel));
            CountrySeparatorViewModel viewModel = (CountrySeparatorViewModel)serverListItem;
            Assert.AreEqual(expectedName, viewModel.Name);
        }

        private void AssertCountries(IEnumerable<IServerListItem> serverListItems, IEnumerable<string> expectedCountriesEnum)
        {
            IList<string> expectedCountries = expectedCountriesEnum.ToList();
            foreach (IServerListItem serverListItem in serverListItems)
            {
                Assert.IsInstanceOfType(serverListItem, typeof(ServersByCountryViewModel));
                ServersByCountryViewModel viewModel = (ServersByCountryViewModel)serverListItem;
                Assert.IsTrue(expectedCountries.Contains(viewModel.CountryCode));
                expectedCountries.Remove(viewModel.CountryCode);
            }
        }
    }
}
