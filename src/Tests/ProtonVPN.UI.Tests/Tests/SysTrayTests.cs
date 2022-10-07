/*
 * Copyright (c) 2022 Proton
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
using ProtonVPN.UI.Tests.Results;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Windows;

namespace ProtonVPN.UI.Tests.Tests
{
    [TestFixture]
    [Category("Connection")]
    internal class SysTrayTests : TestSession
    {
        private LoginWindow _loginWindow = new LoginWindow();
        private HomeWindow _homeWindow = new HomeWindow();
        private SysTrayWindow _trayWindow = new SysTrayWindow();
        private SysTrayResult _trayResult = new SysTrayResult();

        [Test]
        [Category("Smoke")]
        public void QuickConnectUsingTray()
        {
            TestCaseId = 243;

            _trayWindow.OpenSysTrayWindow()
                .QuickConnect();
            _trayResult.WaitUntilConnected();

            TestRailClient.MarkTestsByStatus();
            TestCaseId = 245;

            _trayWindow.QuickConnect();
            _trayResult.WaitUntilDisconnected();
        }

        [Test]
        public void OpenAppViaTray()
        {
            TestCaseId = 246;

            _homeWindow.CloseApp();
            _trayWindow.OpenSysTrayWindow()
                .OpenProtonVpn();
            _trayResult.CheckIfClientIsRunning();
        }

        [Test]
        public void QuitAppViaTray()
        {
            TestCaseId = 247;

            _trayWindow.RightClickTrayIcon()
               .ExitTheAppViaTray();
            _trayResult.CheckIfClientIsClosed();
        }

        [Test]
        public void ConnectViaTrayProfile()
        {
            TestCaseId = 244;

            _homeWindow.CloseApp();
            _trayWindow.OpenSysTrayWindow()
                .ConnectToRandomProfile();
            _trayResult.WaitUntilConnected();
        }

        [SetUp]
        public void TestInitialize()
        {
            RestartFileExplorer();
            DeleteUserConfig();
            StartFileExplorer();
            LaunchApp();
            _loginWindow.SignIn(TestUserData.GetPlusUser())
                .NavigateToSettings()
                .ClickOnNotificationsCheckBox()
                .CloseSettings();
            _trayWindow.CloseActiveNotification()
                .ClickOnTaskBar();
        }

        [TearDown]
        public void TestCleanup()
        {
            Cleanup();
        }
    }
}
