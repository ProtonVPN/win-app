/*
 * Copyright (c) 2025 Proton AG
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Documents;
using FlaUI.Core.WindowsAPI;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
[Category("ARM")]
public class NavigationTests : FreshSessionSetUp
{
    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    public void NavigateToSettingsViaKebabMenu()
    {
        HomeRobot.ExpandKebabMenuButton()
            .NavigateToSettingsViaKebabMenu();

        SettingRobot.Verify.IsSettingsPageDisplayed();
    }

    [Test]
    public void AppExitViaKebabMenu()
    {
        HomeRobot.ExpandKebabMenuButton()
            .ExitViaKebabMenu();

        // Allow some delay after exiting the app
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        Assert.That(AreNoProtonVPNProcessesRunning, Is.True, "ProtonVPN process was still running after app was exited.");
    }

    [Test]
    public void AppExitViaAccountDropDown()
    {
        SettingRobot
            .OpenSettings()
            .ExpandAccountDropdown()
            .ExitTheApp();

        // Allow some delay after exiting the app
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        Assert.That(AreNoProtonVPNProcessesRunning, Is.True, "ProtonVPN process was still running after app was exited.");
    }

    [Test]
    public void ClickingOnSidebarClosesSettings()
    {
        SettingRobot.OpenSettings()
            .Verify.IsSettingsPageDisplayed();
        SidebarRobot.ClickOnSidebar();
        SettingRobot.Verify.IsSettingsPageNotDisplayed();
    }

    [Test]
    public void KeyboardShortcutsNavigateToRelevantComponents()
    {
        SidebarRobot
            .Verify.IsSidebarConnectionsDisplayed()
            .ShortcutTo(VirtualKeyShort.NUMPAD1)
            .Verify.IsSidebarRecentsDisplayed()
            .ShortcutTo(VirtualKeyShort.NUMPAD2)
            .Verify.IsSidebarCountriesDisplayed()
            .ShortcutTo(VirtualKeyShort.NUMPAD3)
            .Verify.IsSidebarProfilesDisplayed()
            .ShortcutTo(VirtualKeyShort.KEY_1)
            .Verify.IsSidebarRecentsDisplayed()
            .ShortcutTo(VirtualKeyShort.KEY_2)
            .Verify.IsSidebarCountriesDisplayed()
            .ShortcutTo(VirtualKeyShort.KEY_3)
            .Verify.IsSidebarProfilesDisplayed()
            .ShortcutTo(VirtualKeyShort.KEY_F)
            .Verify.IsSidebarSearchResultsDisplayed()
            .ExitSearchWithTab()
            .Verify.IsSidebarConnectionsDisplayed();

        Thread.Sleep(TestConstants.OneSecondTimeout);

        Window.Focus();

        Thread.Sleep(TestConstants.OneSecondTimeout);

        SettingRobot
            .OpenSettingsViaShortcut()
            .Verify.IsSettingsPageDisplayed();

        Thread.Sleep(TestConstants.OneSecondTimeout);

        Window.Focus();

        Thread.Sleep(TestConstants.OneSecondTimeout);

        SettingRobot
            .CloseSettingsUsingEscButton()
            .Verify.IsSettingsPageNotDisplayed();
    }

    [Test]
    public void AboutPageIsOpened()
    {
        SettingRobot
            .OpenSettings()
            .ScrollToAboutSection()
            .Verify.IsCorrectAppVersionDisplayedInAboutSettingsCard(TestEnvironment.GetAppVersion())
            .OpenAboutSection()
            .Verify
                .IsChangelogDispalyed()
                .IsCorrectAppVersionDisplayedInAboutSection(TestEnvironment.GetAppVersion())
            .PressLearnMore()
            .Verify.IsLicensingDisplayed();
    }


    [Test]
    public void NavigateToProfiles()
    {
        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnConnectionsPage();
        SidebarRobot
            .NavigateToProfiles();
        NavigationRobot
            .Verify.IsOnHomePage()
                   .IsOnProfilesPage();

        List<string> profileNames = ["Streaming US", "Gaming", "P2P"];

        foreach (string profileName in profileNames)
        {
            SidebarRobot
                .ExpandSecondaryActionsForProfile(profileName)
                .EditProfile();
            NavigationRobot
                .Verify.IsOnProfilePage();
            ProfileRobot
                .Verify.DoesProfileNameEqual(profileName)
                .CloseProfile();
            NavigationRobot
                .Verify.IsOnHomePage();
        }
    }

    public static bool AreNoProtonVPNProcessesRunning() =>
        !Process.GetProcesses().Any(p => new[] { "ProtonVPN", "ProtonVPNService" }.Contains(p.ProcessName, StringComparer.OrdinalIgnoreCase));
}
