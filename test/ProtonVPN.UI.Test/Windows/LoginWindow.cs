/*
 * Copyright (c) 2022 Proton Technologies AG
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
using OpenQA.Selenium.Interactions;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Windows
{
    public class LoginWindow : UIActions
    {
        public LoginWindow EnterUsername(string username)
        {
            WaitUntilLoginInputIsDisplayed();
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

        public LoginWindow LoginWithVisionaryUser()
        {
            return PerformLogin(TestUserData.GetVisionaryUser().Username, TestUserData.GetVisionaryUser().Password);
        }

        public LoginWindow WaitUntilLoginInputIsDisplayed()
        {
            WaitUntilElementExistsByAutomationId("LoginInput", 10);
            return this;
        }

        private LoginWindow PerformLogin(string username, string password)
        {
            EnterUsername(username)
                .EnterPassword(password)
                .ClickLoginButton();
            
            WaitUntilElementExistsByAutomationId("WelcomeModal", 30);
            Session.FindElementByAccessibilityId("WelcomeModal").Click();
            var actions = new Actions(Session);
            actions.SendKeys(Keys.Escape).Build().Perform();
            return this;
        }
    }
}
