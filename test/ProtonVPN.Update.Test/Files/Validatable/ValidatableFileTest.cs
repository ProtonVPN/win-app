/*
 * Copyright (c) 2020 Proton Technologies AG
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

using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Update.Files.Validatable;

namespace ProtonVPN.Update.Test.Files.Validatable
{
    [TestClass]
    [DeploymentItem("TestData", "TestData")]
    public class ValidatableFileTest
    {
        [TestMethod]
        public async Task Valid_ShouldBeTrue_WhenValidChecksum()
        {
            var filename = Path.Combine("TestData", "ProtonVPN_win_v1.5.2.exe");
            var validatable = new ValidatableFile();

            var result = await validatable.Valid(filename, "93c8fc621f9f8721b41ba4093ae7bec78956e7d8");

            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task Valid_ShouldBeFalse_WhenInvalidChecksum()
        {
            var filename = Path.Combine("TestData", "ProtonVPN_win_v1.5.2.exe");
            var validatable = new ValidatableFile();

            var result = await validatable.Valid(filename, "03c8fc621f9f8721b41ba4093ae7bec78956e7d8");

            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task Valid_ShouldBeFalse_WhenFileNotExists()
        {
            var filename = Path.Combine("TestData", "FileNotExists.exe");
            var validatable = new ValidatableFile();

            var result = await validatable.Valid(filename, "Value doesn't matter");

            result.Should().BeFalse();
        }
    }
}
