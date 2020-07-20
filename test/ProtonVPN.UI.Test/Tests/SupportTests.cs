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

using NUnit.Framework;
using ProtonVPN.UI.Test.Results;
using ProtonVPN.UI.Test.TestsHelper;
using ProtonVPN.UI.Test.Windows;

namespace ProtonVPN.UI.Test.Tests
{
    [TestFixture]
    public class SupportTests : UITestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly MainWindow _mainWindow = new MainWindow();
        private readonly BugReportWindow _bugReportWindow = new BugReportWindow();
        private readonly LoginResult _loginResult = new LoginResult();

        [Test]
        public void SendBugReport()
        {
            TestCaseId = 21554;

            _loginWindow.LoginWithFreeUser();
            _mainWindow.ClickHamburgerMenu().HamburgerMenu.ClickReportBug();

            RefreshSession();

            _bugReportWindow.EnterYourEmail("test@protonmail.com")
                .EnterFeedback("Feedback")
                .ClickSend();

            _bugReportWindow.VerifySendingIsSuccessful();
        }

        [Test]
        public void ResetPassword()
        {
            TestCaseId = 200;

            UIActions.CloseAllChromeWindows();
            _loginWindow
                .ClickNeedHelpButton()
                .ClickResetPasswordButton();
            _loginResult.VerifyChromeWindowIsOpened();
        }

        [Test]
        public void ForgotUsername()
        {
            TestCaseId = 201;

            UIActions.CloseAllChromeWindows();
            _loginWindow
                .ClickNeedHelpButton()
                .ClickForgotUsernameButton();
            _loginResult.VerifyChromeWindowIsOpened();
        }

        [Test]
        public void CreateAccount()
        {
            TestCaseId = 199;

            UIActions.CloseAllChromeWindows();
            _loginWindow
                .ClickCreateAccountButton();
            _loginResult.VerifyChromeWindowIsOpened();
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
