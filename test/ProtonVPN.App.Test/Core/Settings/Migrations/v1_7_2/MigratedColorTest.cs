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

using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Settings.Migrations.v1_7_2;

namespace ProtonVPN.App.Test.Core.Settings.Migrations.v1_7_2
{
    [TestClass]
    public class MigratedColorTest
    {
        [TestMethod]
        public void Map_ShouldContain_AllowedValuesOnly()
        {
            var allowedColors = new ColorProvider().GetColors();

            var usedColors = MigratedColorCode.Map.Values.Select(v => v);

            usedColors.Should().BeSubsetOf(allowedColors);
        }

    }
}
