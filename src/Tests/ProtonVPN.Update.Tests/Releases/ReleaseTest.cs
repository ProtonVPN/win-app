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
        public void Empty_ShouldBeTrue_WhenReleaseVersion_IsZero()
        {
            Release release = new() { ChangeLog = new List<string>(), Version = new(0, 0, 0) };
            bool result = release.IsEmpty();
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Empty_ShouldBeFalse_WhenReleaseVersion_IsNotZero()
        {
            FileResponse file = new()
            {
                Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "012345", Args = "-install"
            };
            Release release = new()
            {
                ChangeLog = new[] { "Fixed" },
                IsEarlyAccess = false,
                File = file,
                IsNew = true,
                Version = new(0, 0, 1),
            };

            bool result = release.IsEmpty();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void EmptyRelease_Should_BeEmpty()
        {
            Release result = Release.Empty;

            result.IsEmpty().Should().BeTrue();
        }

        [TestMethod]
        public void EmptyRelease_ShouldHave_ZeroVersion()
        {
            Release result = Release.Empty;

            result.Version.Major.Should().Be(0);
            result.Version.Minor.Should().Be(0);
            result.Version.Build.Should().Be(0);
            result.Version.Revision.Should().Be(-1);
        }

        [TestMethod]
        public void Release_ShouldImplement_IComparable()
        {
            Release release = new() { Version = new(1, 2, 3), IsEarlyAccess = false, IsNew = true, };

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
            Release release = new() { Version = Version.Parse(version) };
            Release other = new() { Version = Version.Parse(otherVersion) };

            int result = release.CompareTo(other);

            result.Should().Be(expected);
        }
    }
}