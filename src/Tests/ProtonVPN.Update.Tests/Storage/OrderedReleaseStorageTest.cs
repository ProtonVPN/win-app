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
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Update.Releases;
using ProtonVPN.Update.Storage;

namespace ProtonVPN.Update.Tests.Storage;

[TestClass]
public class OrderedReleaseStorageTest
{
    private IReleaseStorage _origin;

    [TestInitialize]
    public void TestInitialize()
    {
        _origin = Substitute.For<IReleaseStorage>();
    }

    [TestMethod]
    public async Task Releases_ShouldCall_Origin_Releases()
    {
        OrderedReleaseStorage storage = new OrderedReleaseStorage(_origin);

        await storage.GetReleasesAsync();

        await _origin.Received(1).GetReleasesAsync();
    }

    [TestMethod]
    public async Task Releases_ShouldBe_InDescendingOrder()
    {
        Release[] releases = {
            new() {Version = new(0, 1, 2)},
            new() {Version = new(5, 5, 5)},
            new() {Version = new(4, 4, 4)},
            new() {Version = new(3, 3, 3)},
            new() {Version = new(2, 1, 1)},
            new() {Version = new(2, 1, 10)},
            new() {Version = new(2, 1, 9)},
            new() {Version = new(2, 1, 100)},
            new() {Version = new(2, 1, 0)}
        };

        _origin.GetReleasesAsync().Returns(releases);
        OrderedReleaseStorage storage = new OrderedReleaseStorage(_origin);

        IEnumerable<Release> result = await storage.GetReleasesAsync();

        result.Should().BeInDescendingOrder();
    }

    [TestMethod]
    public async Task Releases_ShouldHave_NoDuplicates()
    {
        Release[] releases = {
            new() {Version = new(0, 1, 2)},
            new() {Version = new(5, 5, 5)},
            new() {Version = new(4, 4, 4)},
            new() {Version = new(4, 4, 4)},
            new() {Version = new(2, 1, 0)}
        };

        _origin.GetReleasesAsync().Returns(releases);
        OrderedReleaseStorage storage = new OrderedReleaseStorage(_origin);

        IEnumerable<Release> result = await storage.GetReleasesAsync();

        result.Should().HaveCount(4);
    }

    [TestMethod]
    public async Task Releases_ShouldKeep_StableReleaseOverEarlyAccess()
    {
        Release[] releases = {
            new() {Version = new(1, 0, 0), IsEarlyAccess = true},
            new() {Version = new(1, 0, 0), IsEarlyAccess = false},
            new() {Version = new(2, 0, 0), IsEarlyAccess = false},
            new() {Version = new(2, 0, 0), IsEarlyAccess = true},
        };

        _origin.GetReleasesAsync().Returns(releases);
        OrderedReleaseStorage storage = new OrderedReleaseStorage(_origin);

        IEnumerable<Release> result = await storage.GetReleasesAsync();

        result.Should().HaveCount(2);
        result.First().Should().Be(releases[2]);
        result.Last().Should().Be(releases[1]);
    }
}