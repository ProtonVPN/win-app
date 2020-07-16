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

using ProtonVPN.UI.Test.Windows;
using ProtonVPN.UI.Test.Results;
using NUnit.Framework;

namespace ProtonVPN.UI.Test.Tests
{
    [TestFixture]
    public class LoginTests : UITestSession
    {
        private readonly LoginWindow _loginActions = new LoginWindow();
        private readonly LoginResult _loginResult = new LoginResult();
        private readonly MainWindowResults _mainWindowResults = new MainWindowResults();

        [Test]
        public void LoginAsFreeUser()
        {
            TestCaseId = 231;

            _loginActions.LoginWithFreeUser();
            _mainWindowResults.VerifyUserIsLoggedIn();
        }

        [Test]
        public void LoginAsBasicUser()
        {
            TestCaseId = 231;

            _loginActions.LoginWithBasicUser();
            _mainWindowResults.VerifyUserIsLoggedIn();
        }

        [Test]
        public void LoginWithSpecialCharsUser()
        {
            TestCaseId = 233;

            _loginActions.LoginWithAccountThatHasSpecialChars();
            _mainWindowResults.VerifyUserIsLoggedIn();
        }

        [Test]
        public void LoginAsPlusUser()
        {
            TestCaseId = 231;

            _loginActions.LoginWithPlusUser();
            _mainWindowResults.VerifyUserIsLoggedIn();
        }

        [Test]
        public void LoginAsVisionaryUser()
        {
            TestCaseId = 231;

            _loginActions.LoginWithVisionaryUser();
            _mainWindowResults.VerifyUserIsLoggedIn();
        }

        [Test]
        public void LoginUsingIncorrectCredentials()
        {
            TestCaseId = 232;

            _loginActions.LoginWithIncorrectCredentials();
            _loginResult.VerifyLoginErrorIsShown();
        }

        [Test]
        public void LoginAsTrialUser()
        {
            TestCaseId = 265;

            _loginActions.LoginWithTrialUser();
            _loginResult.VerifyTrialPopupIsShown();
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
