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
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests;

[TestFixture]
[Category("UI")]
public class RecentsTests : TestSession
{
    private ShellRobot _shellRobot = new();
    private HomeRobot _homeRobot = new();
    private CountriesRobot _countriesRobot = new();
    private LoginRobot _loginRobot = new();

    private const string FIRST_COUNTRY_CODE = "AR";
    private const string FIRST_COUNTRY_NAME = "Argentina";
    private const string SECOND_COUNTRY_CODE = "AU";
    private const string SECOND_COUNTRY_NAME = "Australia";

    [OneTimeSetUp]
    public void OneTimeSetup()
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

    [Test, Order(0)]
    public void RecentIsAddedToTheList()
    {
        _homeRobot
            .VerifyRecentsDoesNotExist();

        _shellRobot
            .DoNavigateToCountriesPage();

        _countriesRobot
            .DoConnect(FIRST_COUNTRY_CODE);
        _homeRobot
            .VerifyVpnStatusIsConnected();

        _shellRobot
            .DoNavigateToCountriesPage();

        _countriesRobot
            .DoConnect(SECOND_COUNTRY_CODE);
        _homeRobot
            .VerifyVpnStatusIsConnected()
            .VerifyRecentsTabIsDisplayed();

        _homeRobot
            .DoClickOnRecentsTab()
            .VerifyCountryIsInRecentsList(FIRST_COUNTRY_NAME);
    }

    [Test, Order(1)]
    public void ConnectionToRecent()
    {
        _homeRobot
            .DoClickOnRecentCountry(FIRST_COUNTRY_NAME)
            .VerifyVpnStatusIsConnected()
            .DoDisconnect()
            .VerifyVpnStatusIsDisconnected();
    }

    [Test, Order(2)]
    public void RemoveRecent()
    {
        _homeRobot
            .RemoveRecent(FIRST_COUNTRY_NAME)
            .RemoveRecent(SECOND_COUNTRY_NAME)
            .VerifyRecentsDoesNotExist();
    }

    [TearDown]
    public void SaveArtifacts()
    {
        SaveScreenshotAndLogsIfFailed();
    }

    [OneTimeTearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}
