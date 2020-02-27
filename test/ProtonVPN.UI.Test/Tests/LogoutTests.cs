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

using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Test.Pages;

namespace ProtonVPN.UI.Test.Tests
{
    [TestClass]
    public class LogoutTests : UITestSession
    {
        readonly LoginWindow loginWindow = new LoginWindow();
        readonly MainWindow mainWindow = new MainWindow();
        readonly LogoutConfirmationPopup logoutConfirmationPopup = new LogoutConfirmationPopup();

        [TestMethod]
        public void SuccessfulLogout()
        {
            loginWindow.LoginWithPlusUser();

            mainWindow.ClickHamburgerMenu();
            mainWindow.HamburgerMenu.ClickLogout();
            RefreshSession();

            loginWindow.VerifyUserIsOnLoginWindow();
        }

        [TestMethod]
        public void LogoutWhileConnectedToVpn()
        {
            loginWindow.LoginWithPlusUser();

            mainWindow.ConnectViaQuickConnect();
            //add waitUtils with looping until element visible or smthn
            Thread.Sleep(20000);
            mainWindow.ClickHamburgerMenu();

            mainWindow.HamburgerMenu.ClickLogout();
            RefreshSession();

            logoutConfirmationPopup.ClickContinueButton();
            RefreshSession();

            loginWindow.VerifyUserIsOnLoginWindow();
        }

        [TestMethod]
        public void CancelLogoutWhileConnectedToVpn()
        {
            loginWindow.LoginWithPlusUser();

            mainWindow.ConnectViaQuickConnect();
            //add waitUtils with looping until element visible or smthn
            Thread.Sleep(20000);
            mainWindow
                .ClickHamburgerMenu()
                .HamburgerMenu
                    .ClickLogout();
            
            RefreshSession();
            logoutConfirmationPopup.ClickCancelButton();

            mainWindow.VerifyUserIsLoggedIn();
        }


        [TestInitialize]
        public void TestInitialize()
        {
            CreateSession();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TearDown();
        }
    }
}
