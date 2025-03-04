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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Update.Releases;
using ProtonVPN.Update.Responses;

namespace ProtonVPN.Update.Tests.Releases;

[TestClass]
public class ReleasesTest
{
    private ILogger _logger;

    [TestInitialize]
    public void TestInitialize()
    {
        _logger = Substitute.For<ILogger>();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _logger = null;
    }

    [TestMethod]
    public void Releases_ShouldImplement_IEnumerable()
    {
        Update.Releases.Releases releases = new(_logger, [], new Version(), "");

        releases.Should().BeAssignableTo<IEnumerable<Release>>();
    }

    [TestMethod]
    public void Releases_Version_ShouldBe_FromReleases()
    {
        string json = File.ReadAllText(@"TestData\windows-releases.json");
        ReleasesResponse releasesResponse = JsonConvert.DeserializeObject<ReleasesResponse>(json);
        List<Version> expected = releasesResponse.Releases.Select(r => Version.Parse(r.Version)).ToList();

        Update.Releases.Releases releases = new(_logger, releasesResponse.Releases, new Version(), "");

        IEnumerable<Version> result = releases.Select(r => r.Version);

        result.Should()
            .OnlyHaveUniqueItems().And
            .Contain(expected);
    }

    [TestMethod]
    public void Releases_EarlyAccess_ShouldBeTrue_ForEarlyAccess()
    {
        string json = File.ReadAllText(@"TestData\windows-releases.json");
        ReleasesResponse releasesResponse = JsonConvert.DeserializeObject<ReleasesResponse>(json);
        Update.Releases.Releases releases = new(_logger, releasesResponse.Releases, new Version(), "EarlyAccess");

        releases.Where(r => r.IsEarlyAccess).Should().HaveCount(2);
    }

    [TestMethod]
    public void Releases_New_ShouldBeTrue_ForNewReleases()
    {
        string json = File.ReadAllText(@"TestData\windows-releases.json");
        ReleasesResponse releasesResponse = JsonConvert.DeserializeObject<ReleasesResponse>(json);
        Update.Releases.Releases releases = new(_logger, releasesResponse.Releases, Version.Parse("1.5.0"), "");

        releases.Where(r => r.IsNew).Should().HaveCount(3);
    }

    [TestMethod]
    public void Releases_ShouldContainOnlyValidVersions()
    {
        string json = File.ReadAllText(@"TestData\windows-releases-invalid-versions.json");
        ReleasesResponse releasesResponse = JsonConvert.DeserializeObject<ReleasesResponse>(json);
        Update.Releases.Releases releases = new(_logger, releasesResponse.Releases, Version.Parse("1.5.0"), "");

        releases.Should().HaveCount(2);
        releases.Should().Contain(r => r.Version == new Version("1.5.0"));
        releases.Should().Contain(r => r.Version == new Version("1.5.1"));
    }
}