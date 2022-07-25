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

using NUnit.Framework;
using ProtonVPN.UI.Test.Results;
using ProtonVPN.UI.Test.TestsHelper;
using ProtonVPN.UI.Test.Windows;

namespace ProtonVPN.UI.Test.Tests
{
    [TestFixture]
    [Category("UI")]
    public class LoginLogoutTests : TestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly HomeWindow _mainWindow = new HomeWindow();
        private readonly LoginResult _loginResult = new LoginResult();
        private readonly HomeResult _homeResult = new HomeResult();

        [Test]
        public void LoginAsFreeUser()
        {
            TestCaseId = 231;

            _loginWindow.SignIn(TestUserData.GetFreeUser());
            _homeResult.CheckIfLoggedIn();

            TestRailClient.MarkTestsByStatus();
            TestCaseId = 197;
        }

        [Test]
        public void LoginWithSpecialCharsUser()
        {
            TestCaseId = 233;

            _loginWindow.SignIn(TestUserData.GetUserWithSpecialChars());
            _homeResult.CheckIfLoggedIn();
        }

        [Test]
        public void LoginAsPlusUser()
        {
            TestCaseId = 231;

            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeResult.CheckIfLoggedIn();
        }

        [Test]
        public void LoginAsVisionaryUser()
        {
            TestCaseId = 231;

            _loginWindow.SignIn(TestUserData.GetVisionaryUser());
            _homeResult.CheckIfLoggedIn();
        }

        [Test]
        public void LoginUsingIncorrectCredentials()
        {
            TestCaseId = 232;

            _loginWindow.EnterCredentials(TestUserData.GetIncorrectCredentialsUser());
            _loginResult.CheckIfLoginErrorIsDisplayed();
        }

        [Test]
        public void SuccessfulLogout()
        {
            TestCaseId = 211;

            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _mainWindow.Logout();
            _loginResult.CheckIfLoginWindowIsDisplayed();
        }

        [SetUp]
        public void TestInitialize()
        {
            DeleteUserConfig();
            LaunchApp();
        }

        [TearDown]
        public void TestCleanup()
        {
            Cleanup();
        }
    }
}
