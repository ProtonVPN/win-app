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
using ProtonVPN.Update.Releases;
using ProtonVPN.Update.Responses;

namespace ProtonVPN.Update.Tests.Releases
{
    [TestClass]
    public class ReleasesTest
    {
        [TestMethod]
        public void Releases_ShouldImplement_IEnumerable()
        {
            CategoryResponse[] categories = new CategoryResponse[0];
            Update.Releases.Releases releases = new(categories, new Version(), "");

            releases.Should().BeAssignableTo<IEnumerable<Release>>();
        }

        [TestMethod]
        public void Releases_ShouldBeEmpty_WhenCategories_AreEmpty()
        {
            CategoryResponse[] categories = new CategoryResponse[0];
            Update.Releases.Releases releases = new(categories, new Version(), "");

            releases.Should().HaveCount(0);
        }

        [TestMethod]
        public void Releases_ShouldBeEmpty_WhenCategories_HaveNoReleases()
        {
            CategoryResponse[] categories = { new() { Name = "Stable" } };
            Update.Releases.Releases releases = new(categories, new Version(), "");

            releases.Should().HaveCount(0);
        }

        [TestMethod]
        public void Releases_ShouldContain_AllReleases_FromCategories()
        {
            string json = File.ReadAllText(@"TestData\win-update.json");
            CategoriesResponse categories = JsonConvert.DeserializeObject<CategoriesResponse>(json);
            Update.Releases.Releases releases = new(categories.Categories, new Version(), "");

            releases.Should().HaveCount(5);
        }

        [TestMethod]
        public void Releases_Version_ShouldBe_FromReleases()
        {
            string json = File.ReadAllText(@"TestData\win-update.json");
            CategoriesResponse categories = JsonConvert.DeserializeObject<CategoriesResponse>(json);
            List<Version> expected = categories.Categories.SelectMany(c => c.Releases).Select(r => Version.Parse(r.Version)).ToList();

            Update.Releases.Releases releases = new(categories.Categories, new Version(), "");

            IEnumerable<Version> result = releases.Select(r => r.Version);

            result.Should()
                .OnlyHaveUniqueItems().And
                .Contain(expected);
        }

        [TestMethod]
        public void Releases_EarlyAccess_ShouldBeTrue_ForEarlyAccess()
        {
            string json = File.ReadAllText(@"TestData\win-update.json");
            CategoriesResponse categories = JsonConvert.DeserializeObject<CategoriesResponse>(json);
            Update.Releases.Releases releases = new(categories.Categories, new Version(), "EarlyAccess");

            releases.Where(r => r.EarlyAccess).Should().HaveCount(2);
        }

        [TestMethod]
        public void Releases_New_ShouldBeTrue_ForNewReleases()
        {
            string json = File.ReadAllText(@"TestData\win-update.json");
            CategoriesResponse categories = JsonConvert.DeserializeObject<CategoriesResponse>(json);
            Update.Releases.Releases releases = new(categories.Categories, Version.Parse("1.5.0"), "");

            releases.Where(r => r.New).Should().HaveCount(3);
        }
    }
}