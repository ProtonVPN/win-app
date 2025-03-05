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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Servers.Mappers.Tests;

[TestClass]
public class LogicalServerMapperTest
{
    private IEntityMapper _entityMapper;
    private LogicalServerMapper _mapper;

    [TestInitialize]
    public void Initialize()
    {
        _entityMapper = Substitute.For<IEntityMapper>();
        _entityMapper.Map<PhysicalServerResponse, PhysicalServer>(Arg.Any<IEnumerable<PhysicalServerResponse>>())
            .Returns(new List<PhysicalServer>());
        _mapper = new(_entityMapper);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _entityMapper = null;
        _mapper = null;
    }

    [TestMethod]
    [DataRow(1u, ServerFeatures.SecureCore)]
    [DataRow(4u, ServerFeatures.P2P)]
    [DataRow(8u, ServerFeatures.Streaming)]
    [DataRow(16u, ServerFeatures.Ipv6)]
    [DataRow(32u, ServerFeatures.Restricted)]
    [DataRow(64u, ServerFeatures.Partner)]
    [DataRow(128u, ServerFeatures.DoubleRestricted)]
    [DataRow(0u | 16u, ServerFeatures.Ipv6)]
    [DataRow(4u | 8u, ServerFeatures.P2P)]
    public void TestGetLogicalServer(ulong features, ServerFeatures expectedFeatures)
    {
        bool isFeatureSupported = _mapper
            .Map(GetLogicalServerResponse(features))
            .Features
            .IsSupported(expectedFeatures);

        Assert.IsTrue(isFeatureSupported);
    }

    private LogicalServerResponse GetLogicalServerResponse(ulong features)
    {
        return new LogicalServerResponse
        {
            Id = "Logical server ID",
            Name = "CH#1",
            City = "Geneva",
            Domain = "ch.protonvpn.com",
            EntryCountry = "ch",
            ExitCountry = "ch",
            Features = features,
            HostCountry = string.Empty,
            Load = 0,
            Location = new LocationResponse { Lat = 50, Long = 20, },
            Servers = new()
        };
    }
}