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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Core.Models;

namespace ProtonVPN.Core.Tests.Models
{
    [TestClass]
    public class UserTest
    {
        [DataTestMethod]
        [DataRow(0, "ProtonMail Account")]
        [DataRow(1, "ProtonMail Account")]
        [DataRow(2, "ProtonMail Account")]
        [DataRow(3, "ProtonMail Account")]
        [DataRow(4, "ProtonVPN Account")]
        public void GetAccountPlan_ShouldBe_MappedFormServices(int services, string expected)
        {
            // Arrange
            User user = new() { Services = services };
            // Act
            string result = user.GetAccountPlan();
            // Assert
            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow("free", false)]
        [DataRow("basic", true)]
        [DataRow("plus", true)]
        [DataRow("visionary", true)]
        public void Paid_ShouldBe_MappedFromVpnPlan(string vpnPlan, bool expected)
        {
            // Arrange
            User user = new() { VpnPlan = vpnPlan };
            // Act
            bool result = user.Paid();
            // Assert
            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(null, true)]
        [DataRow("", true)]
        [DataRow("user.name", false)]
        public void Empty_ShouldBeTrue_WhenUsernameIsNullOrEmpty(string username, bool expected)
        {
            // Arrange
            User user = new() { Username = username };
            // Act
            bool result = user.Empty();
            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void EmptyUser_ShouldBe_Empty()
        {
            // Act
            User user = User.EmptyUser();
            // Assert
            user.Empty().Should().BeTrue();
        }
    }
}