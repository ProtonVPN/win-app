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
    public class VpnErrorTypeContractTest
    {
        [TestMethod]
        public void Enum_ShouldHaveSame_UnderlyingType_As_VpnError()
        {
            // Arrange
            Type type = Enum.GetUnderlyingType(typeof(VpnErrorTypeContract));
            Type expected = Enum.GetUnderlyingType(typeof(VpnError));
            // Assert
            type.Should().BeSameAs(expected);
        }

        [TestMethod]
        public void Enum_ShouldHaveSame_Values_As_VpnError()
        {
            // Arrange
            int[] values = Enum.GetValues(typeof(VpnErrorTypeContract)).Cast<int>().ToArray();
            int[] expected = Enum.GetValues(typeof(VpnError)).Cast<int>().ToArray();
            // Assert
            values.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Enum_ShouldHaveSame_Names_As_VpnError()
        {
            // Arrange
            string[] values = Enum.GetNames(typeof(VpnErrorTypeContract));
            string[] expected = Enum.GetNames(typeof(VpnError));
            // Assert
            values.Should().BeEquivalentTo(expected);
        }
    }
}