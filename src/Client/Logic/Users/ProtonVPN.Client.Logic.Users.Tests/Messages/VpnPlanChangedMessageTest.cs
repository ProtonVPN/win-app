﻿/*
 * Copyright (c) 2024 Proton AG
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
    [DataRow("old", 0, "new", 2, true)]
    [DataRow("old", 2, "new", 0, true)]
    [DataRow("old", 0, "new", 0, true)]
    [DataRow("old", 1, "new", 1, true)]
    [DataRow("old", 2, "new", 2, true)]
    [DataRow("old", 3, "new", 3, true)]
    [DataRow("same", 0, "same", 1, true)]
    [DataRow("same", 0, "same", 2, true)]
    [DataRow("same", 0, "same", 3, true)]
    [DataRow("same", 1, "same", 0, true)]
    [DataRow("same", 1, "same", 2, true)]
    [DataRow("same", 1, "same", 3, true)]
    [DataRow("same", 2, "same", 0, true)]
    [DataRow("same", 2, "same", 1, true)]
    [DataRow("same", 2, "same", 3, true)]
    [DataRow("same", 3, "same", 0, true)]
    [DataRow("same", 3, "same", 1, true)]
    [DataRow("same", 3, "same", 2, true)]
    [DataRow("same", 0, "same", 0, false)]
    [DataRow("same", 1, "same", 1, false)]
    [DataRow("same", 2, "same", 2, false)]
    [DataRow("same", 3, "same", 3, false)]
    public void TestHasChanged(string oldPlanTitle, int oldPlanMaxTier,
        string newPlanTitle, int newPlanMaxTier, bool expectedResult)
    {
        VpnPlan oldPlan = new(oldPlanTitle, oldPlanTitle, (sbyte)oldPlanMaxTier);
        VpnPlan newPlan = new(newPlanTitle, newPlanTitle, (sbyte)newPlanMaxTier);
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
        VpnPlan oldPlan = new("test", "test", (sbyte)oldPlanMaxTier);
        VpnPlan newPlan = new("test", "test", (sbyte)newPlanMaxTier);
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
        VpnPlan oldPlan = new("test", "test", (sbyte)oldPlanMaxTier);
        VpnPlan newPlan = new("test", "test", (sbyte)newPlanMaxTier);
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
        VpnPlan oldPlan = new("test", "test", (sbyte)oldPlanMaxTier);
        VpnPlan newPlan = new("test", "test", (sbyte)newPlanMaxTier);
        VpnPlanChangedMessage vpnPlanChangedMessage = new(oldPlan, newPlan);

        bool result = vpnPlanChangedMessage.HasMaxTierChanged();

        Assert.AreEqual(expectedResult, result);
    }
}