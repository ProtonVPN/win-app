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

using System.Threading.Tasks;
using ProtonVPN.UI.Test.Windows;
using ProtonVPN.UI.Test.Results;
using NUnit.Framework;

namespace ProtonVPN.UI.Test.Tests
{
    [TestFixture]
    public class SecureCoreTests : UITestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly MainWindow _mainWindow = new MainWindow();
        private readonly MainWindowResults _mainWindowResults = new MainWindowResults();

        [Test]
        public async Task QuickConnectWhileSecureCoreIsEnabled()
        {
            TestCaseId = 255;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.EnableSecureCore();
            _mainWindow.QuickConnect();
            _mainWindowResults.CheckIfConnected();
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 256;
            _mainWindow.DisconnectUsingSidebarButton();
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public void CheckIfAfterKillingAppSecureCoreConnectionIsRestored()
        {
            TestCaseId = 218;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.EnableSecureCore();
            _mainWindow.QuickConnect();
            _mainWindowResults.CheckIfSameServerIsKeptAfterKillingApp();

            _mainWindow.DisconnectUsingSidebarButton();
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public async Task ConnectAndDisconnectViaMapSecureCore()
        {
            TestCaseId = 253;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.EnableSecureCore();
            _mainWindow.ConnectToCountryViaPinSecureCore("US");
            _mainWindow.WaitUntilConnected();
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 254;

            _mainWindow.DisconnectFromCountryViaPinSecureCore("US");
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
