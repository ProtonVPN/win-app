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
using ProtonVPN.UI.Tests.ApiClient.TestEnv;
using ProtonVPN.UI.Tests.Robots.Countries;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.SLI;

[TestFixture]
public class BtiSLI : TestSession
{
    private const string SERVER = "CI-NL#01";

    private string _measurementGroup;
    private string _workflow;
    private string _runId;

    private LokiPusher _lokiPusher = new();
    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();
    private CountriesRobot _countriesRobot = new();
    private ShellRobot _shellRobot = new();

    private AtlasApiClient _atlasApiClient = new();
    

    [OneTimeSetUp]
    public async Task TestInitialize()
    {
        await ResetEnvironmentAsync();
        _runId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    [Test]
    [Category("SLI-BTI-PROD")]
    public async Task AlternativeRoutingSli()
    {
        _workflow = "alternative_routing";
        _measurementGroup = "alt_routing_login";

        BtiController.SetScenarioAsync("enable/block_vpn_prod_api_endpoint");

        LaunchApp();

        _loginRobot.DoLogin(TestUserData.PlusUser);
        PerformanceTestHelper.StartMonitoring();
        _homeRobot.DoCloseWelcomeOverlay();
        PerformanceTestHelper.StopMonitoring();

        PerformanceTestHelper.AddMetric("duration", PerformanceTestHelper.GetDuration);
    }

    [Test]
    [Category("SLI-BTI")]
    public async Task ReconnectionSli()
    {
        await _atlasApiClient.MockApiAsync(Scenarios.MAINTENANCE_ONE_MINUTE);

        _workflow = "reconnection";
        _measurementGroup = "reconnection_success";

        LaunchApp();

        _loginRobot.DoLogin(TestUserData.PlusUserBti);
        _homeRobot.DoCloseWelcomeOverlay();
        _shellRobot.DoNavigateToCountriesPage();
        _countriesRobot
            .SearchFor(SERVER)
            .DoConnect(SERVER);
        _homeRobot.VerifyVpnStatusIsConnected();

        PerformanceTestHelper.StartMonitoring();
        string currentIpAddress = NetworkUtils.GetIpAddressBti();
        
        BtiController.SetScenarioAsync(Scenarios.PUT_NL_1_IN_MAINTENANCE);
        _homeRobot.VerifyAllStatesUntilConnected();

        string newIpAddress = NetworkUtils.GetIpAddressBti();
        Assert.AreNotEqual(currentIpAddress, newIpAddress, $"Failed to reconnect user to new server. " +
            $"Old IP: ${currentIpAddress}. New IP: ${newIpAddress}");

        PerformanceTestHelper.StopMonitoring();
    }

    [TearDown]
    public async Task TestCleanup()
    {
        PerformanceTestHelper.AddTestStatusMetric();
        await _lokiPusher.PushCollectedMetricsAsync(PerformanceTestHelper.MetricsList, _runId, _measurementGroup, _workflow);
        await ResetEnvironmentAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        Cleanup();
        await _lokiPusher.PushAllLogsAsync(_runId, _workflow);
    }

    private async Task ResetEnvironmentAsync()
    {
        PerformanceTestHelper.Reset();
        await _atlasApiClient.MockApiAsync("");
        BtiController.SetScenarioAsync("reset");
    }
}
