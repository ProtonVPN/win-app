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
using ProtonVPN.UI.Tests.Annotations;
using ProtonVPN.UI.Tests.ApiClient.TestEnv;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Settings;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.ServiceLevelIndicators;

[TestFixture]
[Category("SLI")]
[Workflow("split_tunneling_measurement")]
public class SplitTunnelingSli : TestSession
{
    private const string BROWSER = "Microsoft Edge";
    private const string IP_ME = "212.102.35.236";

    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();
    private ShellRobot _shellRobot = new();
    private SettingsRobot _settingsRobot = new();

    private LokiPusher _lokiPusher = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();
        _loginRobot.DoLogin(TestUserData.PlusUser);
        _homeRobot.DoCloseWelcomeOverlay();
    }

    [Test]
    [Duration]
    [Sli("split_tunneling_connect")]
    public void SplitTunnelingPerformance()
    {
        _homeRobot
            .DoConnect()
            .VerifyAllStatesUntilConnected();
        _homeRobot.DoDisconnect()
            //Imitate User's delay
            .Wait(TestConstants.FiveSecondsTimeout);

        _shellRobot
            .DoNavigateToSplitTunnelingFeaturePage();
        _settingsRobot
            .EnableSplitTunneling()
            .ExcludeApp(BROWSER)
            .ExcludeIp(IP_ME)
            .DoApplyChanges();

        _shellRobot.DoNavigateToHomePage();
        _homeRobot.DoConnect();
        SliHelper.Measure(() =>
        {
            _homeRobot.VerifyAllStatesUntilConnected();
        });
        _homeRobot.DoDisconnect();
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
        _lokiPusher.PushMetrics();
        SliHelper.Reset();
        _lokiPusher.PushAllLogs();
    }
}
