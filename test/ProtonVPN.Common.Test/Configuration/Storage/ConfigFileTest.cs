/*
 * Copyright (c) 2022 Proton Technologies AG
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
using ProtonVPN.Common.Configuration.Storage;

namespace ProtonVPN.Common.Test.Configuration.Storage
{
    [TestClass]
    public class ConfigFileTest
    {
        [TestMethod]
        public void Path_ShouldBe_Default()
        {
            const string expected = "ProtonVPN.config";
            var file = new ConfigFile();

            var result = file.Path();

            Path.GetFileName(result).Should().Be(expected);
        }
    }
}
