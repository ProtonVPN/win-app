﻿/*
 * Copyright (c) 2022 Proton
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
using ProtonVPN.UI.Test.FlaUI.Utils;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.FlaUI.Windows
{
    public class LoginWindow : FlaUIActions
    {
        private TextBox UsernameInput => ElementByAutomationId("LoginInput").AsTextBox();
        private TextBox PasswordInput => ElementByAutomationId("LoginPasswordInput").AsTextBox();
        private Button LoginButton => ElementByAutomationId("LoginButton").AsButton();
        private Button SkipButton => ElementByName("Skip").AsButton();

        public HomeWindow SignIn(TestUserData user)
        {
            WaitUntilElementExistsByAutomationId("LoginInput", TestConstants.MediumTimeout);
            UsernameInput.Enter(user.Username);
            PasswordInput.Enter(user.Password);
            LoginButton.Invoke();
            WaitUntilElementExistsByName("Skip", TestConstants.LongTimeout);
            SkipButton.Invoke();
            return new HomeWindow();
        }

        public LoginWindow CheckIfKillSwitchIsNotActive() => CheckIfNotDisplayedByName("Disable");

        public LoginWindow CheckIfLoginWindowIsDisplayed() => WaitUntilElementExistsByAutomationId("LoginInput", TestConstants.ShortTimeout);
    }
}