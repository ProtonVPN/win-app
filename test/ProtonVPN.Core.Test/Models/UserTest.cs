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
using ProtonVPN.Core.User;

namespace ProtonVPN.Core.Test.Models
{
    [TestClass]
    public class UserTest
    {
        [DataTestMethod]
        [DataRow(0, "?")]
        [DataRow(1, "ProtonMail Account")]
        [DataRow(2, "?")]
        [DataRow(3, "?")]
        [DataRow(4, "ProtonVPN Account")]
        [DataRow(5, "ProtonMail Account")]
        [DataRow(6, "?")]
        public void GetAccountPlan_ShouldBe_MappedFormServices(int services, string expected)
        {
            // Arrange
            var user = new Core.Models.User { Services = services };
            // Act
            var result = user.GetAccountPlan();
            // Assert
            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(null, 0, PlanStatus.Free)]
        [DataRow(null, 1, PlanStatus.Free)]
        [DataRow("free", 0, PlanStatus.Free)]
        [DataRow("free", 1, PlanStatus.Free)]
        [DataRow("trial", 0, PlanStatus.TrialNotStarted)]
        [DataRow("trial", 1, PlanStatus.TrialStarted)]
        [DataRow("basic", 0, PlanStatus.Paid)]
        [DataRow("basic", 1, PlanStatus.Paid)]
        [DataRow("plus", 0, PlanStatus.Paid)]
        [DataRow("plus", 1, PlanStatus.Paid)]
        [DataRow("visionary", 0, PlanStatus.Paid)]
        [DataRow("visionary", 1, PlanStatus.Paid)]
        public void TrialStatus_ShouldBe_MappedFromVpnPlan_AndExpirationTime(string vpnPlan, int expirationTime, PlanStatus expected)
        {
            // Arrange
            var user = new Core.Models.User { VpnPlan = vpnPlan, ExpirationTime = expirationTime };
            // Act
            var result = user.TrialStatus();
            // Assert
            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow("free", false)]
        [DataRow("trial", false)]
        [DataRow("basic", true)]
        [DataRow("plus", true)]
        [DataRow("visionary", true)]
        public void Paid_ShouldBe_MappedFromVpnPlan(string vpnPlan, bool expected)
        {
            // Arrange
            var user = new Core.Models.User { VpnPlan = vpnPlan };
            // Act
            var result = user.Paid();
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
            var user = new Core.Models.User {Username = username};
            // Act
            var result = user.Empty();
            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void EmptyUser_ShouldBe_Empty()
        {
            // Act
            var user = Core.Models.User.EmptyUser();
            // Assert
            user.Empty().Should().BeTrue();
        }
    }
}
