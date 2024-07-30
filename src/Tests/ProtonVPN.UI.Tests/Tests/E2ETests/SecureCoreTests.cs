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
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
public class SecureCoreTests : TestSession
{
    private const string COUNTRY = "Australia";
    private const string ENTRY_COUNTRY = "Switzerland";
    private const string COUNTRY_CODE = "AU";
    private const string ENTRY_COUNTRY_CODE = "CH";

    private ShellRobot _shellRobot = new();
    private HomeRobot _homeRobot = new();
    private CountriesRobot _countriesRobot = new();
    private LoginRobot _loginRobot = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();

        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.PlusUser);

        _homeRobot
            .DoCloseWelcomeOverlay()
            .DoWaitForVpnStatusSubtitleLabel()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsInInitalState();

        _shellRobot
            .DoNavigateToSecureCoreCountriesPage();
    }

    [Test]
    public void SecureCoreViaCountry()
    {
        _countriesRobot
            .DoConnect(COUNTRY_CODE);

        _homeRobot
            .VerifyVpnStatusIsConnecting()
            .VerifyConnectionCardIsConnecting(COUNTRY)
            .VerifyVpnStatusIsConnected()
            .VerifyConnectionCardIsConnected(COUNTRY);

        _shellRobot
            .DoNavigateToSecureCoreCountriesPage();

        _countriesRobot
            .Wait(TestConstants.DefaultAnimationDelay)
            .VerifyActiveConnection(COUNTRY_CODE);
    }

    [Test]
    public void SecureCoreViaSpecficEntry()
    {
        _countriesRobot
            .DoNavigateToCountry(COUNTRY_CODE)
            .DoConnectSecureCore(ENTRY_COUNTRY_CODE, COUNTRY_CODE);

        _homeRobot
            .VerifyVpnStatusIsConnecting()
            .VerifyConnectionCardIsConnecting(COUNTRY, $"via {ENTRY_COUNTRY}")
            .VerifyVpnStatusIsConnected()
            .VerifyConnectionCardIsConnected(COUNTRY, $"via {ENTRY_COUNTRY}");

        _shellRobot
            .DoNavigateToSecureCoreCountriesPage();

        _countriesRobot
            .VerifyActiveConnection(COUNTRY_CODE)
            .DoNavigateToCountry(COUNTRY_CODE)
            .VerifyActiveConnection(ENTRY_COUNTRY_CODE, COUNTRY_CODE);
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}
