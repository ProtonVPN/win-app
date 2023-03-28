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

using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Results;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Windows;

namespace ProtonVPN.UI.Tests.Tests
{
    [TestFixture]
    [Category("Connection")]
    public class ConnectionTests : TestSession
    {
        private LoginWindow _loginWindow = new LoginWindow();
        private HomeWindow _homeWindow = new HomeWindow();
        private ProfilesWindow _profilesWindow = new ProfilesWindow();
        private SettingsWindow _settingsWindow = new SettingsWindow();
        private HomeResult _homeResult = new HomeResult();
        private SettingsResult _settingsResult = new SettingsResult();
        private LoginResult _loginResult = new LoginResult();

        private const string DNS_ADDRESS = "8.8.8.8";
        private const string GOOGLE_URL = "www.google.com";

        [Test]
        [Category("Smoke")]
        public async Task QuickConnectDisconnectAndValidateIP()
        {
            _homeWindow.PressQuickConnectButton()
                .WaitUntilConnected();
            await _homeResult.CheckIfCorrectIpAddressIsDisplayed();
            _homeWindow.PressQuickConnectButton()
                .WaitUntilDisconnected();
        }

        [Test]
        public void ConnectToProfileViaProfileTab()
        {
            _homeWindow.NavigateToProfilesTab()
                .ConnectViaProfile("Fastest")
                .WaitUntilConnected();
        }

        [Test]
        public void ConnectByCountryList()
        {
            _homeWindow.ConnectViaCountry("Netherlands")
                .WaitUntilConnected();
            _homeWindow.PressDisconnectButtonInCountryList("Netherlands")
                .WaitUntilDisconnected();
        }

        [Test]
        [Category("Smoke")]
        public void ConnectToCreatedProfile()
        {
            DeleteProfiles();

            _homeWindow.NavigateToProfiles();
            _profilesWindow.PressCreateNewProfile()
                .CreateProfile(TestConstants.ProfileName)
                .ConnectToProfile(TestConstants.ProfileName);
            _homeWindow.WaitUntilConnected();
        }

        [Test]
        public void LogoutWhileConnectedToVpn()
        {
            _homeWindow.PressQuickConnectButton()
                .WaitUntilConnected()
                .Logout();
            _homeWindow.ContinueLogout();
            _loginResult.CheckIfLoginWindowIsDisplayed();
        }

        [Test]
        public void CancelLogoutWhileConnectedToVpn()
        {
            _homeWindow.PressQuickConnectButton()
                .WaitUntilConnected()
                .Logout();
            _homeWindow.CancelLogout();
            _homeResult.CheckIfLoggedIn();
        }

        [Test]
        public void KillSwitchIsNotActiveOnLogout()
        {
            _homeWindow.EnableKillSwitch()
                .PressQuickConnectButton()
                .WaitUntilConnected()
                .Logout();
            _homeWindow.ContinueLogout();
            _loginResult.CheckIfLoginWindowIsDisplayed()
                .CheckIfKillSwitchIsNotActive();
            _homeResult.CheckIfDnsIsResolved(GOOGLE_URL);
        }

        [Test]
        public void ConnectionIsRestoredToSameServerAfterAppKill()
        {
            _homeWindow.NavigateToSettings()
                .DisableStartToTray()
                .ClickOnConnectOnBoot()
                .CloseSettings();
            _homeWindow.PressQuickConnectButton()
                .WaitUntilConnected();
            _homeResult.KillClientAndCheckIfConnectionIsKept();
        }

        [Test]
        [Category("Smoke")]
        public void AutoConnectConnectsAutomatically()
        {
            _homeWindow.NavigateToSettings();
            _settingsWindow.DisableStartToTray()
                .CloseSettings()
                .ExitTheApp();

            //Delay to allow app to properly exit
            Thread.Sleep(2000);
            LaunchApp();

            _homeWindow.WaitUntilConnected();
            _homeWindow.PressQuickConnectButton()
                .NavigateToSettings();
            _settingsWindow.ClickOnConnectOnBoot()
                .CloseSettings()
                .ExitTheApp();

            //Delay to allow app to properly exit
            Thread.Sleep(2000);
            LaunchApp();

            _homeWindow.WaitUntilDisconnected();
        }

        [Test]
        public void ConnectAndDisconnectViaMap()
        {
            _homeWindow.PerformConnectionViaMap(TestConstants.MapCountry)
                .WaitUntilConnected();
            _homeWindow.PerformConnectionViaMap(TestConstants.MapCountry)
                .WaitUntilDisconnected();
        }

        [Test]
        [Category("Smoke")]
        public void CustomDnsManipulation()
        {
            _homeWindow.NavigateToSettings();
            _settingsWindow.NavigateToConnectionTab()
                .ClickOnCustomDnsCheckBox()
                .PressContinueToDisableNetshield()
                .CloseSettings();
            _homeResult.CheckIfNetshieldIsDisabled();

            _homeWindow.NavigateToSettings();
            _settingsWindow.NavigateToConnectionTab()
                .EnterCustomDnsAddress(DNS_ADDRESS)
                .CloseSettings();
            _homeWindow.PressQuickConnectButton()
                .WaitUntilConnected();
            _settingsResult.CheckIfDnsAddressMatches(DNS_ADDRESS);

            _homeWindow.NavigateToSettings();
            _settingsWindow.NavigateToConnectionTab()
                .RemoveCustomDns()
                .PressReconnect()
                .WaitUntilConnected();
            _settingsResult.CheckIfDnsAddressDoesNotMatch(DNS_ADDRESS);
        }

        [Test]
        [Category("Smoke")]
        public void CancelConnectionWhileConnecting()
        {
            _homeWindow.PressQuickConnectButton()
                .CancelConnection()
                .WaitUntilDisconnected();
        }

        [Test]
        [Category("Smoke")]
        public void AppExitWithKillSwitchEnabled()
        {
            _homeWindow.PressQuickConnectButton()
                .WaitUntilConnected()
                .EnableKillSwitch()
                .ExitTheApp()
                .ConfirmExit();
            _homeResult.CheckIfDnsIsResolved(GOOGLE_URL);
        }

        [Test]
        public void ConnectUsingOpenVpnUdp()
        {
            _homeWindow.NavigateToSettings()
                .NavigateToConnectionTab()
                .SelectProtocolOpenVpnUdp()
                .CloseSettings();
            _homeWindow.PressQuickConnectButton()
                .WaitUntilConnected();
        }

        [Test]
        public void ConnectUsingOpenVpnTcp()
        {
            _homeWindow.NavigateToSettings()
                .NavigateToConnectionTab()
                .SelectProtocolOpenVpnTcp()
                .CloseSettings();
            _homeWindow.PressQuickConnectButton()
                .WaitUntilConnected();
        }

        [Test]
        public void LocalNetworkingIsReachable()
        {
            _homeWindow.PressQuickConnectButton()
                .WaitUntilConnected();
            _homeResult.CheckIfLocalNetworkingWorks(NetworkUtils.GetDefaultGatewayAddress().ToString());
        }

        [SetUp]
        public void TestInitialize()
        {
            DeleteUserConfig();
            LaunchApp();
            _loginWindow.SignIn(TestUserData.GetPlusUser());
        }

        [TearDown]
        public void TestCleanup()
        {
            Cleanup();
        }
    }
}
