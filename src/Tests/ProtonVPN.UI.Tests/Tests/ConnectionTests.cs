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

using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.Robots.Countries;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Overlays;
using ProtonVPN.UI.Tests.Robots.Shell;

namespace ProtonVPN.UI.Tests.Tests
{
    [TestFixture]
    [Category("UI")]
    public class ConnectionTests : TestSession
    {
        private ShellRobot _shellRobot = new ShellRobot();
        private HomeRobot _homeRobot = new HomeRobot();
        private CountriesRobot _countriesRobot = new CountriesRobot();
        private OverlaysRobot _overlaysRobot = new OverlaysRobot();

        private const string PAGE = "Countries";
        private const string COUNTRY = "Lithuania";
        private const string CITY = "Vilnius";
        private const string COUNTRY_CODE = "LT";
        private const int SERVER_NUMBER = 10;

        [SetUp]
        public void TestInitialize()
        {
            LaunchApp();

            _homeRobot
                .VerifyVpnStatusIsDisconnected()
                .VerifyConnectionCardIsInInitalState();
        }

        [Test]
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
        public void ConnectAndCancel()
        {
            _homeRobot
                .DoConnect()
                .VerifyVpnStatusIsConnecting()
                .VerifyConnectionCardIsConnecting()
                .Wait(500)
                .DoCancelConnection()
                .VerifyVpnStatusIsDisconnected()
                .VerifyConnectionCardIsDisconnected();
        }

        [Test]
        public void ConnectToSpecificCity()
        {
            _shellRobot
                .DoNavigateToCountriesPage()
                .VerifyCurrentPageName(PAGE);

            _countriesRobot
                .VerifyConnectionFormExists()
                .DoConnectTo(COUNTRY_CODE, CITY);

            _homeRobot
                .VerifyVpnStatusIsConnecting()
                .VerifyConnectionCardIsConnecting(COUNTRY, CITY)
                .VerifyVpnStatusIsConnected()
                .VerifyConnectionCardIsConnected(COUNTRY, CITY);

            _shellRobot
                .DoNavigateToCountriesPage()
                .VerifyCurrentPageName(PAGE);

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
                .VerifyOverlayIsOpened()
                .DoCloseOverlay();

            _homeRobot
                .DoOpenServerLoadOverlay();

            _overlaysRobot
                .VerifyOverlayIsOpened()
                .DoCloseOverlay();

            _homeRobot
                .DoOpenProtocolOverlay();

            _overlaysRobot
                .VerifyOverlayIsOpened()
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
}