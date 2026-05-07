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

using System.Net.NetworkInformation;
using FluentAssertions;
using NSubstitute;
using ProtonVPN.Client.Logic.Feedback.Diagnostics.Logs;
using ProtonVPN.OperatingSystems.Network.Contracts.NetworkInterfaces;

namespace ProtonVPN.Client.Logic.Feedback.Tests.Diagnostics;

[TestClass]
public class NetworkAdapterLogTest : LogBaseTest
{
    private INetworkInterfacesProvider? _networkInterfacesProvider;

    [TestInitialize]
    public override void Initialize()
    {
        base.Initialize();

        _networkInterfacesProvider = Substitute.For<INetworkInterfacesProvider>();
    }

    [TestCleanup]
    public override void Cleanup()
    {
        base.Cleanup();

        _networkInterfacesProvider = null;
    }

    [TestMethod]
    public void ItShouldCreateLogFile()
    {
        // Arrange
        NetworkInterfaceInfo[] interfaces = new[]
        {
            CreateInterface("interface1"),
            CreateInterface("interface2"),
        };
        _networkInterfacesProvider!.Get().Returns(interfaces);

        NetworkAdapterLog log = new(_networkInterfacesProvider, StaticConfig!);

        // Act
        log.Write();

        // Assert
        string path = Path.Combine(TMP_PATH, "NetworkAdapters.txt");
        File.Exists(path).Should().BeTrue();

        string content = File.ReadAllText(path);
        content.Should().Contain("interface1");
        content.Should().Contain("interface2");
    }

    private static NetworkInterfaceInfo CreateInterface(string name)
    {
        return new()
        {
            Guid = $"Guid of {name}",
            Name = name,
            Description = $"Description of {name}",
            Type = NetworkInterfaceType.Wireless80211,
            OperationalStatus = OperationalStatus.Up
        };
    }
}