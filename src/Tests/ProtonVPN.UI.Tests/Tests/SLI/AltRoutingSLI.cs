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
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.SLI;

[TestFixture]
[Category("SLI-BTI-PROD")]
public class AltRoutingSLI : TestSession
{
    private const string WORKFLOW = "alternative_routing";
    private string _runId;
    private LokiPusher _lokiPusher = new();
    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();
    private string _measurementGroup;

    [OneTimeSetUp]
    public async Task TestInitialize()
    {
        _runId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    [Test]
    public async Task AlternativeRoutingSli()
    {
        _measurementGroup = "alt_routing_login";

        BtiController.SetScenarioAsync("reset");
        BtiController.SetScenarioAsync("enable/block_vpn_prod_api_endpoint");

        LaunchApp();

        _loginRobot.DoLogin(TestUserData.PlusUser);
        PerformanceTestHelper.StartMonitoring();
        _homeRobot.DoCloseWelcomeOverlay();
        PerformanceTestHelper.StopMonitoring();

        PerformanceTestHelper.AddMetric("duration", PerformanceTestHelper.GetDuration);
    }

    [TearDown]
    public async Task TestCleanup()
    {
        PerformanceTestHelper.AddTestStatusMetric();
        await _lokiPusher.PushCollectedMetricsAsync(PerformanceTestHelper.MetricsList, _runId, _measurementGroup, WORKFLOW);
        PerformanceTestHelper.Reset();
        BtiController.SetScenarioAsync("reset");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        Cleanup();
        await _lokiPusher.PushAllLogsAsync(_runId, WORKFLOW);
    }
}
