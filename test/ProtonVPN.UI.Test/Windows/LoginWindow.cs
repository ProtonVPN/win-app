/*
 * Copyright (c) 2020 Proton Technologies AG
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

using OpenQA.Selenium;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Windows
{
    public class LoginWindow : UIActions
    {
        public LoginWindow EnterUsername(string username)
        {
            WaitUntilDisplayed(By.XPath("//*[@AutomationId='LoginInput']"), 5);
            ClickOnObjectWithId("LoginInput");
            InsertTextIntoFieldWithId("LoginInput", username);
            return this;
        }

        public LoginWindow EnterPassword(string password)
        {
            ClickOnObjectWithId("LoginPasswordInput");
            InsertTextIntoFieldWithId("LoginPasswordInput", password);
            return this;
        }

        public LoginWindow ClickLoginButton()
        {
            ClickOnObjectWithId("LoginButton");
            return this;
        }

        public LoginWindow LoginWithPlusUser()
        {
            return PerformLogin(TestUserData.GetPlusUser().Username, TestUserData.GetPlusUser().Password);
        }

        public LoginWindow LoginWithIncorrectCredentials()
        {
            EnterUsername("IncorrectName")
                .EnterPassword("IncorrectPassword")
                .ClickLoginButton();
            return this;
        }

        public LoginWindow LoginWithFreeUser()
        {
            return PerformLogin(TestUserData.GetFreeUser().Username, TestUserData.GetFreeUser().Password);
        }

        public LoginWindow LoginWithAccountThatHasSpecialChars()
        {
            return PerformLogin(TestUserData.GetUserWithSpecialChars().Username, TestUserData.GetUserWithSpecialChars().Password);
        }

        public LoginWindow LoginWithBasicUser()
        {
            return PerformLogin(TestUserData.GetBasicUser().Username, TestUserData.GetBasicUser().Password);
        }

        public LoginWindow LoginWithVisionaryUser()
        {
            return PerformLogin(TestUserData.GetVisionaryUser().Username, TestUserData.GetVisionaryUser().Password);
        }

        public LoginWindow LoginWithTrialUser()
        {
            EnterUsername(TestUserData.GetTrialUser().Username)
                .EnterPassword(TestUserData.GetTrialUser().Password)
                .ClickLoginButton();
            WaitUntilElementIsNotVisible(By.ClassName("Loading"), 15);
            RefreshSession();
            return this;
        }

        private LoginWindow PerformLogin(string username, string password)
        {
            EnterUsername(username)
                .EnterPassword(password)
                .ClickLoginButton();

            WaitUntilElementIsNotVisible(By.ClassName("Loading"), 15);
            RefreshSession();

            ClickOnObjectWithName("Skip");

            return this;
        }
    }
}
