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
    public class LogoutTests : UITestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly LoginResult _loginResult = new LoginResult();
        private readonly MainWindow _mainWindow = new MainWindow();
        private readonly MainWindowResults _mainWindowResults = new MainWindowResults();
        private readonly LogoutConfirmationPopup _logoutConfirmationPopup = new LogoutConfirmationPopup();

        [Test]
        public void SuccessfulLogout()
        {
            TestCaseId = 211;

            _loginWindow.LoginWithPlusUser();

            _mainWindow.ClickHamburgerMenu();
            _mainWindow.HamburgerMenu.ClickLogout();
            RefreshSession();

            _loginResult.VerifyUserIsOnLoginWindow();
        }

        [Test]
        public void LogoutWhileConnectedToVpn()
        {
            TestCaseId = 212;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.QuickConnect();
            _mainWindow.ClickHamburgerMenu();

            _mainWindow.HamburgerMenu.ClickLogout();
            RefreshSession();

            _logoutConfirmationPopup.ClickContinueButton();
            RefreshSession();

            _loginWindow.WaitUntilLoginInputIsDisplayed();

            _loginResult.VerifyUserIsOnLoginWindow();
        }

        [Test]
        public void CancelLogoutWhileConnectedToVpn()
        {
            TestCaseId = 21549;

            _loginWindow.LoginWithPlusUser();

            _mainWindow
                .QuickConnect()
                .ClickHamburgerMenu()
                .HamburgerMenu
                .ClickLogout();
            
            RefreshSession();
            _logoutConfirmationPopup.ClickCancelButton();

            _mainWindowResults.VerifyUserIsLoggedIn();
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
