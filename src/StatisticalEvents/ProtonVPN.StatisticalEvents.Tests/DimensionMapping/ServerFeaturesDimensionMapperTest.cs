/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.StatisticalEvents.Contracts.Models;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers;

namespace ProtonVPN.StatisticalEvents.Tests.DimensionMapping;

[TestClass]
public class ServerFeaturesDimensionMapperTest
{
    private ServerFeaturesDimensionMapper? _mapper;

    [TestInitialize]
    public void Setup()
    {
        _mapper = new ServerFeaturesDimensionMapper();
    }

    [TestMethod]
    public void Map_ShouldReturnCorrectString_ForAllFeatures()
    {
        // Arrange
        ServerDetailsEventData serverDetails = new()
        {
            IsFree = true,
            SupportsTor = true,
            SupportsP2P = true,
            SecureCore = true,
            IsB2B = true,
            SupportsStreaming = true,
            SupportsIpv6 = true
        };

        string expected = "free,ipv6,p2p,partnership,secureCore,streaming,tor"; // Sorted alphabetically

        // Act
        string result = _mapper!.Map(serverDetails);

        // Assert
        Assert.AreEqual(expected, result, "Mapping did not return the expected string for all features.");
    }

    [TestMethod]
    public void Map_ShouldReturnEmptyString_ForNoFeatures()
    {
        // Arrange
        ServerDetailsEventData serverDetails = new()
        {
            IsFree = false,
            SupportsTor = false,
            SupportsP2P = false,
            SecureCore = false,
            IsB2B = false,
            SupportsStreaming = false,
            SupportsIpv6 = false
        };

        // Act
        string result = _mapper!.Map(serverDetails);

        // Assert
        Assert.AreEqual(string.Empty, result, "Mapping did not return an empty string for no features.");
    }

    [TestMethod]
    public void Map_ShouldHandleSubsetOfFeatures()
    {
        // Arrange
        ServerDetailsEventData serverDetails = new()
        {
            IsFree = true,
            SupportsTor = false,
            SupportsP2P = false,
            SecureCore = true,
            IsB2B = false,
            SupportsStreaming = true,
            SupportsIpv6 = false
        };

        string expected = "free,secureCore,streaming"; // Sorted alphabetically

        // Act
        string result = _mapper!.Map(serverDetails);

        // Assert
        Assert.AreEqual(expected, result, "Mapping did not return the expected string for a subset of features.");
    }
}