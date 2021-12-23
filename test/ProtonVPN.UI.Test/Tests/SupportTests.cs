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
using ProtonVPN.UI.Test.Windows;

namespace ProtonVPN.UI.Test.Tests
{
    [TestFixture]
    public class SupportTests : UITestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly MainWindow _mainWindow = new MainWindow();
        private readonly BugReportWindow _bugReportWindow = new BugReportWindow();
        private readonly HamburgerMenu _hamburgerMenu = new HamburgerMenu();

        [Test]
        public void SendBugReport()
        {
            TestCaseId = 21554;

            _loginWindow.LoginWithFreeUser();
            _mainWindow.ClickHamburgerMenu();
            _hamburgerMenu.ClickReportBug();

            _bugReportWindow
                .SelectIssue("Connecting to VPN")
                .PressContactUs()
                .FillBugReportForm("Test Feedback", 3)
                .ClickSend();

            _bugReportWindow.VerifySendingIsSuccessful();
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
