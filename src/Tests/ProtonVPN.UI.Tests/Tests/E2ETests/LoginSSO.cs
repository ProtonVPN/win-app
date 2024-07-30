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

using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class LoginSSO : TestSession
{
    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();

    private readonly List<string> _ssoErrorMessages = new() { "Email domain associated to an existing organization. Please sign in with SSO" };

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();
    }

    [Test]
    [Retry(3)]
    public void LoginSsoDomainDetection()
    {
        //Delay to allow app to setup unauth session
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        _loginRobot.DoLogin(TestUserData.SsoUser)
            .VerifyLoginErrorIsDisplayed(_ssoErrorMessages);

        CompleteSsoLogin();
    }

    [Test]
    [Retry(3)]
    public void LoginSsoHappyPath()
    {
        //Delay to allow app to setup unauth session
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        _loginRobot.EnterEmail(TestUserData.SsoUser.Username)
            .SwitchToSsoLogin();

        CompleteSsoLogin();
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }

    private void CompleteSsoLogin()
    {
        _loginRobot.ClickSignIn()
            .DoLoginSsoWebview(TestUserData.SsoUser.Password);

        _homeRobot
            .DoCloseWelcomeOverlay()
            .DoWaitForVpnStatusSubtitleLabel();
    }
}
