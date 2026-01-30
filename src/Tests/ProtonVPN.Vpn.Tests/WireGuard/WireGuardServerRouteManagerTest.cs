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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Files.Contracts;
using ProtonVPN.OperatingSystems.Network.Contracts.Routing;
using ProtonVPN.Serialization.Contracts;
using ProtonVPN.Vpn.WireGuard;

namespace ProtonVPN.Vpn.Tests.WireGuard;

[TestClass]
public class WireGuardServerRouteManagerTest
{
    private const string ROUTES_FILE_PATH = @"C:\temp\WireGuardServerRoutes.json";

    private IStaticConfiguration _config;
    private IRoutingTableHelper _routingTableHelper;
    private IIpv4GatewayResolver _ipv4GatewayResolver;
    private IFileReaderWriter _fileReaderWriter;

    [TestInitialize]
    public void TestInitialize()
    {
        _config = Substitute.For<IStaticConfiguration>();
        _routingTableHelper = Substitute.For<IRoutingTableHelper>();
        _ipv4GatewayResolver = Substitute.For<IIpv4GatewayResolver>();
        _fileReaderWriter = Substitute.For<IFileReaderWriter>();

        _config.WireGuardServerRoutesFilePath.Returns(ROUTES_FILE_PATH);
    }

    [TestMethod]
    public void CleanupPersistedRoutes_ShouldWriteDistinctRoutes()
    {
        // Arrange
        List<PersistedServerRoute> persistedRoutes = [
            new PersistedServerRoute { DestinationIpAddress = "1.1.1.1", IsIpv6 = false },
            new PersistedServerRoute { DestinationIpAddress = "1.1.1.1", IsIpv6 = false },
            new PersistedServerRoute { DestinationIpAddress = "FE80::1", IsIpv6 = true },
            new PersistedServerRoute { DestinationIpAddress = "fe80::1", IsIpv6 = true },
        ];

        _fileReaderWriter
            .ReadOrNew<List<PersistedServerRoute>>(ROUTES_FILE_PATH, Serializers.Json)
            .Returns(persistedRoutes);

        _routingTableHelper.DeleteRoute(Arg.Any<string>(), Arg.Any<bool>()).Returns(false);

        IEnumerable<PersistedServerRoute> captured = null;
        _fileReaderWriter
            .Write(Arg.Do<IEnumerable<PersistedServerRoute>>(routes => captured = routes), ROUTES_FILE_PATH, Serializers.Json)
            .Returns(FileOperationResult.Success);

        WireGuardServerRouteManager sut = new(
            _config,
            _routingTableHelper,
            _ipv4GatewayResolver,
            _fileReaderWriter);

        // Act
        sut.CleanupPersistedRoutes();

        // Assert
        captured.Should().NotBeNull();
        List<PersistedServerRoute> result = captured.ToList();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(
            [
                new PersistedServerRoute { DestinationIpAddress = "1.1.1.1", IsIpv6 = false },
                new PersistedServerRoute { DestinationIpAddress = "FE80::1", IsIpv6 = true },
            ],
            options => options.WithoutStrictOrdering());
    }
}