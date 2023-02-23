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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Common.Logging;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Settings;
using ProtonVPN.Partners;
using ProtonVPN.Servers;
using ProtonVPN.Streaming;

namespace ProtonVPN.App.Tests.Servers
{
    [TestClass]
    public class ServerListFactoryTest
    {
        private IStreamingServices _streamingServices;
        private IPartnersService _partnersService;
        private IUserStorage _userStorage;
        private IAppSettings _appSettings;
        private ILogger _logger;
        private ServerManager _serverManager;
        private ServerListFactory _serverListFactory;
        private IActiveUrls _urls;

        private User _user;
        private List<string> _countries;

        private readonly List<string> _countriesWithFreeServers = new() {"JP", "NL", "US"};

        [TestInitialize]
        public void TestInitialize()
        {
            InitializeUserStorage();

            List<LogicalServerResponse> servers = new List<LogicalServerResponse>
            {
                CreateServer("IT#1", Features.None, "IT", null, 1),
                CreateServer("FR#1", Features.None, "FR", null, 1),
                CreateServer("CH#1", Features.None, "CH", null, 1),
                CreateServer("SE#1", Features.None, "SE", null, 1),
                CreateServer("IS#1", Features.None, "IS", null, 1),
                CreateServer("US-TX#1", Features.None, "US", null, 1),
                CreateServer("IT#2", Features.SecureCore, "IT", "SE", 2),
                CreateServer("FR#2", Features.SecureCore, "FR", "IS", 2),
                CreateServer("CH#2", Features.P2P, "CH", null, 2),
                CreateServer("CH#3", Features.Tor, "CH", null, 2),
                CreateServer("IS#2", Features.Tor, "IS", null, 2),
                CreateServer("US-TX#2", Features.P2P, "US", null, 2, "Houston")
            };

            foreach (string countryCode in _countriesWithFreeServers)
            {
                servers.Add(CreateServer(countryCode + "-FREE#1", Features.None, countryCode, countryCode));
            }

            _appSettings = Substitute.For<IAppSettings>();
            _logger = Substitute.For<ILogger>();
            _serverManager = Substitute.For<ServerManager>(_userStorage, _appSettings, _logger);
            _serverManager.Load(servers);
            _urls = Substitute.For<IActiveUrls>();
            _partnersService = Substitute.For<IPartnersService>();
            _partnersService.GetPartnerTypes().Returns(new List<PartnerType>());

            InitializeSortedCountries();
            _serverListFactory = new ServerListFactory(_serverManager, _userStorage, _streamingServices, _partnersService, _urls);
        }

        private void InitializeUserStorage()
        {
            _userStorage = Substitute.For<IUserStorage>();
            _user = new User();
            _userStorage.GetUser().Returns(_user);
        }

        private LogicalServerResponse CreateServer(string name, Features features, string exitCountryCode,
            string entryCountryCode = null, sbyte tier = 0, string city = "")
        {
            return new LogicalServerResponse
            {
                Name = name,
                EntryCountry = entryCountryCode ?? exitCountryCode,
                ExitCountry = exitCountryCode,
                Features = (sbyte)features,
                Tier = tier,
                City = city,
                Servers = new List<PhysicalServerResponse>()
            };
        }

        private void InitializeSortedCountries()
        {
            _countries = new List<string>
            {
                "IT",
                "CH",
                "SE",
                "IS",
                "US",
                "FR"
            }.OrderBy(Countries.GetName).ToList();

            _serverManager.GetCountries().Returns(_countries);
            _streamingServices = Substitute.For<IStreamingServices>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _streamingServices = null;
            _partnersService = null;
            _userStorage = null;
            _appSettings = null;
            _logger = null;
            _serverManager = null;
            _serverListFactory = null;
            _urls = null;

            _user = null;
            _countries = null;
        }

        [TestMethod]
        public void BuildServerList_ItShouldDisplayFreeCountriesForFreeUserFirst()
        {
            // Arrange
            _userStorage.GetUser().Returns(new User {MaxTier = ServerTiers.Free});

            // Act
            ObservableCollection<IServerListItem> result = _serverListFactory.BuildServerList();

            Assert.AreEqual(result[0].Name, "FREE Locations (" + _countriesWithFreeServers.Count + ")");

            // Assert
            for (int i = 0; i < _countriesWithFreeServers.Count; i++)
            {
                Assert.AreEqual(Countries.GetName(_countriesWithFreeServers[i]), result[i + 1].Name);
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
            Assert.AreEqual(2, result.Count);
            Assert.IsInstanceOfType(result[1], typeof(ServersByCountryViewModel));
            ServersByCountryViewModel viewModel = (ServersByCountryViewModel)result[1];
            Assert.AreEqual("CH", viewModel.CountryCode);
        }

        [TestMethod]
        public void BuildServerList_WithCityNameInSearchQuery()
        {
            // Arrange
            string searchQuery = "hou";

            // Act
            ObservableCollection<IServerListItem> result = _serverListFactory.BuildServerList(searchQuery);

            // Assert
            Assert.AreEqual(2, result.Count);
            ServersByCountryViewModel countryViewModel = (ServersByCountryViewModel)result[1];
            Assert.AreEqual(2, countryViewModel.Servers.Count);
            ServerItemViewModel serverItemViewModel = (ServerItemViewModel) countryViewModel.Servers[1];
            Assert.AreEqual("Houston", serverItemViewModel.Server.City);
        }

        [TestMethod]
        public void BuildServerList_WithCountryCodeInSearchQuery()
        {
            // Arrange
            string searchQuery = "TX";

            // Act
            ObservableCollection<IServerListItem> result = _serverListFactory.BuildServerList(searchQuery);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsInstanceOfType(result[1], typeof(ServersByCountryViewModel));
            ServersByCountryViewModel viewModel = (ServersByCountryViewModel)result[1];
            Assert.AreEqual("US", viewModel.CountryCode);
            foreach (IServerListItem server in viewModel.Servers)
            {
                if (server is ServerItemViewModel)
                {
                    Assert.IsTrue(server.Name.StartsWith("US-TX#"));
                }
            }
        }

        [TestMethod]
        public void BuildSecureCoreList()
        {
            // Act
            ObservableCollection<IServerListItem> result = _serverListFactory.BuildSecureCoreList();

            // Assert
            IList<string> secureCoreCountries = new List<string> {"FR", "IT"};
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
            IList<string> p2pCountries = new List<string> {"CH", "US"};
            IList<string> nonP2Pcountries = new List<string> {"IT", "SE", "IS", "FR"};
            int numOfSeparators = 2;
            Assert.AreEqual(p2pCountries.Count + nonP2Pcountries.Count + numOfSeparators, result.Count);

            AssertCountrySeparator(result[0], "P2P");
            AssertCountries(result.Skip(1).Take(p2pCountries.Count), p2pCountries);
            AssertCountrySeparator(result[p2pCountries.Count + 1], "OTHERS");
            AssertCountries(result.Skip(p2pCountries.Count + 2).Take(nonP2Pcountries.Count), nonP2Pcountries);
        }

        private void AssertCountrySeparator(IServerListItem serverListItem, string expectedName)
        {
            Assert.IsInstanceOfType(serverListItem, typeof(CountrySeparatorViewModel));
            CountrySeparatorViewModel viewModel = (CountrySeparatorViewModel)serverListItem;
            Assert.AreEqual(expectedName, viewModel.Name);
        }

        private void AssertCountries(IEnumerable<IServerListItem> serverListItems,
            IEnumerable<string> expectedCountriesEnum)
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