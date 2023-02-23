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

using FlaUI.Core.AutomationElements;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Windows
{
    public class LoginWindow : UIActions
    {
        private TextBox UsernameInput => ElementByAutomationId("LoginInput").AsTextBox();
        private TextBox PasswordInput => ElementByAutomationId("LoginPasswordInput").AsTextBox();
        private Button LoginButton => ElementByAutomationId("LoginButton").AsButton();
        private Button SkipButton => ElementByName("Skip").AsButton();
        private Button HelpButton => ElementByAutomationId("HelpButton").AsButton();
        private Button ReportAnIssueButton => ElementByAutomationId("ReportAnIssueButton").AsButton();
        private TextBox TwoFaInput => ElementByAutomationId("TwoFactorAuthInput").AsTextBox();

        public HomeWindow SignIn(TestUserData user)
        {
            EnterCredentials(user);
            WaitUntilElementExistsByName("Skip", TestConstants.LongTimeout);
            SkipButton.Invoke();
            return new HomeWindow();
        }

        public LoginWindow EnterCredentials(TestUserData user)
        {
            WaitUntilElementExistsByAutomationId("LoginInput", TestConstants.MediumTimeout);
            UsernameInput.Text = user.Username;
            PasswordInput.Text = user.Password;
            LoginButton.Invoke();
            return this;
        }

        public HomeWindow EnterTwoFactorCode(string twoFaCode)
        {
            WaitUntilDisplayedByAutomationId("TwoFactorAuthInput", TestConstants.LongTimeout);
            TwoFaInput.Text = twoFaCode;
            LoginButton.Invoke();
            WaitUntilElementExistsByName("Skip", TestConstants.LongTimeout);
            return new HomeWindow();
        }

        public BugReportWindow NavigateToBugReport()
        {
            HelpButton.Invoke();
            ReportAnIssueButton.Invoke();
            return new BugReportWindow();
        }
    }
}
