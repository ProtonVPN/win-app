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

using System.Net;
using System.Net.Sockets;
using System.Threading;
using FlaUI.Core.AutomationElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Test.FlaUI.Utils;

namespace ProtonVPN.UI.Test.FlaUI.Windows
{
    public class HomeWindow : FlaUIActions
    {
        private Button QuickConnectButton => ElementByAutomationId("SidebarQuickConnectButton").AsButton();
        private AutomationElement HamburgerMenuButton => ElementByAutomationId("MenuHamburgerButton").AsMenu().FindFirstChild();
        private Button ProfilesButton => ElementByAutomationId("MenuProfilesButton").AsButton();
        private AutomationElement ProfilesTab => ElementByName("Profiles");
        private AutomationElement Profile(string profileName) => ElementByName(profileName);
        private Button ConnectButton => ElementByName("Connect").AsButton();
        private AutomationElement Country(string countryName) => ElementByName(countryName);
        private AutomationElement CountryListDisconnectButton => ElementByAutomationId("Button");
        private TextBox SearchInput => ElementByAutomationId("SearchInput").AsTextBox();
        private Button LogoutButton => ElementByAutomationId("MenuLogoutButton").AsButton();
        private Button ContinueButton => ElementByAutomationId("ContinueButton").AsButton();
        private Button CancelButton => ElementByAutomationId("CancelActionButton").AsButton();
        private AutomationElement KillSwitchToggle => ElementByAutomationId("KillSwitchToggle");
        private AutomationElement KillSwitchOn => ElementByClassName("SwitchOn");
        private Button SettingsMenuButton => ElementByAutomationId("MenuSettingsButton").AsButton();
        private Label IpAddressLabel => ElementByAutomationId("IPAddressTextBlock").AsLabel();
        private AutomationElement CountryPin(string countryCode) => ElementByAutomationId(countryCode);
        private AutomationElement SecureCorePin => ElementByAutomationId("SecureCoreButton");
        private AutomationElement SecureCoreON => ElementByAutomationId("SecureCoreOnButton");
        private AutomationElement SecureCoreWarningCloseButton => ElementByName("Activate Secure Core");

        public HomeWindow PressQuickConnectButton()
        {
            QuickConnectButton.Invoke();
            return this;
        }

        public HomeWindow NavigateToProfilesTab()
        {
            ProfilesTab.Click();
            return this;
        }

        public ProfilesWindow NavigateToProfiles()
        {
            HamburgerMenuButton.Click();
            ProfilesButton.Invoke();
            Thread.Sleep(2000);
            return new ProfilesWindow();
        }

        public SettingsWindow NavigateToSettings()
        {
            HamburgerMenuButton.Click();
            SettingsMenuButton.Invoke();
            WaitUntilDisplayedByAutomationId("StartMinimizedCombobox", TestConstants.ShortTimeout);
            Thread.Sleep(2000);
            return new SettingsWindow();
        }

        public HomeWindow ConnectViaProfile(string profileName)
        {
            MoveMouseToElement(Profile(profileName));
            ConnectButton.Click();
            return this;
        }

        public HomeWindow ConnectViaCountry(string countryName)
        {
            SearchInput.Enter(countryName);
            MoveMouseToElement(Country(countryName));
            ConnectButton.Click();
            return this;
        }

        public HomeWindow PressDisconnectButtonInCountryList(string countryName)
        {
            ElementByName(countryName).Click();
            CountryListDisconnectButton.Click();
            return this;
        }

        public LoginWindow Logout()
        {
            HamburgerMenuButton.Click();
            LogoutButton.Invoke();
            return new LoginWindow();
        }

        public LoginWindow ContinueLogout()
        {
            WaitUntilElementExistsByAutomationId("ContinueButton", TestConstants.ShortTimeout);
            Thread.Sleep(2000);
            ContinueButton.Invoke();
            return new LoginWindow();
        }

        public HomeWindow CancelLogout()
        {
            WaitUntilElementExistsByAutomationId("CancelActionButton", TestConstants.ShortTimeout);
            Thread.Sleep(2000);
            CancelButton.Invoke();
            return this;
        }

        public HomeWindow CheckIfLoggedIn()
        {
            CheckIfExistsByAutomationId("MenuHamburgerButton");
            return this;
        }

        public HomeWindow EnableKillSwitch()
        {
            KillSwitchToggle.Click();
            KillSwitchOn.Click();
            return this;
        }

        public string GetTextBlockIpAddress()
        {
            return IpAddressLabel.Text;
        }

        public HomeWindow KillClientAndCheckIfConnectionIsKept()
        {
            string ipAddress = GetTextBlockIpAddress();
            KillAndRestartProtonVPNClient();
            CheckIfConnected();
            Assert.IsTrue(ipAddress == GetTextBlockIpAddress());
            return this;
        }

        public HomeWindow PerformConnectionViaMap(string countryCode)
        {
            CountryPin(countryCode).FindFirstChild().AsButton().Invoke();
            return this;
        }

        public HomeWindow PerformConnectionViaMapSecureCore(string countryCode)
        {
            CountryPin(countryCode).FindFirstChild().FindFirstChild().AsButton().Invoke();
            return this;
        }

        public HomeWindow EnableSecureCore()
        {
            SecureCorePin.Click();
            SecureCoreON.Click();
            return this;
        }

        public HomeWindow CloseSecureCoreWarningModal()
        {
            SecureCoreWarningCloseButton.Click();
            return this;
        }

        public HomeWindow CheckIfDisconnected() => WaitUntilTextMatchesByAutomationId(
            "SidebarQuickConnectButton", 
            TestConstants.MediumTimeout, 
            "Quick Connect",
            "Failed to disconnect in " + TestConstants.MediumTimeout.Seconds + " s");
      
        public HomeWindow CheckIfConnected() => WaitUntilTextMatchesByAutomationId(
            "SidebarQuickConnectButton", 
            TestConstants.MediumTimeout, 
            "Disconnect",
            "Failed to connect");

        public HomeWindow CheckIfNetshieldIsDisabled() => CheckIfDisplayedByClassName("Shield");

        public HomeWindow CheckIfDnsIsResolved()
        {
            Assert.IsTrue(IsConnectedToInternet(), "User was not connected to internet.");
            return this;
        }

        private void KillAndRestartProtonVPNClient()
        {
            KillProtonVpnProcess();
            LaunchApp();
            WaitUntilElementExistsByAutomationId("MenuHamburgerButton", TestConstants.LongTimeout);
        }

        private static bool IsConnectedToInternet()
        {
            bool isConnected = true;
            try
            {
                Dns.GetHostEntry("www.google.com");
            }
            catch (SocketException)
            {
                isConnected = false;
            }
            return isConnected;
        }
    } 
}
