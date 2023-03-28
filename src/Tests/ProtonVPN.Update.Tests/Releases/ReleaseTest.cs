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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Update.Releases;
using ProtonVPN.Update.Responses;

namespace ProtonVPN.Update.Tests.Releases
{
    [TestClass]
    public class ReleaseTest
    {
        [TestMethod]
        public void Version_ShouldBe_Release_Version()
        {
            ReleaseResponse contract = new() { Version = "3.2.1" };
            Release release = new(contract, false, new Version());

            Version result = release.Version;

            result.Major.Should().Be(3);
            result.Minor.Should().Be(2);
            result.Build.Should().Be(1);
            result.Revision.Should().Be(-1);
        }

        [TestMethod]
        public void ChangeLog_ShouldBe_Release_ChangeLog()
        {
            string[] changeLog = new[] { "Super new changes.", "Super new feature.", "Fixed bugs!" };
            ReleaseResponse contract = new() { Version = "3.2.1", ChangeLog = changeLog };
            Release release = new(contract, false, new Version());

            IReadOnlyList<string> result = release.ChangeLog;

            result.Should().ContainInOrder(changeLog);
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void EarlyAccess_ShouldBe_EarlyAccess(bool expected)
        {
            ReleaseResponse contract = new() { Version = "1.1.1" };
            Release release = new(contract, expected, new Version());

            bool result = release.EarlyAccess;

            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(false, "1.2.3", "0.0.0")]
        [DataRow(false, "1.2.3", "1.2.3")]
        [DataRow(true, "1.2.3", "1.2.4")]
        [DataRow(true, "1.2.3", "10.10.10")]
        public void New_ShouldBe_WhenReleaseVersion_AndCurrentVersion(bool expected, string releaseVersion, string currentVersion)
        {
            ReleaseResponse contract = new()
            { Version = releaseVersion };
            Release release = new(contract, expected, Version.Parse(currentVersion));

            bool result = release.EarlyAccess;

            result.Should().Be(expected);
        }

        [TestMethod]
        public void File_ShouldBe_Release_File()
        {
            FileResponse file = new();
            ReleaseResponse contract = new() { Version = "9.9.9", File = file };
            Release release = new(contract, false, new Version());

            FileResponse result = release.File;

            result.Should().BeSameAs(file);
        }

        [TestMethod]
        public void Empty_ShouldBeTrue_WhenReleaseVersion_IsZero()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "012345", Arguments = "-install" };
            ReleaseResponse contract = new() { Version = "0.0.0", ChangeLog = new[] { "Fixed" }, File = file };
            Release release = new(contract, false, new Version());

            bool result = release.Empty();

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Empty_ShouldBeFalse_WhenReleaseVersion_IsNotZero()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "012345", Arguments = "-install" };
            ReleaseResponse contract = new() { Version = "0.0.1", ChangeLog = new[] { "Fixed" }, File = file };
            Release release = new(contract, false, new Version());

            bool result = release.Empty();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void EmptyRelease_Should_BeEmpty()
        {
            Release result = Release.EmptyRelease();

            result.Empty().Should().BeTrue();
        }

        [TestMethod]
        public void EmptyRelease_ShouldHave_ZeroVersion()
        {
            Release result = Release.EmptyRelease();

            result.Version.Major.Should().Be(0);
            result.Version.Minor.Should().Be(0);
            result.Version.Build.Should().Be(0);
            result.Version.Revision.Should().Be(-1);
        }

        [TestMethod]
        public void Release_ShouldImplement_IComparable()
        {
            Release release = new(new ReleaseResponse { Version = "1.2.3" }, false, new Version());

            release.Should().BeAssignableTo<IComparable<IRelease>>();
        }

        [DataTestMethod]
        [DataRow(-1, "0.0.0", "1.2.3")]
        [DataRow(-1, "1.2.2", "1.2.3")]
        [DataRow(0, "1.2.3", "1.2.3")]
        [DataRow(1, "1.2.4", "1.2.3")]
        [DataRow(1, "10.20.30", "1.2.3")]
        public void CompareTo_ShouldCompare_ReleaseVersions(int expected, string version, string otherVersion)
        {
            Release release = new(new ReleaseResponse { Version = version }, false, new Version());
            Release other = new(new ReleaseResponse { Version = otherVersion }, true, new Version());

            int result = release.CompareTo(other);

            result.Should().Be(expected);
        }
    }
}
