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
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using ProtonVPN.UI.Tests.ApiClient;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.Robots.Countries;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Settings;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.Performance;

[TestFixture]
[Category("Performance")]
public class PerformanceTests : TestSession
{
    private string _runId;
    private string _measurementGroup;

    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();
    private ShellRobot _shellRobot = new();
    private CountriesRobot _countriesRobot = new();
    private SettingsRobot _settingsRobot = new();

    private LokiApiClient _lokiHelper = new();
    private PerformanceTestHelper _performanceTestHelper = new();


    [OneTimeSetUp]
    public void TestInitialize()
    {
        LaunchApp();
        _runId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    [Test, Order(0)]
    public async Task LoginPerformance()
    {
        _measurementGroup = "login";

        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.PlusUser);

        PerformanceTestHelper.StartMonitoring();
        _homeRobot.DoWaitForVpnStatusSubtitleLabel();
        PerformanceTestHelper.StopMonitoring();

        PerformanceTestHelper.AddMetric("duration", PerformanceTestHelper.GetDuration);
    }

    [Test, Order(1)]
    public async Task QuickConnectPerformance()
    {
        _measurementGroup = "quick_connect";

        //First connection is made to make sure that everything is setup
        _homeRobot
            .DoConnect()
            .VerifyAllStatesUntilConnected();
        _homeRobot.DoDisconnect()
            .Wait(TestConstants.VeryShortTimeout);

        _homeRobot.DoConnect();
        PerformanceTestHelper.StartMonitoring();
        _homeRobot.VerifyAllStatesUntilConnected();
        PerformanceTestHelper.StopMonitoring();
        _homeRobot.DoDisconnect()
            //This helps to avoid race conditions when sending API calls.
            .Wait(TestConstants.VeryShortTimeout);

        PerformanceTestHelper.AddMetric("duration", PerformanceTestHelper.GetDuration);
    }

    [Test, Order(2)]
    public async Task NetworkPerformance()
    {
        _measurementGroup = "network_speed";

        PerformanceTestHelper.AddNetworkSpeedToMetrics("download_speed_disconnected", "upload_speed_disconnected");

        _shellRobot
          .DoNavigateToCountriesPage()
          .VerifyCurrentPage("Countries", false);

        _countriesRobot.SearchFor("France")
            .DoConnect("FR");
        _homeRobot.VerifyVpnStatusIsConnected()
            .Wait(TestConstants.VeryShortTimeout);

        PerformanceTestHelper.AddNetworkSpeedToMetrics("download_speed_connected", "upload_speed_connected");

        _homeRobot.DoDisconnect()
            .VerifyVpnStatusIsDisconnected()
            .Wait(TestConstants.VeryShortTimeout);
    }

    [Test, Order(3)]
    public async Task ConnectionToSpecificServer()
    {
        _measurementGroup = "specific_server_connect";
        string serverName = await _performanceTestHelper.GetRandomSpecificPaidServerAsync();

        _shellRobot
           .DoNavigateToCountriesPage()
           .VerifyCurrentPage("Countries", false);

        _countriesRobot.SearchFor(serverName);
        PerformanceTestHelper.StartMonitoring();
        _countriesRobot.DoConnect(serverName);
        _homeRobot.VerifyVpnStatusIsConnected();
        PerformanceTestHelper.StopMonitoring();
        _homeRobot.DoDisconnect()
            .Wait(TestConstants.VeryShortTimeout);
    }

    [TearDown]
    public async Task TestCleanup()
    {
        PerformanceTestHelper.AddTestStatusMetric();
        await _lokiHelper.PushCollectedMetricsAsync(PerformanceTestHelper.MetricsList, _runId, _measurementGroup);
        PerformanceTestHelper.Reset();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        Cleanup();
        await _lokiHelper.PushLogsAsync(TestConstants.ClientLogsPath, _runId, "windows_client_logs");
        await _lokiHelper.PushLogsAsync(GetServiceLogsPath(), _runId, "windows_service_logs");
    }
}
