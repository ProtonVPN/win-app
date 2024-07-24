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
[Workflow("wireguard_tls_measurement")]
public class StealthSli: TestSession
{
    private const string PROTOCOL = "Stealth";

    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();
    private ShellRobot _shellRobot = new();
    private SettingsRobot _settingsRobot = new();

    private LokiPusher _lokiPusher = new();


    [OneTimeSetUp]
    public void TestInitialize()
    {
        LaunchApp();
        _loginRobot.DoLogin(TestUserData.PlusUser);
        _homeRobot.DoCloseWelcomeOverlay();
    }

    [Test, Order(0)]
    [Duration]
    [Sli("wireguard_tls")]
    public void WireguardTlsConnectionSpeed()
    {
        _shellRobot
            .DoNavigateToSettingsPage();
        _settingsRobot
            .DoNavigateToProtocolSettingsPage()
            .DoSelectProtocol(TestConstants.Protocol.WireGuardTls)
            .DoApplyChanges();
        _shellRobot
            .DoNavigateToHomePage();

        _homeRobot.DoConnect()
            .VerifyVpnStatusIsConnected()
            .DoDisconnect()
            //Imitate user's delay
            .Wait(TestConstants.TenSecondsTimeout);
        _homeRobot.DoConnect();

        SliHelper.Measure(() =>
        {
            _homeRobot.VerifyVpnStatusIsConnected();
        });
    }

    [Test, Order(1)]
    [TestStatus]
    [Sli("wireguard_tls")]
    public void WireguardTlsStability()
    {
        SliHelper.Measure(() =>
        {
            _homeRobot.VerifyProtocolExist(PROTOCOL);
        });
        _homeRobot.DoDisconnect();
    }

    [OneTimeTearDown]
    public void TestCleanup()
    {
        _lokiPusher.PushMetrics();
        Cleanup();
        SliHelper.Reset();
        _lokiPusher.PushAllLogs();
    }
}
