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

using System;
using System.Threading;
using OpenQA.Selenium;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Pages
{
    public class LoginWindow : UITestSession
    {

        private LoginWindow Login(string username, string password)
        {
            EnterUsername(username)
                .EnterPassword(password)
                .ClickLoginButton();

            RefreshSession();

            var modal = Session.FindElementByAccessibilityId("WelcomeModal");
            modal.FindElementByAccessibilityId("CloseButton").Click();

            return this;
        }

        public LoginWindow LoginWithPlusUser()
        {
            return Login(TestUserData.GetPlusUser().Username, TestUserData.GetPlusUser().Password);
        }

        public LoginWindow LoginWithFreeUser()
        {
            return Login(TestUserData.GetFreeUser().Username, TestUserData.GetFreeUser().Password);
        }

        public LoginWindow LoginWithBasicUser()
        {
            return Login(TestUserData.GetBasicUser().Username, TestUserData.GetBasicUser().Password);
        }

        public LoginWindow LoginWithVisionaryUser()
        {
            return Login(TestUserData.GetVisionaryUser().Username, TestUserData.GetVisionaryUser().Password);
        }

        public LoginWindow LoginWithTrialUser()
        {
            return Login(TestUserData.GetTrialUser().Username, TestUserData.GetTrialUser().Password);
        }

        public LoginWindow EnterUsername(string username)
        {
            UIActions.InsertTextIntoFieldWithId("LoginInput", username);
            return this;
        }

        public LoginWindow EnterPassword(string password)
        {
            UIActions.InsertTextIntoFieldWithId("LoginPasswordInput", password);
            return this;
        }

        public LoginWindow ClickLoginButton()
        {
            UIActions.ClickOnObjectWithId("LoginButton");
            return this;
        }

        public LoginWindow VerifyUserIsOnLoginWindow()
        {
            UIActions.CheckIfObjectWithNameIsDisplayed("Start with Windows");
            return this;
        }
    }
}
