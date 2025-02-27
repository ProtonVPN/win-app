/*
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

using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
public class RecentsTests : BaseTest
{
    private const string CONNECTION_NAME = "Fastest country";

    [OneTimeSetUp]
    public void SetUp()
    {
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    public void RecentIsAddedToList()
    {
        SidebarRobot.NavigateToRecents()
            .Verify.IsNoRecentsLabelDisplayed();

        HomeRobot.ConnectViaConnectionCard()
            .Verify.IsConnected()
            .Disconnect()
            .Verify.IsDisconnected();

        SidebarRobot.Verify.HasNoRecentsLabel()
            .IsConnectionOptionDisplayed(CONNECTION_NAME)
            .IsRecentsCountDisplayed(1);
    }

    [Test, Order(1)]
    public void RecentDeletion()
    {
        Thread.Sleep(TestConstants.AnimationDelay);

        SidebarRobot
            .ExpandSecondaryActionsForRecents(CONNECTION_NAME)
            .RemoveRecent();

        SidebarRobot.Verify.IsNoRecentsLabelDisplayed();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Cleanup();
    }
}
