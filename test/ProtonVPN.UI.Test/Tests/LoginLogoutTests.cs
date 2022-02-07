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

using ProtonVPN.UI.Test.Windows;
using ProtonVPN.UI.Test.Results;
using NUnit.Framework;

namespace ProtonVPN.UI.Test.Tests
{
    [TestFixture]
    [Category("UI")]
    public class LoginLogoutTests : UITestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly LoginResult _loginResult = new LoginResult();
        private readonly MainWindowResults _mainWindowResults = new MainWindowResults();
        private readonly MainWindow _mainWindow = new MainWindow();

        [Test]
        public void LoginAsFreeUser()
        {
            TestCaseId = 231;

            _loginWindow.LoginWithFreeUser();
            _mainWindowResults.VerifyUserIsLoggedIn();
            TestRailClient.MarkTestsByStatus();

            //In CI side, fresh installation is performed
            TestCaseId = 197;
        }

        [Test]
        public void LoginAsBasicUser()
        {
            TestCaseId = 231;

            _loginWindow.LoginWithBasicUser();
            _mainWindowResults.VerifyUserIsLoggedIn();
        }

        [Test]
        public void LoginWithSpecialCharsUser()
        {
            TestCaseId = 233;

            _loginWindow.LoginWithAccountThatHasSpecialChars();
            _mainWindowResults.VerifyUserIsLoggedIn();
        }

        [Test]
        public void LoginAsPlusUser()
        {
            TestCaseId = 231;

            _loginWindow.LoginWithPlusUser();
            _mainWindowResults.VerifyUserIsLoggedIn();
        }

        [Test]
        public void LoginAsVisionaryUser()
        {
            TestCaseId = 231;

            _loginWindow.LoginWithVisionaryUser();
            _mainWindowResults.VerifyUserIsLoggedIn();
        }

        [Test]
        public void LoginUsingIncorrectCredentials()
        {
            TestCaseId = 232;

            _loginWindow.LoginWithIncorrectCredentials();
            _loginResult.VerifyLoginErrorIsShown();
        }

        [Test]
        public void SuccessfulLogout()
        {
            TestCaseId = 211;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickHamburgerMenu();
            _mainWindow.HamburgerMenu.ClickLogout();
            _loginWindow.WaitUntilLoginInputIsDisplayed();
            _loginResult.VerifyUserIsOnLoginWindow();
        }

        [SetUp]
        public void TestInitialize()
        {
            CreateSession();
        }

        [TearDown]
        public void TestCleanup()
        {
            TearDown();
        }
    }
}
