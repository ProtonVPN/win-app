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
using ProtonVPN.UI.Test.FlaUI.Utils;
using ProtonVPN.UI.Test.FlaUI.Windows;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Tests
{
    [TestFixture]
    [Category("Connection")]
    public class ConnectionTests : TestSession
    {
        private LoginWindow _loginWindow = new LoginWindow();
        private HomeWindow _homeWindow = new HomeWindow();
        private ProfilesWindow _profilesWindow = new ProfilesWindow();
        private SettingsWindow _settingsWindow = new SettingsWindow();

        [Test]
        public void QuickConnectAndDisconnect()
        {
            UITestSession.TestCaseId = 221;
            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeWindow.PressQuickConnectButton()
                .CheckIfConnected();

            UITestSession.TestRailClient.MarkTestsByStatus();
            UITestSession.TestCaseId = 222;

            _homeWindow.PressQuickConnectButton()
                .CheckIfDisconnected();
        }

        [Test]
        public void ConnectToProfileViaProfileTab()
        {
            UITestSession.TestCaseId = 225;

            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeWindow.NavigateToProfilesTab()
                .ConnectViaProfile("Fastest")
                .CheckIfConnected();
        }

        [Test]
        public void SelectConnectionByCountryVisionaryUser()
        {
            UITestSession.TestCaseId = 223;

            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeWindow.ConnectViaCountry("Netherlands")
                .CheckIfConnected();

            UITestSession.TestRailClient.MarkTestsByStatus();
            UITestSession.TestCaseId = 224;

            _homeWindow.PressDisconnectButtonInCountryList("Netherlands")
                .CheckIfDisconnected();
        }

        [Test]
        public void ConnectToCreatedProfile()
        {
            UITestSession.TestCaseId = 21551;
            UITestSession.DeleteProfiles();

            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeWindow.NavigateToProfiles();
            _profilesWindow.CreateStandartProfile(TestConstants.ProfileName)
                .ConnectToProfile(TestConstants.ProfileName);
            _homeWindow.CheckIfConnected();
        }

        [Test]
        public void LogoutWhileConnectedToVpn()
        {
            UITestSession.TestCaseId = 212;

            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeWindow.PressQuickConnectButton()
                .CheckIfConnected()
                .Logout();
            _homeWindow.ContinueLogout()
                .CheckIfLoginWindowIsDisplayed();
        }

        [Test]
        public void CancelLogoutWhileConnectedToVpn()
        {
            UITestSession.TestCaseId = 21549;

            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeWindow.PressQuickConnectButton()
                .CheckIfConnected()
                .Logout();
            _homeWindow.CancelLogout()
                .CheckIfLoggedIn();
        }

        [Test]
        public void CheckIfKillSwitchIsNotActiveOnLogout()
        {
            UITestSession.TestCaseId = 215;

            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeWindow.EnableKillSwitch()
                .PressQuickConnectButton()
                .CheckIfConnected()
                .Logout();
            _homeWindow.ContinueLogout()
                .CheckIfLoginWindowIsDisplayed()
                .CheckIfKillSwitchIsNotActive();
            _homeWindow.CheckIfDnsIsResolved();
        }

        [Test]
        public void CheckIfConnectionIsRestoredToSameServerAfterAppKill()
        {
            UITestSession.TestCaseId = 217;

            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeWindow.NavigateToSettings()
                .DisableStartToTray()
                .ClickOnConnectOnBoot()
                .CloseSettings();
            _homeWindow.PressQuickConnectButton()
                .CheckIfConnected()
                .KillClientAndCheckIfConnectionIsKept();
        }

        [Test]
        public void CheckIfAutoConnectConnectsAutomatically()
        {
            UITestSession.TestCaseId = 204;

            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeWindow.NavigateToSettings();
            _settingsWindow.DisableStartToTray()
                .CloseSettings();
            KillProtonVpnProcess();
            LaunchApp();
            _homeWindow.CheckIfConnected();

            UITestSession.TestRailClient.MarkTestsByStatus();
            UITestSession.TestCaseId = 205;

            _homeWindow.PressQuickConnectButton()
                .NavigateToSettings();
            _settingsWindow.ClickOnConnectOnBoot();
            KillProtonVpnProcess();
            LaunchApp();
            _homeWindow.CheckIfDisconnected();
        }

        [Test]
        public void ConnectAndDisconnectViaMap()
        {
            UITestSession.TestCaseId = 219;

            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeWindow.PerformConnectionViaMap("US")
                .CheckIfConnected();

            UITestSession.TestRailClient.MarkTestsByStatus();
            UITestSession.TestCaseId = 220;

            _homeWindow.PerformConnectionViaMap("US")
                .CheckIfDisconnected();
        }

        [Test]
        public void CheckCustomDnsManipulation()
        {
            UITestSession.TestCaseId = 4578;

            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeWindow.NavigateToSettings();
            _settingsWindow.NavigateToConnectionTab()
                .ClickOnCustomDnsCheckBox()
                .PressContinueToDisableNetshield()
                .CloseSettings();
            _homeWindow.CheckIfNetshieldIsDisabled();

            UITestSession.TestRailClient.MarkTestsByStatus();
            UITestSession.TestCaseId = 4579;

            _homeWindow.NavigateToSettings();
            _settingsWindow.NavigateToConnectionTab()
                .EnterCustomDnsAddress("8.8.8.8")
                .CloseSettings();
            _homeWindow.PressQuickConnectButton()
                .CheckIfConnected();
            _settingsWindow.CheckIfDnsAddressMatches("8.8.8.8");

            UITestSession.TestRailClient.MarkTestsByStatus();
            UITestSession.TestCaseId = 4581;

            _homeWindow.NavigateToSettings();
            _settingsWindow.NavigateToConnectionTab()
                .RemoveCustomDns()
                .PressReconnect()
                .CheckIfConnected();
            _settingsWindow.CheckIfDnsAddressDoesNotMatch("8.8.8.8");
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
