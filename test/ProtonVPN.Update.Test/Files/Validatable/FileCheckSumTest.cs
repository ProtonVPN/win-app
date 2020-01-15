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
    public class FileCheckSumTest
    {
        [TestMethod]
        public async Task Value_ShouldBe_SHA1()
        {
            var filename = Path.Combine("TestData", "ProtonVPN_win_v1.5.1.exe");
            var fileCheckSum = new FileCheckSum(filename);

            var result = await fileCheckSum.Value();

            result.Should().Be("ba6b5ca2db65ff7817e3336a386e7525c01dc639");
        }
    }
}
