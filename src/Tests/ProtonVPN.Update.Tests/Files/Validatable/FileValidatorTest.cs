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

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Tests.Common;
using ProtonVPN.Update.Files.Validatable;

namespace ProtonVPN.Update.Tests.Files.Validatable
{
    [TestClass]
    public class FileValidatorTest
    {
        [TestMethod]
        public async Task Valid_ShouldBeTrue_WhenValidChecksum()
        {
            string filename = TestConfig.GetFolderPath("ProtonVPN_win_v1.5.2.exe");
            FileValidator validatable = new();

            bool result = await validatable.Valid(filename, "6771cf15b98782e59716cefee4af6f5fc4d43e1a2a4fc14eb7cb80176de3210ee8342ce6fe28eb76f5a5765ac4d7efec312c1712581eaf2a1e5e8daae5c94e2a");

            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task Valid_ShouldBeFalse_WhenInvalidChecksum()
        {
            string filename = TestConfig.GetFolderPath("ProtonVPN_win_v1.5.2.exe");
            FileValidator validatable = new();

            bool result = await validatable.Valid(filename, "03c8fc621f9f8721b41ba4093ae7bec78956e7d8");

            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task Valid_ShouldBeFalse_WhenFileNotExists()
        {
            string filename = TestConfig.GetFolderPath("FileNotExists.exe");
            FileValidator validatable = new();

            bool result = await validatable.Valid(filename, "Value doesn't matter");

            result.Should().BeFalse();
        }
    }
}