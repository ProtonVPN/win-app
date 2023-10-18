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
using ProtonVPN.UI.Tests.Robots.Countries;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Overlays;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests;

[TestFixture]
[Category("UI")]
public class ConnectionTests : TestSession
{
    private const string COUNTRIES_PAGE_TITLE = "Countries";
    private const string COUNTRY = "Lithuania";
    private const string CITY = "Vilnius";
    private const string COUNTRY_CODE = "LT";
    private const int SERVER_NUMBER = 10;
    private const string SERVER_LOAD_OVERLAY_TITLE = "What is server load?";
    private const string LATENCY_OVERLAY_TITLE = "What is latency?";
    private const string PROTOCOL_OVERLAY_TITLE = "What is a VPN protocol?";

    private ShellRobot _shellRobot = new();
    private HomeRobot _homeRobot = new();
    private CountriesRobot _countriesRobot = new();
    private OverlaysRobot _overlaysRobot = new();
    private LoginRobot _loginRobot = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();

        _loginRobot
            .Wait(TestConstants.InitializationDelay)
            .DoLogin(TestUserData.PlusUser);

        _homeRobot
            .DoWaitForVpnStatusSubtitleLabel()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsInInitalState();
    }

    [Test]
    [Retry(3)]
    //TODO When reconnection logic is implemented remove retry
    public void Connect()
    {
        _homeRobot
            .DoConnect()
            .VerifyVpnStatusIsConnecting()
            .VerifyConnectionCardIsConnecting()
            .VerifyVpnStatusIsConnected()
            .VerifyConnectionCardIsConnected();
    }

    [Test]
    [Retry(3)]
    //TODO When reconnection logic is implemented remove retry
    public void ConnectAndDisconnect()
    {
        _homeRobot
            .DoConnect()
            .VerifyVpnStatusIsConnecting()
            .VerifyConnectionCardIsConnecting()
            .VerifyVpnStatusIsConnected()
            .VerifyConnectionCardIsConnected()
            .DoDisconnect()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsDisconnected();
    }

    [Test]
    [Retry(3)]
    //TODO When reconnection logic is implemented remove retry
    public void ConnectAndCancel()
    {
        _homeRobot
            .DoConnect()
            .VerifyVpnStatusIsConnecting()
            .VerifyConnectionCardIsConnecting()
            .DoCancelConnection()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsDisconnected();
    }

    [Test]
    public void ConnectToSpecificCity()
    {
        _shellRobot
            .DoNavigateToCountriesPage()
            .VerifyCurrentPage(COUNTRIES_PAGE_TITLE, false);

        _countriesRobot
            .VerifyConnectionFormExists()
            .DoConnectTo(COUNTRY_CODE, CITY);

        _homeRobot
            .VerifyVpnStatusIsConnecting()
            .VerifyConnectionCardIsConnecting(COUNTRY, CITY)
            .VerifyVpnStatusIsConnected()
            .VerifyConnectionCardIsConnected(COUNTRY, CITY);
    }

    [Test]
    public void ConnectToSpecificServer()
    {
        _shellRobot
            .DoNavigateToCountriesPage()
            .VerifyCurrentPage(COUNTRIES_PAGE_TITLE, false);

        _countriesRobot
            .VerifyConnectionFormExists()
            .DoConnectTo(COUNTRY_CODE, CITY, SERVER_NUMBER);

        _homeRobot
            .VerifyVpnStatusIsConnecting()
            .VerifyConnectionCardIsConnecting(COUNTRY, CITY, SERVER_NUMBER)
            .VerifyVpnStatusIsConnected()
            .VerifyConnectionCardIsConnected(COUNTRY, CITY, SERVER_NUMBER);
    }

    [Test]
    [Retry(3)]
    //TODO When reconnection logic is implemented remove retry
    public void OpenConnectionDetails()
    {
        _homeRobot
            .DoConnect()
            .VerifyVpnStatusIsConnecting()
            .VerifyConnectionCardIsConnecting()
            .VerifyVpnStatusIsConnected()
            .VerifyConnectionCardIsConnected()
            .DoOpenConnectionDetails()
            .VerifyConnectionDetailsIsOpened();

        _homeRobot
            .DoOpenLatencyOverlay();

        _overlaysRobot
            .VerifyOverlayIsOpened(LATENCY_OVERLAY_TITLE, true)
            .DoCloseOverlay();

        _homeRobot
            .DoOpenServerLoadOverlay();

        _overlaysRobot
            .VerifyOverlayIsOpened(SERVER_LOAD_OVERLAY_TITLE, true)
            .DoCloseOverlay();

        _homeRobot
            .DoOpenProtocolOverlay();

        _overlaysRobot
            .VerifyOverlayIsOpened(PROTOCOL_OVERLAY_TITLE, true)
            .DoCloseOverlay();

        _homeRobot
            .DoCloseConnectionDetails();
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}