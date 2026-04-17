/*
 * Copyright (c) 2026 Proton AG
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

using NSubstitute;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Connection.Contracts.Preferences;
using ProtonVPN.Client.Logic.Connection.Contracts.ServerListGenerators;
using ProtonVPN.Client.Logic.Connection.ServerListGenerators;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Logic.Connection.Tests;

[TestClass]
public class ServerListGeneratorTests
{
    private static readonly IList<VpnProtocol> _preferredProtocols = [VpnProtocol.OpenVpnUdp];

    [TestMethod]
    public void Generate_SetsAreAllExcluded_WhenExclusionsRemoveAllServers()
    {
        // Arrange                           
        ISettings settings = CreateSettings();
        IExclusionChecker exclusionChecker = Substitute.For<IExclusionChecker>();
        exclusionChecker.HasExcludedLocations.Returns(true);
        exclusionChecker.IsServerExcluded(Arg.Any<Server>()).Returns(true);

        List<Server> servers = [CreateServer("s1", "US")];

        IServersLoader serversLoader = Substitute.For<IServersLoader>();
        serversLoader.GetServers().Returns(servers);

        ILogger logger = Substitute.For<ILogger>();
        ServerListGenerator generator = new(settings, serversLoader, exclusionChecker, logger);

        IConnectionIntent connectionIntent = new ConnectionIntent(MultiCountryLocationIntent.Default);

        // Act
        ServerListResult result = generator.Generate(connectionIntent, _preferredProtocols);

        // Assert
        Assert.AreEqual(0, result.PhysicalServers.Count);
        Assert.IsTrue(result.Diagnostic.AreAllCandidatesExcluded);
    }

    [TestMethod]
    public void Generate_SetsHadCandidates_WhenServersRemainAfterExclusions()
    {
        // Arrange
        ISettings settings = CreateSettings();
        IExclusionChecker exclusionChecker = Substitute.For<IExclusionChecker>();
        exclusionChecker.HasExcludedLocations.Returns(true);
        exclusionChecker.IsServerExcluded(Arg.Is<Server>(s => s.ExitCountry == "US")).Returns(true);

        List<Server> servers =
        [
            CreateServer("s1", "CH"),
            CreateServer("s2", "US"),
        ];

        IServersLoader serversLoader = Substitute.For<IServersLoader>();
        serversLoader.GetServers().Returns(servers);

        ILogger logger = Substitute.For<ILogger>();
        ServerListGenerator generator = new(settings, serversLoader, exclusionChecker, logger);

        IConnectionIntent connectionIntent = new ConnectionIntent(MultiCountryLocationIntent.Default);

        // Act
        ServerListResult result = generator.Generate(connectionIntent, _preferredProtocols);

        // Assert
        Assert.AreEqual(1, result.PhysicalServers.Count);
        Assert.IsFalse(result.Diagnostic.AreAllCandidatesExcluded);
    }

    [TestMethod]
    public void Generate_DoesNotFlagExclusions_WhenNoServersWereAvailable()
    {
        // Arrange
        ISettings settings = CreateSettings();
        IExclusionChecker exclusionChecker = Substitute.For<IExclusionChecker>();
        exclusionChecker.HasExcludedLocations.Returns(true);
        exclusionChecker.IsServerExcluded(Arg.Any<Server>()).Returns(true);

        IServersLoader serversLoader = Substitute.For<IServersLoader>();
        serversLoader.GetServers().Returns([]);

        ILogger logger = Substitute.For<ILogger>();
        ServerListGenerator generator = new(settings, serversLoader, exclusionChecker, logger);

        IConnectionIntent connectionIntent = new ConnectionIntent(MultiCountryLocationIntent.Default);

        // Act
        ServerListResult result = generator.Generate(connectionIntent, _preferredProtocols);

        // Assert
        Assert.AreEqual(0, result.PhysicalServers.Count);
        Assert.IsFalse(result.Diagnostic.AreAllCandidatesExcluded);
    }

    [TestMethod]
    public void Generate_DoesNotApplyExclusions_WhenSingleLocationIntent()
    {
        // Arrange
        ISettings settings = CreateSettings();
        IExclusionChecker exclusionChecker = Substitute.For<IExclusionChecker>();
        exclusionChecker.HasExcludedLocations.Returns(true);
        exclusionChecker.IsServerExcluded(Arg.Any<Server>()).Returns(true);

        List<Server> servers = [CreateServer("s1", "US")];

        IServersLoader serversLoader = Substitute.For<IServersLoader>();
        serversLoader.GetServers().Returns(servers);

        ILogger logger = Substitute.For<ILogger>();
        ServerListGenerator generator = new(settings, serversLoader, exclusionChecker, logger);

        IConnectionIntent connectionIntent = new ConnectionIntent(new SingleCountryLocationIntent("US"));

        // Act
        ServerListResult result = generator.Generate(connectionIntent, _preferredProtocols);

        // Assert
        Assert.AreEqual(1, result.PhysicalServers.Count);
        Assert.IsFalse(result.Diagnostic.AreAllCandidatesExcluded);
    }

    private static ISettings CreateSettings()
    {
        ISettings settings = Substitute.For<ISettings>();
        settings.DeviceLocation.Returns((DeviceLocation?)null);
        settings.IsPortForwardingEnabled.Returns(false);
        settings.VpnPlan.Returns(new VpnPlan("VPN Plus", "vpnplus", 1, false));
        return settings;
    }

    private static Server CreateServer(string id, string exitCountry)
    {
        return new Server
        {
            Id = id,
            Name = id,
            City = "City",
            State = "State",
            EntryCountry = exitCountry,
            ExitCountry = exitCountry,
            HostCountry = exitCountry,
            Domain = $"{id}.example.com",
            Latitude = 0,
            Longitude = 0,
            Status = 1,
            Tier = ServerTiers.Plus,
            Features = 0,
            Load = 0,
            Score = 1,
            StatusReference = new()
            {
                Index = 0,
                Cost = 0,
                Penalty = 0,
            },
            EntryLocation = new()
            {
                Latitude = 0,
                Longitude = 0,
            },
            ExitLocation = new()
            {
                Latitude = 0,
                Longitude = 0,
            },
            Servers =
            [
                new PhysicalServer
                {
                    Id = $"{id}-p1",
                    EntryIp = "10.0.0.1",
                    ExitIp = "10.0.0.1",
                    Domain = $"{id}.example.com",
                    Label = $"{id}-p1",
                    Status = 1,
                    X25519PublicKey = "key",
                    Signature = "signature",
                    IsIpv6Supported = false,
                }
            ],
            IsVirtual = false,
            GatewayName = id,
        };
    }
}
