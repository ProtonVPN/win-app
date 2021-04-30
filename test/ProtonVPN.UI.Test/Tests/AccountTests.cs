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
using ProtonVPN.UI.Test.TestsHelper;
using NUnit.Framework;

namespace ProtonVPN.UI.Test.Tests
{
    [TestFixture]
    public class AccountTests : UITestSession
    {
        private readonly AccountResult _accountResult = new AccountResult();
        private readonly MainWindow _mainWindow = new MainWindow();
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly MainWindowResults _mainWindowResults = new MainWindowResults();
        private readonly ModalWindow _modalWindow = new ModalWindow();

        [Test]
        public void CheckIfUsernameIsDisplayedInAccountSection()
        {
            TestCaseId = 199;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickAccount();
            RefreshSession();
            _accountResult.VerifyLoggedInAsTextIs(TestUserData.GetPlusUser().Username);
        }

        [Test]
        public void CheckRestrictedAccountConnections()
        {
            TestCaseId = 262;

            _loginWindow.LoginWithFreeUser();
            _mainWindow.MoveMouseToCountryByName("Canada");
            _mainWindowResults.CheckIfConnectButtonIsNotDisplayed();


            _mainWindow.ClickProfilesButton();
            _mainWindow.ConnectToAProfileByName("SecureCore");
            _mainWindowResults.CheckIfUpgradeRequiredModalIsShown("Secure Core");
            _modalWindow.ClickCancelButton();

            _mainWindow.ConnectToCountryViaPin("MX");
            _mainWindowResults.CheckIfUpgradeRequiredModalIsShown("Plus Server");
            _modalWindow.ClickCancelButton();

            _mainWindow.ConnectToAProfileByName("PaidCountry");
            _mainWindowResults.CheckIfUpgradeRequiredModalIsShown("Plus Server");
            _modalWindow.ClickCancelButton();

            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickLogout();
            _loginWindow.WaitUntilLoginInputIsDisplayed();
            _loginWindow.LoginWithBasicUser();
            _mainWindow.ClickProfilesButton();
            _mainWindow.ConnectToAProfileByName("SecureCore");
            _mainWindowResults.CheckIfUpgradeRequiredModalIsShown("Secure Core");
        }

        [SetUp]
        public void SetUp()
        {
            CreateSession();
        }

        [TearDown]
        public void CleanUp()
        {
            TearDown();
        }
    }
}
