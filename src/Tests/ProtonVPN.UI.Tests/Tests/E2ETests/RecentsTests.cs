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

using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
[Category("SMOKE_3")]
public class RecentsTests : BaseTest
{
    private const string CONNECTION_NAME = "Fastest country";
    private const string COUNTRY_NAME = "Austria";
    private const string PROFILE_NAME = "Gaming";

    [OneTimeSetUp]
    public void SetUp()
    {
        LaunchClient();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    [Property("TestCaseId", "602418")]
    public void RecentIsAddedToList()
    {
        SidebarRobot
            .NavigateToRecents()
            .Verify.IsNoRecentsLabelDisplayed();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .Disconnect()
            .Verify.IsDisconnected();

        SidebarRobot
            .Verify.HasNoRecentsLabel()
                   .IsConnectionOptionDisplayed(CONNECTION_NAME)
                   .IsRecentsCountDisplayed(1)
           .NavigateToAllCountriesTab()
           .ConnectToCountry(COUNTRY_NAME);

        HomeRobot
            .Verify.IsConnected()
            .Disconnect()
            .Verify.IsDisconnected();

        SidebarRobot
            .NavigateToRecents()
            .Verify.IsConnectionOptionDisplayed(COUNTRY_NAME)
                   .IsRecentsCountDisplayed(2);
    }

    [Test, Order(1)]
    [Property("TestCaseId", "602425")]
    public void ProfilesAreAddedToRecentList()
    {
        SidebarRobot
            .NavigateToProfiles()
            .ConnectToProfile(PROFILE_NAME);

        HomeRobot
            .Verify.IsConnected()
            .Disconnect()
            .Verify.IsDisconnected();

        SidebarRobot
            .NavigateToRecents()
            .Verify.IsConnectionOptionDisplayed(PROFILE_NAME)
            .IsRecentsCountDisplayed(3);
    }

    [Test, Order(2)]
    [Property("TestCaseId", "602419")]
    public void RemoveRecentFromList()
    {
        SidebarRobot
            .ExpandSecondaryActionsForRecents(CONNECTION_NAME)
            .RemoveRecent()
            .Verify.IsConnectionOptionMissing(CONNECTION_NAME)
                   .IsRecentsCountDisplayed(2);
    }

    [Test, Order(3)]
    [Property("TestCaseId", "602420")]
    public void PinRecentFromList()
    {
        SidebarRobot
            .Verify.IsConnectionOptionDisplayed(PROFILE_NAME)
                   .IsRecentsCountDisplayed(2)
                   .IsPinnedCountMissing()
            .ExpandSecondaryActionsForRecents(PROFILE_NAME)
            .PinRecent()
            .Verify.IsPinnedCountDisplayed(1)
                   .IsRecentsCountDisplayed(1);
    }

    [Test, Order(4)]
    [Property("TestCaseId", "800922")]
    public void UnpinRecentFromList()
    {
        SidebarRobot
            .Verify.IsConnectionOptionDisplayed(PROFILE_NAME)
                   .IsRecentsCountDisplayed(1)
                   .IsPinnedCountDisplayed(1)
            .ExpandSecondaryActionsForRecents(PROFILE_NAME)
            .UnpinRecent()
            .Verify.IsPinnedCountMissing()
                   .IsRecentsCountDisplayed(2);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Cleanup();
    }
}
