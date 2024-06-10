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
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests;

[TestFixture]
[Category("UI")]
public class LoginTests : TestSession
{
    public const string FREE_PLAN_NAME = "Proton VPN Free";

    private static readonly List<string> INCORRECT_CREDENTIALS_MESSAGES = new()
    {
        "Incorrect login credentials.",
        "This address has been disabled by the account owner.",
        "This username does not exist.",
        "Are you sure this is the correct domain?",
        "Did you mean proton.me instead?",
        "This email address does not exist.",
        "The password is not correct.",
    };

    private static readonly List<string> ZERO_VPN_CONNECTIONS_ASSIGNED_MESSAGES = new()
    {
        "You need first to assign connections to your account or any other sub-account"
    };

    private LoginRobot _loginRobot = new();
    private ShellRobot _shellRobot = new();
    private HomeRobot _homeRobot = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();
    }

    [Test]
    public void LoginWithFreeUser()
    {
        LoginWithUser(TestUserData.FreeUser, FREE_PLAN_NAME);
    }

    [Test]
    public void LoginWithSpecialCharsUser()
    {
        LoginWithUser(TestUserData.SpecialCharsUser, FREE_PLAN_NAME);
    }

    [Test]
    public void LoginWithTwoPassUser()
    {
        LoginWithUser(TestUserData.TwoPassUser, FREE_PLAN_NAME);
    }

    [Test]
    public void LoginWith2FactorAuthUser()
    {
        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.TwoFactorUser)
            .DoEnterTwoFactorCode(TestUserData.GetTwoFactorCode());

        _homeRobot
            .DoCloseWelcomeOverlay()
            .DoWaitForVpnStatusSubtitleLabel();

        _shellRobot
            .VerifyUserIsLoggedIn(TestUserData.TwoFactorUser, FREE_PLAN_NAME)
            .DoOpenAccount()
            .VerifyAccountMenu();
    }

    [Test]
    public void LoginWithIncorrectUser()
    {
        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.IncorrectUser)
            .VerifyLoginErrorIsDisplayed(INCORRECT_CREDENTIALS_MESSAGES);
    }

    [Test]
    public void LoginWithZeroAssignedConnectionsUser()
    {
        _loginRobot
            .DoLogin(TestUserData.ZeroAssignedConnectionsUser)
            .VerifyLoginErrorIsDisplayed(ZERO_VPN_CONNECTIONS_ASSIGNED_MESSAGES);
    }

    public void LoginWithUser(TestUserData user, string planName)
    {
        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(user);

        _homeRobot
            .DoCloseWelcomeOverlay()
            .DoWaitForVpnStatusSubtitleLabel();

        _shellRobot
            .VerifyUserIsLoggedIn(user, planName)
            .DoOpenAccount()
            .VerifyAccountMenu();
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}