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

using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Update.Files;

namespace ProtonVPN.Update.Tests.Files
{
    [TestClass]
    public class FileLocationTest
    {
        [TestMethod]
        public void Path_ShouldBe_CombinedFromFolderAndFilename()
        {
            const string folder = @"C:\Windows\Temp\Downloads";
            const string filename = "ProtonVPN_win_v3.3.3.exe";
            string url = $"https://the.proton.site/downloads/{filename}";

            string expected = Path.Combine(folder, filename);
            FileLocation location = new FileLocation(folder);

            string result = location.Path(url);

            result.Should().Be(expected);
        }
    }
}
