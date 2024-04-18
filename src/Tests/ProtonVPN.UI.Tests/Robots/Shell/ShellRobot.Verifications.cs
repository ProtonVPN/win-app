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
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots.Shell;

public partial class ShellRobot
{
    // Give some margin for sidebar expected width
    private const int SIDEBAR_COLLAPSED_MIN_WIDTH = 50;
    private const int SIDEBAR_COLLAPSED_MAX_WIDTH = 55;
    private const int SIDEBAR_EXPANDED_MIN_WIDTH = 240;
    private const int SIDEBAR_EXPANDED_MAX_WIDTH = 360;

    public ShellRobot VerifyTitleBar()
    {
        Label applicationTitle = ApplicationTitleLabel;

        Assert.IsNotNull(applicationTitle);
        Assert.AreEqual("Proton VPN", applicationTitle.Text);

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
        Assert.IsNotNull(KillSwitchFeatureNavigationViewItem);
        Assert.IsNotNull(NetShieldFeatureNavigationViewItem);
        Assert.IsNotNull(PortForwardingFeatureNavigationViewItem);
        Assert.IsNotNull(SplitTunnelingFeatureNavigationViewItem);
        Assert.IsNotNull(SettingsNavigationViewItem);

        Assert.IsNotNull(NavigationHamburgerButton);
        Assert.IsNotNull(NavigationSidebar);

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

    public ShellRobot VerifyUserIsLoggedIn(TestUserData user, string planName)
    {
        Assert.IsNotNull(ElementByName(user.Username));
        Assert.IsNotNull(ElementByName(planName));
        return this;
    }

    public ShellRobot VerifyAccountMenu()
    {
        Assert.IsNotNull(GoToMyAccountButton);
        Assert.IsNotNull(SignOutButton);
        Assert.IsNotNull(ExitButton);
        return this;
    }

    public ShellRobot VerifySidebarIsCollapsed()
    {
        AssertSidebarWidthIsCorrect(SIDEBAR_COLLAPSED_MIN_WIDTH, SIDEBAR_COLLAPSED_MAX_WIDTH);

        return this;
    }

    public ShellRobot VerifySidebarIsExpanded()
    {
        AssertSidebarWidthIsCorrect(SIDEBAR_EXPANDED_MIN_WIDTH, SIDEBAR_EXPANDED_MAX_WIDTH);

        return this;
    }

    public ShellRobot VerifySignOutConfirmationMessage(TestUserData user)
    {        
        Assert.IsNotNull(OverlayMessage);
        WaitUntilTextMatches(() => OverlayMessageTitle, TestConstants.VeryShortTimeout, $"Sign out from {user.Username}?");

        Assert.IsNotNull(OverlayMessagePrimaryButton);
        Assert.IsNotNull(OverlayMessageCloseButton);

        return this;
    }

    private void AssertSidebarWidthIsCorrect(int minExpectedWidth, int maxExpectedWidth)
    {
        double currentWidth = 0;

        Assert.IsNotNull(NavigationSidebar);

        RetryResult<bool> retry = Retry.WhileFalse(() =>
        {
            currentWidth = NavigationSidebar.ActualWidth;
            return currentWidth >= minExpectedWidth && currentWidth <= maxExpectedWidth;
        }, TestConstants.VeryShortTimeout, TestConstants.DefaultAnimationDelay);


        Assert.IsTrue(retry.Result, $"Sidebar does not have the expected width. Expected: between {minExpectedWidth}px and {maxExpectedWidth}px | Current: {currentWidth}px");
    }
}