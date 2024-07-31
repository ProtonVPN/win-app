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

using System.Threading.Tasks;
using NUnit.Framework;
using ProtonVPN.UI.Tests.ApiClient.TestEnv;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Annotations;

namespace ProtonVPN.UI.Tests.Tests.ServiceLevelIndicators;

[TestFixture]
[Category("SLI-BTI-PROD")]
[Workflow("alternative_routing")]
public class AlternativeRoutingSli : TestSession
{
    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();

    private LokiPusher _lokiPusher = new();
    private AtlasApiClient _atlasApiClient = new();

    [OneTimeSetUp]
    public void TestInitialize()
    {
        BtiController.SetScenario(Scenarios.RESET);
        BtiController.SetScenario(Scenarios.BLOCK_PROD_ENDPOINT);

        LaunchApp();
    }

    [Test]
    [Duration, TestStatus]
    [Sli("alt_routing_login")]
    public void AlternativeRoutingSliMeasurement()
    {
        _loginRobot.DoLogin(TestUserData.PlusUser);
        SliHelper.Measure(() =>
        {
            _homeRobot.DoCloseWelcomeOverlay();
        });
    }

    [TearDown]
    public async Task TestCleanup()
    {
        Cleanup();
        _lokiPusher.PushMetrics();
        _lokiPusher.PushAllLogs();
        SliHelper.Reset();
        BtiController.SetScenario(Scenarios.RESET);
    }
}
