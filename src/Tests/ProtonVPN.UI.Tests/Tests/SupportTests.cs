/*
 * Copyright (c) 2023 Proton AG
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
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Windows;

namespace ProtonVPN.UI.Tests.Tests
{
    [TestFixture]
    [Category("UI")]
    public class SupportTests : TestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly HomeWindow _mainWindow = new HomeWindow();
        private readonly BugReportWindow _bugReportWindow = new BugReportWindow();

        [Test]
        [Category("Smoke")]
        public void SendBugReport()
        {
            _loginWindow.SignIn(TestUserData.GetFreeUser());
            _mainWindow.NavigateToBugReport();
            _bugReportWindow.FillBugReportForm("Connecting to VPN")
                .VerifySendingIsSuccessful();
        }

        [Test]
        [Category("Smoke")]
        public void SendBugReportViaLoginScreen()
        {
            _loginWindow.NavigateToBugReport();
            _bugReportWindow.FillBugReportForm("Connecting to VPN")
                .VerifySendingIsSuccessful();
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
