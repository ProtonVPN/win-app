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
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Settings;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.Performance;

[TestFixture]
[Category("SLI")]
public class SplitTunnelingPerformanceTest : TestSession
{
    private string _runId;
    private string _measurementGroup;
    private const string WORKFLOW = "split_tunneling_measurement";

    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();
    private ShellRobot _shellRobot = new();
    private SettingsRobot _settingsRobot = new();

    private LokiPusher _lokiPusher = new();


    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();
        _runId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    [Test]
    public void SplitTunnelingPerformance()
    {
        _measurementGroup = "split_tunneling_connect";

        _loginRobot.DoLogin(TestUserData.PlusUser);
        _homeRobot.DoWaitForVpnStatusSubtitleLabel()
            .DoConnect()
            .VerifyAllStatesUntilConnected();
        _homeRobot.DoDisconnect()
            .Wait(TestConstants.FiveSecondsTimeout);

        _shellRobot
            .DoNavigateToSplitTunnelingFeaturePage();
        _settingsRobot
            .EnableSplitTunneling()
            .ExcludeApp("Microsoft Edge")
            .ExcludeIp("212.102.35.236")
            .DoApplyChanges();

        _shellRobot.DoNavigateToHomePage();
        _homeRobot.DoConnect();
        PerformanceTestHelper.StartMonitoring();
        _homeRobot.VerifyAllStatesUntilConnected();
        PerformanceTestHelper.StopMonitoring();
        _homeRobot.DoDisconnect()
            //This helps to avoid race conditions when sending API calls.
            .Wait(TestConstants.FiveSecondsTimeout);

        PerformanceTestHelper.AddMetric("duration", PerformanceTestHelper.GetDuration);
    }

    [TearDown]
    public async Task TestCleanup()
    {
        Cleanup();
        PerformanceTestHelper.AddTestStatusMetric();
        await _lokiPusher.PushCollectedMetricsAsync(PerformanceTestHelper.MetricsList, _runId, _measurementGroup, WORKFLOW);
        PerformanceTestHelper.Reset();
        await _lokiPusher.PushAllLogsAsync(_runId, WORKFLOW);
    }
}
