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
using ProtonVPN.UI.Test.ApiClient;

namespace ProtonVPN.UI.Test.Tests
{
    [TestFixture]
    [Category("Connection")]
    public class ConnectionTests : UITestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly MainWindow _mainWindow = new MainWindow();
        private readonly MainWindowResults _mainWindowResults = new MainWindowResults();
        private readonly SettingsWindow _settingsWindow = new SettingsWindow();
        private readonly SettingsResult _settingsResult = new SettingsResult();
        private readonly ModalWindow _modalWindow = new ModalWindow();
        private readonly LoginResult _loginResult = new LoginResult();
        private readonly CommonAPI _client = new CommonAPI("http://ipwhois.app");
        private readonly ProfileWindow _profileWindow = new ProfileWindow();
        private readonly ConnectionResult _connectionResult = new ConnectionResult();

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
            _mainWindow.WaitUntilConnected();
            _mainWindowResults.CheckIfConnected();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 229;
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
            await _mainWindowResults.CheckIfCorrectCountryIsShownAsync();

            _mainWindow.DisconnectUsingSidebarButton();
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public async Task ConnectToRandomServerViaProfilePlusUser()
        {
            TestCaseId = 225;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickProfilesButton();
            _mainWindow.ConnectToAProfileByName("Random");
            _mainWindow.WaitUntilConnected();
            _mainWindowResults.CheckIfConnected();
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();

            _mainWindow.DisconnectUsingSidebarButton();
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
        public void CheckCustomDnsManipulation()
        {
            TestCaseId = 4578;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();
            _settingsWindow.ClickConnectionTab();
            _settingsWindow.EnableCustomDnsServers();
            _settingsWindow.DisableNetshieldForCustomDns();
            _settingsWindow.CloseSettings();
            _mainWindowResults.CheckIfNetshieldIsDisabled();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 4579;

            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();
            _settingsWindow.EnterCustomIpv4Address("8.8.8.8");
            _settingsWindow.CloseSettings();
            _mainWindow.QuickConnect();
            _settingsResult.CheckIfDnsAddressMatches("8.8.8.8");
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 4581;

            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();
            _settingsWindow.RemoveDnsAddress();
            _settingsWindow.PressReconnect();
            _mainWindow.WaitUntilConnected();
            _settingsResult.CheckIfDnsAddressDoesNotMatch("8.8.8.8");

            _mainWindow.DisconnectUsingSidebarButton();
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public void CheckIfConnectionIsRestoredToSameServerAfterAppKill()
        {
            TestCaseId = 217;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.QuickConnect();
            _mainWindowResults.CheckIfSameServerIsKeptAfterKillingApp();
            _mainWindow.DisconnectUsingSidebarButton();
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public async Task ConnectAndDisconnectViaMap()
        {
            TestCaseId = 219;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ConnectToCountryViaPin("US");
            _mainWindow.WaitUntilConnected();
            _mainWindowResults.CheckIfConnected();
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
            await _mainWindowResults.CheckIfCorrectCountryIsShownAsync();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 220;

            _mainWindow.DisconnectFromCountryViaPin("US");
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public async Task CheckIfIpIsExcludedByIp()
        {
            TestCaseId = 7591;

            _loginWindow.LoginWithPlusUser();
            string homeIpAddress = await _client.GetIpAddress();
            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();
            _modalWindow.MoveModalUp(amountOfTimes: 8);
            _settingsWindow.ClickAdvancedTab();
            _settingsWindow.EnableSplitTunneling();
            _settingsWindow.AddIpAddressSplitTunneling("136.243.172.101");
            _settingsWindow.CloseSettings();
            _mainWindow.QuickConnect();
            await _mainWindowResults.CheckIfIpAddressIsExcluded(homeIpAddress);

            _mainWindow.DisconnectUsingSidebarButton();
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public void CheckIfAutoConnectConnectsAutomatically()
        {
            TestCaseId = 204;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();
            _settingsWindow.ClickConnectionTab();
            _settingsWindow.EnableAutoConnectToFastestServer();
            KillProtonVPNProcessAndReopenIt();
            _mainWindow.WaitUntilConnected();
            _mainWindow.DisconnectUsingSidebarButton();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 205;
            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();
            _settingsWindow.ClickConnectionTab();
            _settingsWindow.DisableAutoConnect();
            KillProtonVPNProcessAndReopenIt();
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public void ConnectToCreatedProfile()
        {
            TestCaseId = 21551;

            DeleteProfiles();
            string profileName = "@ProfileToConnect";

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickHamburgerMenu().HamburgerMenu.ClickProfiles();

            _profileWindow.ClickToCreateNewProfile()
                .EnterProfileName(profileName)
                .SelectCountryFromList("Belgium")
                .SelectServerFromList("BE#1")
                .ClickSaveButton();


            _profileWindow.ConnectToProfile(profileName);
            _mainWindow.WaitUntilConnected();
            _mainWindowResults.CheckIfConnected();

            _mainWindow.DisconnectUsingSidebarButton();
            _mainWindowResults.CheckIfDisconnected();
        }



        [Test]
        public void LogoutWhileConnectedToVpn()
        {
            TestCaseId = 212;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.QuickConnect();
            _mainWindow.ClickHamburgerMenu();
            _mainWindow.HamburgerMenu.ClickLogout();
            _modalWindow.ClickContinueButton();
            _loginWindow.WaitUntilLoginInputIsDisplayed();
            _loginResult.VerifyUserIsOnLoginWindow();
        }

        [Test]
        public void CancelLogoutWhileConnectedToVpn()
        {
            TestCaseId = 21549;

            _loginWindow.LoginWithPlusUser();

            _mainWindow
                .QuickConnect()
                .ClickHamburgerMenu()
                .HamburgerMenu
                .ClickLogout();

            _modalWindow.ClickCancelButton();

            _mainWindowResults.VerifyUserIsLoggedIn();

            _mainWindow.DisconnectUsingSidebarButton();
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public void CheckIfKillSwitchIsNotActiveOnLogout()
        {
            TestCaseId = 215;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.EnableKillSwitch();
            _mainWindow.QuickConnect();
            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickLogout();
            _modalWindow.ClickContinueButton();
            _loginWindow.WaitUntilLoginInputIsDisplayed();
            _loginResult.VerifyKillSwitchIsNotActive();
            _connectionResult.CheckIfDnsIsResolved();
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
