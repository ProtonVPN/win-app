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
using FlaUI.Core.AutomationElements;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Windows
{
    public class HomeWindow : UIActions
    {
        private Button QuickConnectButton => ElementByAutomationId("SidebarQuickConnectButton").AsButton();
        private AutomationElement HamburgerMenuButton => ElementByAutomationId("MenuHamburgerButton").AsMenu().FindFirstChild();
        private Button ProfilesButton => ElementByAutomationId("MenuProfilesButton").AsButton();
        private Button BugReportButton => ElementByAutomationId("MenuReportBugButton").AsButton();
        private Button AccountButton => ElementByAutomationId("MenuAccountButton").AsButton();
        private Button ExitButton => ElementByAutomationId("MenuExitButton").AsButton();
        private Button ProfilesTab => ElementByAutomationId("SidebarProfilesButton").AsButton();
        private AutomationElement Profile(string profileName) => ElementByName(profileName);
        private Button ConnectButton => FirstVisibleElementByName("Connect").AsButton();
        private AutomationElement Country(string countryName) => ElementByName(countryName);
        private AutomationElement CountryListDisconnectButton => ElementByAutomationId("Button");
        private TextBox SearchInput => ElementByAutomationId("SearchInput").AsTextBox();
        private Button LogoutButton => ElementByAutomationId("MenuLogoutButton").AsButton();
        private AutomationElement KillSwitchToggle => ElementByAutomationId("KillSwitchToggle");
        private AutomationElement KillSwitchOn => ElementByClassName("SwitchOn");
        private Button SettingsMenuButton => ElementByAutomationId("MenuSettingsButton").AsButton();
        private AutomationElement CountryPin(string countryCode) => ElementByAutomationId(countryCode);
        private AutomationElement SecureCorePin => ElementByAutomationId("SecureCoreButton");
        private AutomationElement SecureCoreON => ElementByAutomationId("SecureCoreOnButton");
        private AutomationElement SecureCoreWarningCloseButton => ElementByName("Activate Secure Core");
        private AutomationElement ModalCloseButton => ElementByAutomationId("ModalCloseButton");
        private AutomationElement SidebarModeButton => ElementByAutomationId("SidebarModeButton");
        private Button CancelButton => ElementByAutomationId("CancelButton").AsButton();
        private Button ClientCloseButton => ElementByAutomationId("CloseButton").AsButton();
        private AutomationElement NetshieldToggle => ElementByAutomationId("NetShieldToggle");
        private AutomationElement NetshieldOff => ElementByClassName("Shield");
        private AutomationElement NetshieldLevelOne => ElementByClassName("ShieldHalfFilled");
        private AutomationElement NetshieldLevelTwo => ElementByClassName("ShieldFilled");
        private AutomationElement ChevronDown => ElementByClassName("ChevronDown");

        public HomeWindow PressQuickConnectButton()
        {
            QuickConnectButton.Invoke();
            return this;
        }

        public HomeWindow NavigateToProfilesTab()
        {
            ProfilesTab.Invoke();
            return this;
        }

        public ProfilesWindow NavigateToProfiles()
        {
            HamburgerMenuButton.Click();
            ProfilesButton.Invoke();
            return new ProfilesWindow();
        }

        public SettingsWindow NavigateToSettings()
        {
            HamburgerMenuButton.Click();
            SettingsMenuButton.Invoke();
            Thread.Sleep(2000);
            return new SettingsWindow();
        }

        public BugReportWindow NavigateToBugReport()
        {
            HamburgerMenuButton.Click();
            BugReportButton.Invoke();
            return new BugReportWindow();
        }

        public HomeWindow NavigateToAccount()
        {
            HamburgerMenuButton.Click();
            AccountButton.Invoke();
            return this;
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
            WaitUntilElementExistsByAutomationIdAndReturnTheElement("ContinueButton", TestConstants.MediumTimeout).AsButton().Invoke();
            return new LoginWindow();
        }

        public HomeWindow CancelLogout()
        {
            WaitUntilElementExistsByAutomationIdAndReturnTheElement("CancelActionButton", TestConstants.MediumTimeout).AsButton().Invoke();
            return this;
        }

        public HomeWindow EnableKillSwitch()
        {
            KillSwitchToggle.Click();
            KillSwitchOn.Click();
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

        public HomeWindow MoveMouseOnCountry(string countryName)
        {
            SearchInput.Text = countryName;
            MoveMouseToElement(ChevronDown);
            return this;
        }

        public HomeWindow ClickWindowsCloseButton()
        {
            ModalCloseButton.Click();
            return this;
        }

        public HomeWindow ClickOnSidebarModeButton()
        {
            SidebarModeButton.Click();
            return this;
        }

        public HomeWindow CancelConnection()
        {
            CancelButton.Invoke();
            return this;
        }

        public HomeWindow ExitTheApp()
        {
            HamburgerMenuButton.Click();
            ExitButton.Invoke();
            return this;
        }

        public HomeWindow ConfirmExit()
        {
            WaitUntilElementExistsByAutomationIdAndReturnTheElement(
                "CloseButton", 
                TestConstants.VeryShortTimeout).AsButton().Invoke();
            return this;
        }

        public HomeWindow CloseApp()
        {
            ClientCloseButton.Invoke();
            return this;
        }

        public HomeWindow EnableNetshieldLevelTwo()
        {
            NetshieldToggle.Click();
            NetshieldLevelTwo.Click();
            return this;
        }

        public HomeWindow DisableNetshield()
        {
            NetshieldToggle.Click();
            NetshieldOff.Click();
            return this;
        }

        public HomeWindow EnableNetshieldLevelOne()
        {
            NetshieldToggle.Click();
            NetshieldLevelOne.Click();
            return this;
        }

        public HomeWindow WaitUntilDisconnected() => WaitUntilTextMatchesByAutomationId(
            "SidebarQuickConnectButton",
            TestConstants.MediumTimeout,
            "Quick Connect",
            "Failed to disconnect in " + TestConstants.MediumTimeout.Seconds + " s");

        public HomeWindow WaitUntilConnected() => WaitUntilTextMatchesByAutomationId(
            "SidebarQuickConnectButton",
            TestConstants.MediumTimeout,
            "Disconnect",
            "Failed to connect");
    } 
}
