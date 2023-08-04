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

using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots.Shell;

public partial class ShellRobot
{
    private const int SIDEBAR_COLLAPSED_WIDTH = 48;
    private const int SIDEBAR_EXPANDED_WIDTH = 160;

    public ShellRobot VerifyTitleBar()
    {
        Label applicationTitle = ApplicationTitleLabel;

        Assert.IsNotNull(applicationTitle);
        Assert.AreEqual("Proton VPN", applicationTitle.Text, false);

        Assert.IsNotNull(ApplicationIcon);

        Assert.IsNotNull(ApplicationTitleBar);
        Assert.IsNotNull(MinimizeButton);
        Assert.IsNotNull(MaximizeButton);
        Assert.IsNotNull(CloseButton);

        return this;
    }

    public ShellRobot VerifyNavigationView()
    {
        Assert.IsNotNull(NavigationView);

        Assert.IsNotNull(HomeNavigationViewItem);
        Assert.IsNotNull(CountriesNavigationViewItem);
        Assert.IsNotNull(SettingsNavigationViewItem);

        Assert.IsNotNull(NavigationHamburgerButton);
        Assert.IsNotNull(NavigationSideBar);

        return this;
    }

    public ShellRobot VerifyCurrentPage(string expectedPageName, bool hasBackButton)
    {
        WaitUntilTextMatches(() => ActivePageTitleLabel, TestConstants.VeryShortTimeout, expectedPageName);

        if(hasBackButton)
        {
            Assert.IsNotNull(GoBackButton);
        }

        return this;
    }

    public ShellRobot VerifyAccountButton()
    {
        Assert.IsNotNull(AccountButton);

        return this;
    }

    public ShellRobot VerifySidebarIsCollapsed()
    {
        VerifySidebarWidth(SIDEBAR_COLLAPSED_WIDTH);

        return this;
    }

    public ShellRobot VerifySidebarIsExpanded()
    {
        VerifySidebarWidth(SIDEBAR_EXPANDED_WIDTH);

        return this;
    }

    private void VerifySidebarWidth(int expectedWidth)
    {
        Grid sidebar = NavigationSideBar;

        Assert.IsNotNull(sidebar);

        Retry.WhileException(() =>
        {
            Assert.AreEqual(expectedWidth, sidebar.ActualWidth, $"Side bar width: {sidebar.ActualWidth}px (expected: {expectedWidth}px)");
        }, TestConstants.VeryShortTimeout, TestConstants.DefaultAnimationDelay);
    }
}