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
using System.Threading.Tasks;
using ProtonVPN.UI.Test.Windows;
using ProtonVPN.UI.Test.Results;
using NUnit.Framework;

namespace ProtonVPN.UI.Test.Tests
{
    [TestFixture]
    public class ConnectionTests : UITestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly MainWindow _mainWindow = new MainWindow();
        private readonly MainWindowResults _mainWindowResults = new MainWindowResults();
        private readonly SettingsWindow _settingsWindow = new SettingsWindow();
        private readonly SettingsResult _settingsResult = new SettingsResult();

        [Test]
        public async Task ConnectUsingQuickConnectTrialUser()
        {
            TestCaseId = 225;

            _loginWindow.LoginWithTrialUser();
            _mainWindow.QuickConnect();
            _mainWindowResults.CheckIfConnected();
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
            await _mainWindowResults.CheckIfCorrectCountryIsShownAsync();
            _mainWindow.DisconnectUsingSidebarButton();
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public async Task ConnectUsingQuickConnectFreeUser()
        {
            TestCaseId = 225;

            _loginWindow.LoginWithFreeUser();
            _mainWindow.QuickConnect();
            _mainWindowResults.CheckIfConnected();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 229;
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
            await _mainWindowResults.CheckIfCorrectCountryIsShownAsync();
        }

        [Test]
        public async Task ConnectUsingQuickConnectBasicUser()
        {
            TestCaseId = 221;

            _loginWindow.LoginWithBasicUser();
            _mainWindow.QuickConnect();
            _mainWindowResults.CheckIfConnected();
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
            await _mainWindowResults.CheckIfCorrectCountryIsShownAsync();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 222;

            _mainWindow.DisconnectUsingSidebarButton();
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public async Task ConnectToFastestViaProfilePlusUser()
        {
            TestCaseId = 225;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickProfilesButton();
            _mainWindow.ConnectToAProfileByName("Fastest");
            _mainWindowResults.CheckIfConnected();
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
            await _mainWindowResults.CheckIfCorrectCountryIsShownAsync();
        }

        [Test]
        public async Task ConnectToRandomServerViaProfilePlusUser()
        {
            TestCaseId = 225;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickProfilesButton();
            _mainWindow.ConnectToAProfileByName("Random");
            _mainWindowResults.CheckIfConnected();
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
        }

        [Test]
        public void CancelConnectionWhileConnectingPlusUser()
        {
            TestCaseId = 227;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickQuickConnectButton();
            //Pause imitates user delay
            Thread.Sleep(1000);
            _mainWindow.CancelConnection();
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public async Task SelectConnectionByCountryVisionaryUser()
        {
            TestCaseId = 223;

            _loginWindow.LoginWithVisionaryUser();
            _mainWindow.ConnectByCountryName("Ukraine");
            _mainWindowResults.CheckIfConnected();
            await _mainWindowResults.CheckIfCorrectCountryIsShownAsync();
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 224;

            _mainWindow.DisconnectByCountryName("Ukraine");
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public void CheckIfCustomDnsAddressIsSet()
        {
            TestCaseId = 4579;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickHamburgerMenu().HamburgerMenu.ClickSettings();
            _settingsWindow.ClickConnectionTab();
            _settingsWindow.EnableCustomDnsServers();
            _settingsWindow.EnterCustomIpv4Address("8.8.8.8");
            _settingsWindow.CloseSettings();
            _mainWindow.QuickConnect();
            _settingsResult.CheckIfDnsAddressMatches("8.8.8.8");
        }

        [Test]
        public void CheckIfInvalidDnsIsNotPermitted()
        {
            TestCaseId = 4580;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickHamburgerMenu().HamburgerMenu.ClickSettings();
            _settingsWindow.ClickConnectionTab();
            _settingsWindow.EnableCustomDnsServers();
            _settingsWindow.EnterCustomIpv4Address("1.A.B.4");
            _settingsResult.CheckIfCustomDnsAddressWasNotAdded();
        }

        [Test]
        public void CheckIfConnectionIsRestoredToSameServerAfterAppKill()
        {
            TestCaseId = 217;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.QuickConnect();
            _mainWindowResults.CheckIfSameServerIsKeptAfterKillingApp();
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
