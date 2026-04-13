/*
 * Copyright (c) 2026 Proton AG
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;

namespace ProtonVPN.Client.Logic.Users.Tests.Messages;

[TestClass]
public class VpnPlanChangedMessageTest
{
    [TestMethod]
    [DataRow("old", "oldName", 0, false, "new", "newName", 2, false, true)]
    [DataRow("old", "oldName", 2, false, "new", "newName", 0, false, true)]
    [DataRow("old", "oldName", 0, false, "new", "newName", 0, false, true)]
    [DataRow("old", "oldName", 1, false, "new", "newName", 1, false, true)]
    [DataRow("old", "oldName", 2, false, "new", "newName", 2, false, true)]
    [DataRow("old", "oldName", 3, false, "new", "newName", 3, false, true)]
    [DataRow("same", "oldName", 0, false, "same", "oldName", 1, false, true)]
    [DataRow("same", "oldName", 0, false, "same", "oldName", 2, false, true)]
    [DataRow("same", "oldName", 0, false, "same", "oldName", 3, false, true)]
    [DataRow("same", "oldName", 1, false, "same", "oldName", 0, false, true)]
    [DataRow("same", "oldName", 1, false, "same", "oldName", 2, false, true)]
    [DataRow("same", "oldName", 1, false, "same", "oldName", 3, false, true)]
    [DataRow("same", "oldName", 2, false, "same", "oldName", 0, false, true)]
    [DataRow("same", "oldName", 2, false, "same", "oldName", 1, false, true)]
    [DataRow("same", "oldName", 2, false, "same", "oldName", 3, false, true)]
    [DataRow("same", "oldName", 3, false, "same", "oldName", 0, false, true)]
    [DataRow("same", "oldName", 3, false, "same", "oldName", 1, false, true)]
    [DataRow("same", "oldName", 3, false, "same", "oldName", 2, false, true)]
    [DataRow("same", "oldName", 0, false, "same", "oldName", 0, false, false)]
    [DataRow("same", "oldName", 1, false, "same", "oldName", 1, false, false)]
    [DataRow("same", "oldName", 2, false, "same", "oldName", 2, false, false)]
    [DataRow("same", "oldName", 3, false, "same", "oldName", 3, false, false)]
    [DataRow("same", "oldName", 0, false, "same", "oldName", 0, true, true)]
    [DataRow("same", "oldName", 1, false, "same", "oldName", 1, true, true)]
    [DataRow("same", "oldName", 2, false, "same", "oldName", 2, true, true)]
    [DataRow("same", "oldName", 3, false, "same", "oldName", 3, true, true)]
    [DataRow("same", "oldName", 0, true, "same", "oldName", 0, false, true)]
    [DataRow("same", "oldName", 1, true, "same", "oldName", 1, false, true)]
    [DataRow("same", "oldName", 2, true, "same", "oldName", 2, false, true)]
    [DataRow("same", "oldName", 3, true, "same", "oldName", 3, false, true)]
    [DataRow("same", "oldName", 0, true, "same", "oldName", 0, true, false)]
    [DataRow("same", "oldName", 1, true, "same", "oldName", 1, true, false)]
    [DataRow("same", "oldName", 2, true, "same", "oldName", 2, true, false)]
    [DataRow("same", "oldName", 3, true, "same", "oldName", 3, true, false)]
    public void TestHasChanged(string oldPlanTitle, string oldPlanName, int oldPlanMaxTier, bool oldPlanIsB2B,
        string newPlanTitle, string newPlanName, int newPlanMaxTier, bool newPlanIsB2B, bool expectedResult)
    {
        VpnPlan oldPlan = new(oldPlanTitle, oldPlanName, (sbyte)oldPlanMaxTier, oldPlanIsB2B);
        VpnPlan newPlan = new(newPlanTitle, newPlanName, (sbyte)newPlanMaxTier, newPlanIsB2B);
        VpnPlanChangedMessage vpnPlanChangedMessage = new(oldPlan, newPlan);

        bool result = vpnPlanChangedMessage.HasChanged();

        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow(0, 0, false)]
    [DataRow(0, 1, false)]
    [DataRow(0, 2, false)]
    [DataRow(0, 3, false)]
    [DataRow(1, 0, true)]
    [DataRow(1, 1, false)]
    [DataRow(1, 2, false)]
    [DataRow(1, 3, false)]
    [DataRow(2, 0, true)]
    [DataRow(2, 1, true)]
    [DataRow(2, 2, false)]
    [DataRow(2, 3, false)]
    [DataRow(3, 0, true)]
    [DataRow(3, 1, true)]
    [DataRow(3, 2, true)]
    [DataRow(3, 3, false)]
    public void TestIsDowngrade(int oldPlanMaxTier, int newPlanMaxTier, bool expectedResult)
    {
        VpnPlan oldPlan = new("test", "test", (sbyte)oldPlanMaxTier, false);
        VpnPlan newPlan = new("test", "test", (sbyte)newPlanMaxTier, false);
        VpnPlanChangedMessage vpnPlanChangedMessage = new(oldPlan, newPlan);

        bool result = vpnPlanChangedMessage.IsDowngrade();

        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow(0, 0, false)]
    [DataRow(0, 1, true)]
    [DataRow(0, 2, true)]
    [DataRow(0, 3, true)]
    [DataRow(1, 0, false)]
    [DataRow(1, 1, false)]
    [DataRow(1, 2, true)]
    [DataRow(1, 3, true)]
    [DataRow(2, 0, false)]
    [DataRow(2, 1, false)]
    [DataRow(2, 2, false)]
    [DataRow(2, 3, true)]
    [DataRow(3, 0, false)]
    [DataRow(3, 1, false)]
    [DataRow(3, 2, false)]
    [DataRow(3, 3, false)]
    public void TestIsUpgrade(int oldPlanMaxTier, int newPlanMaxTier, bool expectedResult)
    {
        VpnPlan oldPlan = new("test", "test", (sbyte)oldPlanMaxTier, false);
        VpnPlan newPlan = new("test", "test", (sbyte)newPlanMaxTier, false);
        VpnPlanChangedMessage vpnPlanChangedMessage = new(oldPlan, newPlan);

        bool result = vpnPlanChangedMessage.IsUpgrade();

        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow(0, 0, false)]
    [DataRow(0, 1, true)]
    [DataRow(0, 2, true)]
    [DataRow(0, 3, true)]
    [DataRow(1, 0, true)]
    [DataRow(1, 1, false)]
    [DataRow(1, 2, true)]
    [DataRow(1, 3, true)]
    [DataRow(2, 0, true)]
    [DataRow(2, 1, true)]
    [DataRow(2, 2, false)]
    [DataRow(2, 3, true)]
    [DataRow(3, 0, true)]
    [DataRow(3, 1, true)]
    [DataRow(3, 2, true)]
    [DataRow(3, 3, false)]
    public void TestHasMaxTierChanged(int oldPlanMaxTier, int newPlanMaxTier, bool expectedResult)
    {
        VpnPlan oldPlan = new("test", "test", (sbyte)oldPlanMaxTier, false);
        VpnPlan newPlan = new("test", "test", (sbyte)newPlanMaxTier, false);
        VpnPlanChangedMessage vpnPlanChangedMessage = new(oldPlan, newPlan);

        bool result = vpnPlanChangedMessage.HasMaxTierChanged();

        Assert.AreEqual(expectedResult, result);
    }
}