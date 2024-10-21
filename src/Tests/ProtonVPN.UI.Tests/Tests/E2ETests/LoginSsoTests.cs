/*
 * Copyright (c) 2024 Proton AG
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

using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class LoginSsoTests : TestSession
{
    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();

    private const string SSO_LOGIN_ERROR = "Email domain associated to an existing organization. Please sign in with SSO";

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();
    }

    [Test]
    public void LoginSsoDomainDetection()
    {
        //Delay to allow app to setup unauth session
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        _loginRobot.Login(TestUserData.SsoUser)
            .Verify.ErrorMessageIsDisplayed(SSO_LOGIN_ERROR);

        CompleteSsoLogin();
    }

    [Test]
    public void LoginSsoHappyPath()
    {
        //Delay to allow app to setup unauth session
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        _loginRobot.EnterEmail(TestUserData.SsoUser)
            .ClickSignInWithSso();
            
        CompleteSsoLogin();
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }

    private void CompleteSsoLogin()
    {
        _loginRobot.ClickSignInButton()
            .DoLoginSsoWebview(TestUserData.SsoUser.Password);
        _homeRobot
            .Verify.IsLoggedIn();
    }
}
