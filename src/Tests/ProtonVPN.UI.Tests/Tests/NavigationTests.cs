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

using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots.Shell;

namespace ProtonVPN.UI.Tests.Tests;

[TestFixture]
[Category("UI")]
public class NavigationTests : TestSession
{
    private ShellRobot _shellRobot = new ShellRobot();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();

        _shellRobot
            .VerifyTitleBar()
            .VerifyNavigationView()
            .VerifyAccountButton()
            .VerifySidebarIsExpanded();
    }

    [Test]
    public void SidebarInteraction()
    {
        _shellRobot
            .DoCollapseExpandSideBar()
            .VerifySidebarIsCollapsed()
            .DoCollapseExpandSideBar()
            .VerifySidebarIsExpanded();
    }

    [Test]
    public void PageNavigation()
    {
        _shellRobot
            .DoNavigateToCountriesPage()
            .VerifyCurrentPageName("Countries")
            .DoNavigateToSettingsPage()
            .VerifyCurrentPageName("Settings")
            .DoNavigateToHomePage();
    }

    [Test]
    public void PageNavigationWhenSidebarCollapsed()
    {
        _shellRobot
            .DoCollapseExpandSideBar()
            .VerifySidebarIsCollapsed();

        PageNavigation();

        _shellRobot
            .DoCollapseExpandSideBar()
            .VerifySidebarIsExpanded();
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}