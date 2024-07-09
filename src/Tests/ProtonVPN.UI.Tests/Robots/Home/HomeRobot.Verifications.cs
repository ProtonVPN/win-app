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

using System.Net.NetworkInformation;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots.Home;

public partial class HomeRobot
{
    private const string UNPROTECTED_STATUS_SUBTITLE = "You are unprotected";
    private const string CONNECTING_STATUS_SUBTITLE = "Protecting your digital identity";
    private const string PROTECTED_STATUS_TITLE = "Protected";

    private const string CONNECTION_CARD_DISCONNECTED_HEADER = "Default connection";
    private const string CONNECTION_CARD_CONNECTING_HEADER = "Connecting to...";
    private const string CONNECTION_CARD_CONNECTED_HEADER = "Browsing safely from";
    private const string CONNECTION_CARD_DEFAULT_FREE_HEADER = "Free connection";

    private const string CONNECTION_CARD_DEFAULT_TITLE = "Fastest country";
    private const string CONNECTION_CARD_DEFAULT_FREE_TITLE = "Fastest free server";

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

    public HomeRobot VerifyConnectionCardIsInInitalStateForFreeUser()
    {
        VerifyConnectionCardLabels(CONNECTION_CARD_DEFAULT_FREE_HEADER, CONNECTION_CARD_DEFAULT_FREE_TITLE);
        Assert.IsNotNull(ConnectionCardConnectButton);

        return this;
    }

    public HomeRobot VerifyConnectionCardIsInInitalState()
    {
        VerifyConnectionCardLabels(CONNECTION_CARD_DISCONNECTED_HEADER, CONNECTION_CARD_DEFAULT_TITLE);
        Assert.IsNotNull(ConnectionCardConnectButton);

        return this;
    }

    public HomeRobot VerifyConnectionCardIsDisconnected(string countryCode = null, string stateOrCity = null, int? serverNumber = null)
    {
        string expectedTitle = GetExpectedConnectionCardTitle(countryCode);
        string expectedSubtitle = GetExpectedConnectionCardSubtitle(stateOrCity, serverNumber);

        VerifyConnectionCardLabels(CONNECTION_CARD_DISCONNECTED_HEADER, expectedTitle, expectedSubtitle);
        Assert.IsNotNull(ConnectionCardConnectButton);

        return this;
    }

    public HomeRobot VerifyConnectionCardIsConnecting(string countryCode = null, string stateOrCity = null, int? serverNumber = null)
    {
        string expectedTitle = GetExpectedConnectionCardTitle(countryCode);
        string expectedSubtitle = GetExpectedConnectionCardSubtitle(stateOrCity, serverNumber);

        VerifyConnectionCardLabels(CONNECTION_CARD_CONNECTING_HEADER, expectedTitle, expectedSubtitle);
        Assert.IsNotNull(ConnectionCardCancelButton);

        return this;
    }

    public HomeRobot VerifyConnectionCardIsConnectingToFreeServer()
    {
        VerifyConnectionCardLabels(CONNECTION_CARD_CONNECTING_HEADER, CONNECTION_CARD_DEFAULT_FREE_TITLE);
        Assert.IsNotNull(ConnectionCardCancelButton);

        return this;
    }

    public HomeRobot VerifyConnectionCardIsConnected(string countryCode = null, string stateOrCity = null, int? serverNumber = null)
    {
        string expectedTitle = GetExpectedConnectionCardTitle(countryCode);
        string expectedSubtitle = GetExpectedConnectionCardSubtitle(stateOrCity, serverNumber);

        VerifyConnectionCardLabels(CONNECTION_CARD_CONNECTED_HEADER, expectedTitle, expectedSubtitle);
        Assert.IsNotNull(ConnectionCardDisconnectButton);

        return this;
    }

    public HomeRobot VerifyConnectionCardIsConnectedToFreeServer()
    {
        VerifyConnectionCardLabels(CONNECTION_CARD_CONNECTED_HEADER, CONNECTION_CARD_DEFAULT_FREE_TITLE);
        Assert.IsNotNull(ConnectionCardChangeServerButton);
        Assert.IsNotNull(ConnectionCardDisconnectButton);

        return this;
    }

    public HomeRobot VerifyConnectionDetailsIsOpened()
    {
        Label connectionDetailsLabel = ConnectionDetailsTitleLabel;

        Assert.IsNotNull(connectionDetailsLabel);
        Assert.AreEqual("Connection details", connectionDetailsLabel.Text);

        Assert.IsNotNull(ConnectionDetailServerLoadButton);
        //Assert.IsNotNull(ConnectionDetailLatencyButton); // Latency is kept hidden in the connection details panel as it is not implemented yet
        Assert.IsNotNull(ConnectionDetailProtocolButton);

        Assert.IsNotNull(ConnectionDetailsCloseButton);

        return this;
    }

    public HomeRobot VerifyAllStatesUntilConnected(string countryCode = null, string stateOrCity = null, int? serverNumber = null)
    {
        VerifyVpnStatusIsConnecting();
        VerifyConnectionCardIsConnecting(countryCode, stateOrCity, serverNumber);
        VerifyVpnStatusIsConnected();
        VerifyConnectionCardIsConnected(countryCode, stateOrCity, serverNumber);
        return this;
    }

    public HomeRobot VerifyAllStatesUntilConnectedToFreeServer()
    {
        VerifyVpnStatusIsConnecting();
        VerifyConnectionCardIsConnectingToFreeServer();
        VerifyVpnStatusIsConnected();
        VerifyConnectionCardIsConnectedToFreeServer();
        return this;
    }

    public HomeRobot VerifyRecentsTabIsDisplayed()
    {
        Assert.IsNotNull(RecentsTab);
        return this;
    }

    public HomeRobot VerifyCountryIsInRecentsList(string country)
    {
        Assert.IsNotNull(GetRecentRow(country));
        return this;
    }

    public HomeRobot VerifyRecentsDoesNotExist()
    {
        Assert.IsNull(Window.FindFirstDescendant(cf => cf.ByName("Recents")));
        return this;
    }

    public HomeRobot VerifyErrorMessageExists(string errorMessage)
    {
        WaitUntilElementExistsByName(errorMessage, TestConstants.FiveSecondsTimeout);
        return this;
    }

    public HomeRobot VerifyUserIsNotReconnected()
    {
        RetryResult<AutomationElement> retry = Retry.WhileNull(
        () =>
        {
            return FindFirstDescendantUsingChildren(cf => cf.ByName(CONNECTION_CARD_CONNECTING_HEADER));
        },
        TestConstants.TenSecondsTimeout, TestConstants.RetryInterval);

        Assert.IsFalse(retry.Success);
        return this;
    }

    public HomeRobot VerifyProtocolExist(string protocolName)
    {
        WaitUntilElementExistsByName(protocolName, TestConstants.FiveSecondsTimeout);
        return this;
    }

    public HomeRobot VerifyNotTheCountryWantedBannerIsDisplayed()
    {
        Assert.IsFalse(NotTheCountryWantedTitle.IsOffscreen);
        Assert.IsFalse(UpgradeButton.IsOffscreen);
        Assert.IsFalse(UpgradeToChooseServerLabel.IsOffscreen);
        return this;
    }

    public HomeRobot VerifyNetshieldUpsellBannerDisplayed()
    {
        Assert.IsFalse(UpgradeButton.IsOffscreen);
        Assert.IsFalse(BlockAdsLabel.IsOffscreen);
        Assert.IsFalse(NetshieldLabel.IsOffscreen);
        return this;
    }

    public HomeRobot VerifyIfLocalNetworkingWorks(string localIpAddress)
    {
        PingReply reply = new Ping().Send(localIpAddress);
        Assert.That(reply.Status == IPStatus.Success, Is.True);
        return this;
    }

    private void VerifyVpnStatusLabels(string expectedTitle = null, string expectedSubtitle = null)
    {
        if (!string.IsNullOrEmpty(expectedTitle))
        {
            WaitUntilTextMatches(() => VpnStatusTitleLabel, TestConstants.ThirtySecondsTimeout, expectedTitle);
        }

        if (!string.IsNullOrEmpty(expectedSubtitle))
        {
            WaitUntilTextMatches(() => VpnStatusSubtitleLabel, TestConstants.ThirtySecondsTimeout, expectedSubtitle);
        }
    }

    private void VerifyConnectionCardLabels(string expectedHeader = null, string expectedTitle = null, string expectedSubtitle = null)
    {
        if (!string.IsNullOrEmpty(expectedHeader))
        {
            WaitUntilTextMatches(() => ConnectionCardHeaderLabel, TestConstants.FiveSecondsTimeout, expectedHeader);
        }

        if (!string.IsNullOrEmpty(expectedTitle))
        {
            WaitUntilTextMatches(() => ConnectionCardTitleLabel, TestConstants.FiveSecondsTimeout, expectedTitle);
        }

        if (!string.IsNullOrEmpty(expectedSubtitle))
        {
            WaitUntilTextMatches(() => ConnectionCardSubtitleLabel, TestConstants.FiveSecondsTimeout, expectedSubtitle);
        }
    }

    private string GetExpectedConnectionCardTitle(string countryCode = null)
    {
        return string.IsNullOrEmpty(countryCode)
            ? CONNECTION_CARD_DEFAULT_TITLE
            : countryCode;
    }

    private string GetExpectedConnectionCardSubtitle(string stateOrCity = null, int? serverNumber = null)
    {
        // Both city and server are null
        if (string.IsNullOrEmpty(stateOrCity) && serverNumber == null)
        {
            return null;
        }

        // Only city is null
        if (string.IsNullOrEmpty(stateOrCity))
        {
            return $"Server #{serverNumber}";
        }

        // Only server is null
        if (serverNumber == null)
        {
            return stateOrCity;
        }

        // Both are set
        return $"{stateOrCity} #{serverNumber}";
    }
}