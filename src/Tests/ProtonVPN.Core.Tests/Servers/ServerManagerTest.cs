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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Tests.Servers
{
    [TestClass]
    public class ServerManagerTest
    {
        private IUserStorage _userStorage;
        private IAppSettings _appSettings;
        private ILogger _logger;

        [TestInitialize]
        public void Initialize()
        {
            _userStorage = Substitute.For<IUserStorage>();
            _appSettings = Substitute.For<IAppSettings>();
            _logger = Substitute.For<ILogger>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _userStorage = null;
            _appSettings = null;
            _logger = null;
        }

        [TestMethod]
        public void ItShouldSkipServersWithoutPublicKeyForWireguard()
        {
            // Arrange
            _appSettings.GetProtocol().Returns(VpnProtocol.WireGuard);
            _userStorage.GetUser().Returns(new User {MaxTier = 0});
            ServerManager serverManager = new ServerManager(_userStorage, _appSettings, _logger);

            // Act
            serverManager.Load(GetServerList());

            // Assert
            serverManager.GetServers(new AnyServer()).Count.Should().Be(1);
        }

        [TestMethod]
        public void ItShouldSkipCountriesWithNoServersForWireguard()
        {
            // Arrange
            _appSettings.GetProtocol().Returns(VpnProtocol.WireGuard);
            ServerManager serverManager = new ServerManager(_userStorage, _appSettings, _logger);

            // Act
            serverManager.Load(GetServerList());

            // Assert
            serverManager.GetCountries().Should().NotContain("US");
        }

        private List<LogicalServerResponse> GetServerList()
        {
            return new()
            {
                new()
                {
                    Name = "US#1",
                    EntryCountry = "US",
                    Servers = new List<PhysicalServerResponse>
                    {
                        new() {X25519PublicKey = string.Empty},
                        new() {X25519PublicKey = string.Empty},
                    }
                },
                new()
                {
                    Name = "CH#1",
                    EntryCountry = "CH",
                    Servers = new List<PhysicalServerResponse>
                    {
                        new() {X25519PublicKey = "key"},
                        new() {X25519PublicKey = "key"},
                    }
                }
            };
        }

        public class AnyServer : Specification<LogicalServerResponse>
        {
            public override bool IsSatisfiedBy(LogicalServerResponse item)
            {
                return true;
            }
        }
    }
}