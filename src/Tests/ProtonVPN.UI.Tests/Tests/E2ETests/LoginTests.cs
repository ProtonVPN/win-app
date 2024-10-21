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

using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class LoginTests : TestSession
{
    private NavigationRobot _navigationRobot = new();
    private LoginRobot _loginRobot = new();

    private const string INCORRECT_CREDENTIALS_ERROR = "The password is not correct. Please try again with a different password.";
    private const string INCORRECT_2FA_CODE_ERROR = "Incorrect code. Please try again.";
    private const string INCORRECT_2FA_CODE = "123456";

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();
    }

    [Test]
    public void LoginWithPlusUser()
    {
        LoginWithUser(TestUserData.PlusUser);
    }

    [Test]
    public void LoginWithSpecialCharsUser()
    {
        LoginWithUser(TestUserData.SpecialCharsUser);
    }

    [Test]
    public void LoginWithTwoPassUser()
    {
        LoginWithUser(TestUserData.TwoPassUser);
    }

    [Test]
    public void LoginWithIncorrectCredentials()
    {
        _navigationRobot
            .Verify.IsOnLoginPage();

        _loginRobot
            .Login(TestUserData.IncorrectUser)
            .Verify.ErrorMessageIsDisplayed(INCORRECT_CREDENTIALS_ERROR);
    }

    [Test]
    public void LoginWithTwoFactor()
    {
        _navigationRobot
            .Verify.IsOnLoginPage();

        _loginRobot
            .Login(TestUserData.TwoFactorUser)
            .EnterTwoFactorCode(TestUserData.GetTwoFactorCode());

        _navigationRobot
            .Verify.IsOnMainPage();
    }

    [Test]
    public void LoginWithIncorrectTwoFactorCode()
    {
        _loginRobot
            .Login(TestUserData.TwoFactorUser)
            .EnterTwoFactorCode(INCORRECT_2FA_CODE)
            .Verify.ErrorMessageIsDisplayed(INCORRECT_2FA_CODE_ERROR);
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }

    private void LoginWithUser(TestUserData user)
    {
        _navigationRobot
            .Verify.IsOnLoginPage();

        _loginRobot
            .Login(user);

        _navigationRobot
            .Verify.IsOnMainPage();
    }
}
