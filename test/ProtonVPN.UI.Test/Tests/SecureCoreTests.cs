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
using ProtonVPN.UI.Test.FlaUI;
using ProtonVPN.UI.Test.FlaUI.Windows;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Tests
{
    [TestFixture]
    [Category("Connection")]
    public class SecureCoreTests : TestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly HomeWindow _mainWindow = new HomeWindow();
        private readonly SettingsWindow _settingsWindow = new SettingsWindow();

        [Test]
        public void QuickConnectWhileSecureCoreIsEnabled()
        {
            UITestSession.TestCaseId = 255;

            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _mainWindow.EnableSecureCore()
                .CloseSecureCoreWarningModal()
                .PressQuickConnectButton()
                .CheckIfConnected();

            UITestSession.TestRailClient.MarkTestsByStatus();
            UITestSession.TestCaseId = 256;

            _mainWindow.PressQuickConnectButton()
                .CheckIfDisconnected();
        }

        [Test]
        public void CheckIfAfterKillingAppSecureCoreConnectionIsRestored()
        {
            UITestSession.TestCaseId = 218;

            _loginWindow.SignIn(TestUserData.GetPlusUser())
                .EnableSecureCore()
                .CloseSecureCoreWarningModal()
                .NavigateToSettings();
            _settingsWindow.DisableStartToTray()
                .CloseSettings();
            _mainWindow.PressQuickConnectButton()
                .CheckIfConnected()
                .KillClientAndCheckIfConnectionIsKept();
        }

        [Test]
        public void ConnectAndDisconnectViaMapSecureCore()
        {
            UITestSession.TestCaseId = 253;

            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _mainWindow.EnableSecureCore()
                .CloseSecureCoreWarningModal()
                .PerformConnectionViaMapSecureCore("US")
                .CheckIfConnected();

            UITestSession.TestRailClient.MarkTestsByStatus();
            UITestSession.TestCaseId = 254;

            _mainWindow.PerformConnectionViaMapSecureCore("US")
                .CheckIfDisconnected();
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
