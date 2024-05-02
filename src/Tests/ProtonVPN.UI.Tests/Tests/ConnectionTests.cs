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
using ProtonVPN.UI.Tests.Robots.Settings;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Tests;

[TestFixture]
[Category("UI")]
public class ConnectionTests : TestSession
{
    private const string COUNTRIES_PAGE_TITLE = "Countries";
    private const string COUNTRY = "Australia";
    private const string CITY = "Melbourne";
    private const string COUNTRY_CODE = "AU";

    private const string SERVER_LOAD_OVERLAY_TITLE = "What is server load?";
    private const string LATENCY_OVERLAY_TITLE = "What is latency?";
    private const string PROTOCOL_OVERLAY_TITLE = "What is a VPN protocol?";

    private const string PROTOCOL_PAGE_TITLE = "Protocol";
    private const string SMART_PROTOCOL = "Smart";

    private ShellRobot _shellRobot = new();
    private SettingsRobot _settingsRobot = new();
    private HomeRobot _homeRobot = new();
    private CountriesRobot _countriesRobot = new();
    private OverlaysRobot _overlaysRobot = new();
    private LoginRobot _loginRobot = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();

        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.PlusUser);

        _homeRobot
            .DoWaitForVpnStatusSubtitleLabel()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsInInitalState();
    }

    [Test]
    public void ConnectViaWireguard()
    {
        PerformProtocolConnectionTest(Protocol.Wireguard);
    }

    [Test]
    public void ConnectViaOpenVpnUdp()
    {
        PerformProtocolConnectionTest(Protocol.OpenVpnUdp);
    }

    [Test]
    public void ConnectViaOpenVpnTcp()
    {
        PerformProtocolConnectionTest(Protocol.OpenVpnTcp); 
    }

    [Test]
    public void ConnectAndDisconnect()
    {
        string unprotectedIpAddress = NetworkUtils.GetIpAddress();

        _homeRobot
            .DoConnect()
            .VerifyAllStatesUntilConnected();

        CommonAssertions.AssertIpAddressChanged(unprotectedIpAddress);

        _homeRobot
            .DoDisconnect()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsDisconnected()
            .Wait(TestConstants.DisconnectionDelay);

        CommonAssertions.AssertIpAddressUnchanged(unprotectedIpAddress);
    }

    [Retry(3)]
    [Test]
    public void ConnectAndCancel()
    {
        string unprotectedIpAddress = NetworkUtils.GetIpAddress();

        _homeRobot
            .DoConnect()
            .VerifyVpnStatusIsConnecting()
            .VerifyConnectionCardIsConnecting()
            //Imitate user's delay
            .Wait(500)
            .DoCancelConnection()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsDisconnected()
            .Wait(TestConstants.DisconnectionDelay);

        CommonAssertions.AssertIpAddressUnchanged(unprotectedIpAddress);
    }

    [Test]
    public void ConnectToSpecificCountry()
    {
        NavigateToCountriesPage();

        _countriesRobot
            .DoConnect(COUNTRY_CODE);
    
        _homeRobot
            .VerifyAllStatesUntilConnected(COUNTRY);

        NavigateToCountriesPage();

        _countriesRobot
            .VerifyActiveConnection(COUNTRY_CODE);
    }

    [Test]
    public void ConnectToSpecificCity()
    {
        NavigateToCountriesPage();

        _countriesRobot
            .DoNavigateToCountry(COUNTRY_CODE)
            .DoConnect(CITY);

        _homeRobot
            .VerifyAllStatesUntilConnected(COUNTRY, CITY);

        NavigateToCountriesPage();

        _countriesRobot
            .DoNavigateToCountry(COUNTRY_CODE);

        _countriesRobot
            .VerifyActiveConnection(CITY);
    }

    [Test]
    public void ConnectToSpecificServer()
    {
        NavigateToServers(COUNTRY_CODE, CITY);

        ServerConnectButton serverConnectButton = _countriesRobot.GetServerConnectButton();

        _countriesRobot
            .DoConnect(serverConnectButton.Name);

        _homeRobot
            .VerifyAllStatesUntilConnected(COUNTRY, CITY, serverConnectButton.Number);

        NavigateToServers(COUNTRY_CODE, CITY);

        _countriesRobot
            .VerifyActiveConnection(serverConnectButton.Name);
    }

    [Test]
    public void OpenConnectionDetails()
    {
        _homeRobot
            .DoConnect()
            .VerifyAllStatesUntilConnected()
            .DoOpenConnectionDetails()
            .VerifyConnectionDetailsIsOpened();

        // VPNWIN-2095 - Latency is kept hidden as it is not implemented yet. 
        //_homeRobot
        //    .DoOpenLatencyOverlay();

        //_overlaysRobot
        //    .VerifyOverlayIsOpened(LATENCY_OVERLAY_TITLE, true)
        //    .DoCloseOverlay();

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

    [Test]
    public void NavigateToProtocolFromOverlay()
    {
        _homeRobot
            .DoConnect()
            .VerifyAllStatesUntilConnected()
            .DoOpenConnectionDetails()
            .VerifyConnectionDetailsIsOpened();

        _homeRobot
            .DoOpenProtocolOverlay();

        _overlaysRobot
            .VerifyOverlayIsOpened(PROTOCOL_OVERLAY_TITLE, true)
            .VerifyProtocolOverlaySettingsCard(SMART_PROTOCOL)
            .DoSwitchProtocol();

        _shellRobot
            .VerifyCurrentPage(PROTOCOL_PAGE_TITLE, true);
    }


    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }

    private void NavigateToServers(string countryCode, string city)
    {
        NavigateToCountriesPage();

        _countriesRobot
            .DoNavigateToCountry(countryCode)
            .DoShowServers(city);
    }

    private void NavigateToCountriesPage()
    {
        _shellRobot
            .DoNavigateToCountriesPage()
            .VerifyCurrentPage(COUNTRIES_PAGE_TITLE, false);
    }

    private void PerformProtocolConnectionTest(Protocol protocol)
    {
        _shellRobot
            .DoNavigateToSettingsPage();
        _settingsRobot
            .DoNavigateToProtocolSettingsPage()
            .DoSelectProtocol(protocol)
            .DoApplyChanges();
        _shellRobot
            .DoNavigateToHomePage();

        _homeRobot
            .DoConnect()
            .VerifyAllStatesUntilConnected();
    }
}