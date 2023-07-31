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

using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots.Home;

public partial class HomeRobot
{
    public HomeRobot DoConnect()
    {
        ConnectionCardConnectButton.Invoke();
        return this;
    }

    public HomeRobot DoCancelConnection()
    {
        ConnectionCardCancelButton.Invoke();
        return this;
    }

    public HomeRobot DoDisconnect()
    {
        ConnectionCardDisconnectButton.Invoke();
        return this;
    }

    public HomeRobot DoOpenConnectionDetails()
    {
        ConnectionCardShowConnectionDetailsButton.Click();
        return this;
    }

    public HomeRobot DoCloseConnectionDetails()
    {
        ConnectionDetailsCloseButton.Click();
        return this;
    }

    public HomeRobot DoOpenServerLoadOverlay()
    {
        ConnectionDetailServerLoadButton.Click();
        return this;
    }

    public HomeRobot DoOpenLatencyOverlay()
    {
        ConnectionDetailLatencyButton.Click();
        return this;
    }

    public HomeRobot DoOpenProtocolOverlay()
    {
        ConnectionDetailProtocolButton.Click();
        return this;
    }

    public HomeRobot DoWaitForVpnStatusSubtitleLabel()
    {
        VpnStatusSubtitleLabel.WaitUntilDisplayed(TestConstants.VeryShortTimeout);
        return this;
    }

    public HomeRobot DoOpenAccount()
    {
        AccountButton.Click();
        return this;
    }

    public HomeRobot DoSignOut()
    {
        SignOutButton.Click();
        return this;
    }

    public HomeRobot DoReportBrowsingSpeedIssue()
    {
        HelpButton.FocusAndClick();
        IssueCategoryMenuItem.Click();
        return this;
    }
}