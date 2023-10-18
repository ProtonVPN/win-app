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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots.Home;

public partial class HomeRobot
{
    private const string UNPROTECTED_STATUS_SUBTITLE = "You are unprotected";
    private const string CONNECTING_STATUS_SUBTITLE = "Protecting your digital identity";
    private const string PROTECTED_STATUS_TITLE = "Protected";

    private const string CONNECTION_CARD_DEFAULT_HEADER = "Recommended";
    private const string CONNECTION_CARD_DISCONNECTED_HEADER = "Last connected to";
    private const string CONNECTION_CARD_CONNECTING_HEADER = "Connecting to...";
    private const string CONNECTION_CARD_CONNECTED_HEADER = "Browsing safely from";

    private const string CONNECTION_CARD_DEFAULT_TITLE = "Fastest country";

    public HomeRobot VerifyVpnStatusIsDisconnected()
    {
        VerifyVpnStatusLabels(null, UNPROTECTED_STATUS_SUBTITLE);

        return this;
    }

    public HomeRobot VerifyVpnStatusIsConnecting()
    {
        VerifyVpnStatusLabels(null, CONNECTING_STATUS_SUBTITLE);

        return this;
    }

    public HomeRobot VerifyVpnStatusIsConnected()
    {
        VerifyVpnStatusLabels(PROTECTED_STATUS_TITLE, null);

        return this;
    }

    public HomeRobot VerifyConnectionCardIsInInitalState()
    {
        VerifyConnectionCardLabels(CONNECTION_CARD_DEFAULT_HEADER, CONNECTION_CARD_DEFAULT_TITLE);
        Assert.IsNotNull(ConnectionCardConnectButton);

        return this;
    }

    public HomeRobot VerifyConnectionCardIsDisconnected(string countryCode = null, string cityState = null, int? serverNumber = null)
    {
        string expectedTitle = GetExpectedConnectionCardTitle(countryCode);
        string expectedSubtitle = GetExpectedConnectionCardSubtitle(cityState, serverNumber);

        VerifyConnectionCardLabels(CONNECTION_CARD_DISCONNECTED_HEADER, expectedTitle, expectedSubtitle);
        Assert.IsNotNull(ConnectionCardConnectButton);

        return this;
    }

    public HomeRobot VerifyConnectionCardIsConnecting(string countryCode = null, string cityState = null, int? serverNumber = null)
    {
        string expectedTitle = GetExpectedConnectionCardTitle(countryCode);
        string expectedSubtitle = GetExpectedConnectionCardSubtitle(cityState, serverNumber);

        VerifyConnectionCardLabels(CONNECTION_CARD_CONNECTING_HEADER, expectedTitle, expectedSubtitle);
        Assert.IsNotNull(ConnectionCardCancelButton);

        return this;
    }

    public HomeRobot VerifyConnectionCardIsConnected(string countryCode = null, string cityState = null, int? serverNumber = null)
    {
        string expectedTitle = GetExpectedConnectionCardTitle(countryCode);
        string expectedSubtitle = GetExpectedConnectionCardSubtitle(cityState, serverNumber);

        VerifyConnectionCardLabels(CONNECTION_CARD_CONNECTED_HEADER, expectedTitle, expectedSubtitle);
        Assert.IsNotNull(ConnectionCardDisconnectButton);

        return this;
    }

    public HomeRobot VerifyConnectionDetailsIsOpened()
    {
        Label connectionDetailsLabel = ConnectionDetailsTitleLabel;

        Assert.IsNotNull(connectionDetailsLabel);
        Assert.AreEqual("Connection details", connectionDetailsLabel.Text);

        Assert.IsNotNull(ConnectionDetailServerLoadButton);
        Assert.IsNotNull(ConnectionDetailLatencyButton);
        Assert.IsNotNull(ConnectionDetailProtocolButton);

        Assert.IsNotNull(ConnectionDetailsCloseButton);

        return this;
    }

    public HomeRobot VerifyUserIsLoggedIn(TestUserData user, string planName)
    {
        Assert.IsNotNull(ElementByName(user.Username));
        Assert.IsNotNull(ElementByName(planName));
        Assert.IsNotNull(SignOutButton);
        return this;
    }

    private void VerifyVpnStatusLabels(string expectedTitle = null, string expectedSubtitle = null)
    {
        if (!string.IsNullOrEmpty(expectedTitle))
        {
            WaitUntilTextMatches(() => VpnStatusTitleLabel, TestConstants.VeryShortTimeout, expectedTitle);
        }

        if (!string.IsNullOrEmpty(expectedSubtitle))
        {
            WaitUntilTextMatches(() => VpnStatusSubtitleLabel, TestConstants.VeryShortTimeout, expectedSubtitle);
        }
    }

    private void VerifyConnectionCardLabels(string expectedHeader = null, string expectedTitle = null, string expectedSubtitle = null)
    {
        if (!string.IsNullOrEmpty(expectedHeader))
        {
            WaitUntilTextMatches(() => ConnectionCardHeaderLabel, TestConstants.VeryShortTimeout, expectedHeader);
        }

        if (!string.IsNullOrEmpty(expectedTitle))
        {
            WaitUntilTextMatches(() => ConnectionCardTitleLabel, TestConstants.VeryShortTimeout, expectedTitle);
        }

        if (!string.IsNullOrEmpty(expectedSubtitle))
        {
            WaitUntilTextMatches(() => ConnectionCardSubtitleLabel, TestConstants.VeryShortTimeout, expectedSubtitle);
        }
    }

    private string GetExpectedConnectionCardTitle(string countryCode = null)
    {
        return string.IsNullOrEmpty(countryCode)
            ? CONNECTION_CARD_DEFAULT_TITLE
            : countryCode;
    }

    private string GetExpectedConnectionCardSubtitle(string cityState = null, int? serverNumber = null)
    {
        // Both city and server are null
        if (string.IsNullOrEmpty(cityState) && serverNumber == null)
        {
            return null;
        }

        // Only city is null
        if (string.IsNullOrEmpty(cityState))
        {
            return $"Server #{serverNumber}";
        }

        // Only server is null
        if (serverNumber == null)
        {
            return cityState;
        }

        // Both are set
        return $"{cityState} #{serverNumber}";
    }
}