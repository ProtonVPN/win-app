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

using System;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
[Category("ARM")]
public class LoginTests : FreshSessionSetUp
{
    private const string INCORRECT_CREDENTIALS_ERROR = "The password is not correct. Please try again with a different password.";
    private const string INCORRECT_2FA_CODE_ERROR = "Incorrect code. Please try again.";
    private const string INCORRECT_2FA_CODE = "123456";
    private const string INCORRECT_USERNAME_ERROR = "Invalid username";

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
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(TestUserData.IncorrectUser)
            .Verify.IsErrorMessageDisplayed(INCORRECT_CREDENTIALS_ERROR);
    }

    [Test]
    [Retry(3)]
    public void LoginWithTwoFactor()
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(TestUserData.TwoFactorUser)
            .EnterTwoFactorCode(TestUserData.GetTwoFactorCode());

        NavigationRobot
            .Verify.IsOnMainPage();
    }

    [Test]
    public void LoginWithIncorrectTwoFactorCode()
    {
        LoginRobot
            .Login(TestUserData.TwoFactorUser)
            .EnterTwoFactorCode(INCORRECT_2FA_CODE)
            .Verify.IsErrorMessageDisplayed(INCORRECT_2FA_CODE_ERROR);
    }

    [Test]
    public void LoginWithWhitespaceUsername()
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(TestUserData.IncorrectUserWithWhitespace)
            .Verify.IsErrorMessageDisplayed(INCORRECT_USERNAME_ERROR);
    }

    private void LoginWithUser(TestUserData user)
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(user);

        NavigationRobot
            .Verify.IsOnMainPage();
    }
}
