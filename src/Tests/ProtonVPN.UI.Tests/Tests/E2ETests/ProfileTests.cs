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

using System;
using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
[Category("ARM")]
public class ProfileTests : BaseTest
{
    private const string PROFILE_NAME = "Profile A";

    [OneTimeSetUp]
    public void SetUp()
    {
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    public void DeleteAllProfiles()
    {
        NavigationRobot
            .Verify.IsOnConnectionsPage();
        SidebarRobot
            .NavigateToProfiles();
        NavigationRobot
            .Verify.IsOnProfilesPage();

        RemoveProfiles();

        SidebarRobot
            .Verify.NoProfilesLabelIsDisplayed();
    }

    [Test, Order(1)]
    public void CreateProfile()
    {
        SidebarRobot
            .CreateProfile();
        NavigationRobot
            .Verify.IsOnProfilePage();
        ProfileRobot
            .SetProfileName(PROFILE_NAME)
            .SaveProfile();
        SidebarRobot
            .ScrollToProfile(PROFILE_NAME)
            .Verify.DoesConnectionItemExist(PROFILE_NAME);
    }

    [Test, Order(2)]
    public void ConnectToProfile()
    {
        SidebarRobot
            .ConnectToProfile(PROFILE_NAME);

        HomeRobot
            .Verify.IsConnecting()
            .DoesConnectionCardTitleEqual(PROFILE_NAME)
            .Verify.IsConnected()
            .DoesConnectionCardTitleEqual(PROFILE_NAME);

        SettingRobot
            .Verify.IsNetshieldBlocking(NetShieldMode.BlockAdsMalwareTrackers);
    }

    [Test, Order(3)]
    public void EditProfile()
    {
        SidebarRobot
            .ScrollToProfile(PROFILE_NAME)
            .Verify.DoesConnectionItemExist(PROFILE_NAME)
            .ExpandSecondaryActionsForProfile(PROFILE_NAME)
            .EditProfile();

        ProfileRobot
            .ExpandSettingsSection()
            .DisableNetShield()
            .SaveProfile();

        HomeRobot
            .Verify.IsConnected();

        SettingRobot
            .Verify.IsNetshieldNotBlocking();
    }

    [Test, Order(4)]
    public void DisconnectFromProfile()
    {
        SidebarRobot
            .ScrollToProfile(PROFILE_NAME)
            .Verify.DoesConnectionItemExist(PROFILE_NAME)
            .DisconnectViaProfile(PROFILE_NAME);

        HomeRobot
            .Verify.IsDisconnected();
    }

    [Test, Order(5)]
    public void DeleteProfile()
    {
        SidebarRobot
            .ScrollToProfile(PROFILE_NAME)
            .Verify.DoesConnectionItemExist(PROFILE_NAME)
            .ExpandSecondaryActionsForProfile(PROFILE_NAME)
            .DeleteProfile();

        ConfirmationRobot
            .PrimaryAction();

        // Wait for profile to be deleted
        Thread.Sleep(500);

        SidebarRobot
            .Verify.DoesConnectionItemNotExist(PROFILE_NAME);
    }

    private void RemoveProfiles()
    {
        int profilesCount = SidebarRobot.GetProfileCount();
        for (int profileIndex = 0; profileIndex < profilesCount; profileIndex++)
        {
            SidebarRobot.ExpandFirstSecondaryActions()
                .DeleteProfile();

            ConfirmationRobot.PrimaryAction();
            // Wait for profile to be deleted
            Thread.Sleep(500);
        }
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Cleanup();
    }
}
