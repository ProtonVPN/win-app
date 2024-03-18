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
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots.Login;

public partial class LoginRobot
{
    public LoginRobot DoLogin(TestUserData user)
    {
        UsernameTextBox.Text = user.Username;
        PasswordBox.Text = user.Password;
        SignInButton.Invoke();
        return this;
    }

    public LoginRobot DoEnterTwoFactorCode(string code)
    {
        TwoFactorInputField("First").Text = (code[0].ToString());
        TwoFactorInputField("Second").Text = (code[1].ToString());
        TwoFactorInputField("Third").Text = (code[2].ToString());
        TwoFactorInputField("Fourth").Text = (code[3].ToString());
        TwoFactorInputField("Fifth").Text = (code[4].ToString());
        TwoFactorInputField("Sixth").Text = (code[5].ToString());
        return this;
    }

    public LoginRobot DoReportAnIssue()
    {
        HelpButton.Invoke();
        CommonActions.Wait(TimeSpan.FromSeconds(1));
        ReportIssueMenuItem.Click();
        return this;
    }
}