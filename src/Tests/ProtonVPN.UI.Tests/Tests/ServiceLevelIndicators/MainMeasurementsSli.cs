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

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Annotations;
using ProtonVPN.UI.Tests.ApiClient.TestEnv;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.Robots.Countries;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.ServiceLevelIndicators;

[TestFixture]
[Category("SLI")]
[Workflow("main_measurements")]
public class MainMeasurementsSLI : TestSession
{
    private const string COUNTRY_NAME = "Germany";
    private const string COUNTRY_CODE = "DE";

    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();
    private ShellRobot _shellRobot = new();
    private CountriesRobot _countriesRobot = new();

    private LokiPusher _lokiPusher = new();
    private SliHelper _performanceTestHelper = new();

    [OneTimeSetUp]
    public void TestInitialize()
    {
        LaunchApp();
    }

    [Test, Order(0)]
    [Duration, TestStatus]
    [Sli("login")]
    public void LoginPerformance()
    {
        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.PlusUser);

        SliHelper.Measure(() =>
        {
            _homeRobot.DoCloseWelcomeOverlay();
        });
    }

    [Test, Order(1)]
    [Duration, TestStatus]
    [Sli("quick_connect")]
    public void QuickConnectPerformance()
    {
        //First connection is made to make sure that everything is setup
        _homeRobot
            .DoConnect()
            .VerifyAllStatesUntilConnected();
        _homeRobot.DoDisconnect()
            .Wait(TestConstants.TenSecondsTimeout);

        _homeRobot.DoConnect();
        SliHelper.Measure(() =>
        {
            _homeRobot.VerifyAllStatesUntilConnected();
        });
        _homeRobot.DoDisconnect();
    }

    [Test, Order(2)]
    [Sli("network_speed")]
    public void NetworkPerformance()
    {
        SliHelper.AddNetworkSpeedToMetrics("download_speed_disconnected", "upload_speed_disconnected");

        _shellRobot
          .DoNavigateToCountriesPage()
          .VerifyCurrentPage("Countries", false);

        _countriesRobot.SearchFor(COUNTRY_NAME)
            .DoConnect(COUNTRY_CODE);
        _homeRobot.VerifyVpnStatusIsConnected()
            .Wait(TestConstants.TenSecondsTimeout);

        SliHelper.AddNetworkSpeedToMetrics("download_speed_connected", "upload_speed_connected");

        _homeRobot.DoDisconnect()
            .VerifyVpnStatusIsDisconnected();
    }

    [Test, Order(3)]
    [Duration, TestStatus]
    [Sli("specific_server_connect")]
    public async Task ConnectionToSpecificServer()
    {
        string serverName = await _performanceTestHelper.GetRandomSpecificPaidServerAsync();

        _shellRobot
           .DoNavigateToCountriesPage()
           .VerifyCurrentPage("Countries", false);

        _countriesRobot.SearchFor(serverName)
            .DoConnect(serverName);

        SliHelper.Measure(() =>
        {
            _homeRobot.VerifyVpnStatusIsConnected();
        });

        _homeRobot.DoDisconnect();
    }

    [TearDown]
    public void TestCleanup()
    {
        _lokiPusher.PushMetrics();
        SliHelper.Reset();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Cleanup();
        _lokiPusher.PushAllLogs();
    }
}
