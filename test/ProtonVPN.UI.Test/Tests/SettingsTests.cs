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
    public class SettingsTests : UITestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly MainWindow _mainWindow = new MainWindow();
        private readonly MainWindowResults _mainWindowResults = new MainWindowResults();
        private readonly SettingsWindow _settingsWindow = new SettingsWindow();
        private readonly SettingsResult _settingsResult = new SettingsResult();

        [Test]
        public void CheckIfSettingsGeneralTabHasAllInfo()
        {
            TestCaseId = 21555;

            _loginWindow.LoginWithFreeUser();

            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();

            _settingsWindow.ClickGeneralTab();
            _settingsResult.VerifySettingsAreDisplayed();
        }

        [Test]
        public void CheckIfOpenAndCloseSavesSession()
        {
            TestCaseId = 204;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();
            _settingsWindow.ClickConnectionTab();
            _settingsWindow.EnableAutoConnectToFastestServer();
            KillProtonVPNProcessAndReopenIt();
            _mainWindow.WaitUntilConnected();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 205;
            _mainWindow.DisconnectUsingSidebarButton();
            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();
            _settingsWindow.ClickConnectionTab();
            _settingsWindow.DisableAutoConnect();
            KillProtonVPNProcessAndReopenIt();
            _mainWindowResults.CheckIfDisconnected();
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
