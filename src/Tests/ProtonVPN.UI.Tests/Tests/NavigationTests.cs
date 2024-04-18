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
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests;

[TestFixture]
[Category("UI")]
public class NavigationTests : TestSession
{
    private const string COUNTRIES_PAGE_TITLE = "Countries";
    private const string SETTINGS_PAGE_TITLE = "Settings";
    private const string NETSHIELD_PAGE_TITLE = "NetShield";
    private const string KILL_SWITCH_PAGE_TITLE = "Kill switch";
    private const string PORT_FORWARDING_PAGE_TITLE = "Port forwarding";
    private const string SPLIT_TUNNELING_PAGE_TITLE = "Split tunneling";

    private ShellRobot _shellRobot = new();
    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();

        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.PlusUser);

        _homeRobot
            .DoWaitForVpnStatusSubtitleLabel();

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
            .DoCollapseExpandSidebar()
            .VerifySidebarIsCollapsed()
            .DoCollapseExpandSidebar()
            .VerifySidebarIsExpanded();
    }

    [Test]
    public void PageNavigation()
    {
        _shellRobot
            .DoNavigateToCountriesPage()
            .VerifyCurrentPage(COUNTRIES_PAGE_TITLE, false)
            .DoNavigateToNetShieldFeaturePage()
            .VerifyCurrentPage(NETSHIELD_PAGE_TITLE, false)
            .DoNavigateToKillSwitchFeaturePage()
            .VerifyCurrentPage(KILL_SWITCH_PAGE_TITLE, false)
            .DoNavigateToPortForwardingFeaturePage()
            .VerifyCurrentPage(PORT_FORWARDING_PAGE_TITLE, false)
            .DoNavigateToSplitTunnelingFeaturePage()
            .VerifyCurrentPage(SPLIT_TUNNELING_PAGE_TITLE, false)
            .DoNavigateToSettingsPage()
            .VerifyCurrentPage(SETTINGS_PAGE_TITLE, false)
            .DoNavigateToHomePage();
    }

    [Test]
    public void PageNavigationWhenSidebarCollapsed()
    {
        _shellRobot
            .DoCollapseExpandSidebar()
            .VerifySidebarIsCollapsed();

        PageNavigation();

        _shellRobot
            .DoCollapseExpandSidebar()
            .VerifySidebarIsExpanded();
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}