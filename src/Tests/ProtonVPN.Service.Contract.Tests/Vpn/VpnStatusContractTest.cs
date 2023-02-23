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
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Service.Contract.Vpn;

namespace ProtonVPN.Service.Contract.Tests.Vpn
{
    [TestClass]
    public class VpnStatusContractTest
    {
        [TestMethod]
        public void Enum_ShouldHaveSame_UnderlyingType_As_VpnStatus()
        {
            // Arrange
            Type type = Enum.GetUnderlyingType(typeof(VpnStatusContract));
            Type expected = Enum.GetUnderlyingType(typeof(VpnStatus));
            // Assert
            type.Should().BeSameAs(expected);
        }

        [TestMethod]
        public void Enum_ShouldHaveSame_Values_As_VpnStatus()
        {
            // Arrange
            int[] values = Enum.GetValues(typeof(VpnStatusContract)).Cast<int>().ToArray();
            int[] expected = Enum.GetValues(typeof(VpnStatus)).Cast<int>().ToArray();
            // Assert
            values.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Enum_ShouldHaveSame_Names_As_VpnStatus()
        {
            // Arrange
            string[] values = Enum.GetNames(typeof(VpnStatusContract));
            string[] expected = Enum.GetNames(typeof(VpnStatus));
            // Assert
            values.Should().BeEquivalentTo(expected);
        }
    }
}