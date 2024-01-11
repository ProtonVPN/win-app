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
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Settings;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;
using System.Threading;

namespace ProtonVPN.UI.Tests.Tests;

[TestFixture]
[Category("UI")]
public class NetshieldTests : TestSession
{
    private ShellRobot _shellRobot = new();
    private SettingsRobot _settingsRobot = new();
    private HomeRobot _homeRobot = new();
    private LoginRobot _loginRobot = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();

        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.PlusUser);

        _homeRobot
            .DoWaitForVpnStatusSubtitleLabel()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsInInitalState();

        //TODO When reconnection logic is implemented remove this sleep.
        //Certificate sometimes takes longer to get and app does not handle it yet
        _shellRobot
            .Wait(TestConstants.InitializationDelay);
    }

    [Test]
    //TODO Change this test case to disabling it while connected, when LA logic is implemented
    public void NetshieldOn()
    {
        _homeRobot
            .DoConnect()
            .VerifyVpnStatusIsConnected()
            //Give some time for server to setup Netshield.
            .Wait(TestConstants.InitializationDelay);
        _settingsRobot
            .VerifyNetshieldIsBlocking();
    }

    [Test]
    public void NetshieldOff()
    {
        _shellRobot
            .DoNavigateToSettingsPage();
        _settingsRobot
            .DoNavigateToNetShieldSettingsPage()
            .DoSelectNetshield();
        _shellRobot
            .DoNavigateToHomePage()
            .Wait(TestConstants.DefaultNavigationDelay);
        _homeRobot
            .DoConnect()
            .VerifyVpnStatusIsConnected();
        _settingsRobot
            .VerifyNetshieldIsNotBlocking();
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}
