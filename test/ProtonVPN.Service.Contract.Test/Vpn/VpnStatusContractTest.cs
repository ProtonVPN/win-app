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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Service.Contract.Vpn;
using System;

namespace ProtonVPN.Service.Contract.Test.Vpn
{
    [TestClass]
    public class VpnStatusContractTest
    {
        [TestMethod]
        public void Enum_ShouldHaveSame_UnderlyingType_As_VpnStatus()
        {
            // Arrange
            var type = Enum.GetUnderlyingType(typeof(VpnStatusContract));
            var expected = Enum.GetUnderlyingType(typeof(VpnStatus));
            // Assert
            type.Should().BeSameAs(expected);
        }

        [TestMethod]
        public void Enum_ShouldHaveSame_Values_As_VpnStatus()
        {
            // Arrange
            var values = (int[])Enum.GetValues(typeof(VpnStatusContract));
            var expected = (int[])Enum.GetValues(typeof(VpnStatus));
            // Assert
            values.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Enum_ShouldHaveSame_Names_As_VpnStatus()
        {
            // Arrange
            var values = Enum.GetNames(typeof(VpnStatusContract));
            var expected = Enum.GetNames(typeof(VpnStatus));
            // Assert
            values.Should().BeEquivalentTo(expected);
        }
    }
}
