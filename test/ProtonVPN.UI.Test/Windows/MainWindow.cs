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

using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using ProtonVPN.UI.Test.Results;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Windows
{
    public class MainWindow : UIActions
    {
        public readonly HamburgerMenu HamburgerMenu;

        public MainWindow()
        {
            HamburgerMenu = new HamburgerMenu();
        }

        public MainWindow ClickHamburgerMenu()
        {
            ClickOnObjectWithId("MenuHamburgerButton");
            return this;
        }

        public MainWindow ClickQuickConnectButton()
        {
            ClickOnObjectWithId("SidebarQuickConnectButton");
            return this;
        }

        public MainWindow ConnectByCountryName(string countryName)
        {
            MoveMouseToCountryByName(countryName);
            ClickOnObjectWithName("CONNECT");
            WaitUntilConnected();
            return this;
        }

        public MainWindow DisconnectByCountryName(string countryName)
        {
            ClickOnObjectWithName(countryName);
            ClickOnObjectWithId("Button");
            return this;
        }

        public MainWindow QuickConnect()
        {
            RefreshSession();
            ClickQuickConnectButton();
            WaitUntilConnected();
            return this;
        }

        public MainWindow ClickProfilesButton()
        {
            ClickOnObjectWithId("SidebarProfilesButton");
            return this;
        }

        public MainWindow ConnectToAProfileByName(string profileName)
        {
            WindowsElement element = Session.FindElementByName(profileName);
            MoveToElement(element);
            ClickOnObjectWithName("CONNECT");
            return this;
        }

        public MainWindow DisconnectUsingSidebarButton()
        {
            ClickQuickConnectButton();
            return this;
        }

        public MainWindow EnableSecureCore()
        {
            ClickOnObjectWithId("SecureCoreButton");
            ClickOnObjectWithId("SecureCoreOnButton");
            return this;
        }

        public MainWindow ClickOnSidebarModeButton()
        {
            ClickOnObjectWithId("SidebarModeButton");
            return this;
        }

        public MainWindow WaitUntilConnected()
        {
            WindowsElement quickConnectButton = Session.FindElementByAccessibilityId("SidebarQuickConnectButton");
            WaitUntilTextMatches(quickConnectButton, "Disconnect", seconds: 20);
            ConnectionResult.WaitUntilInternetConnectionIsRestored();
            return this;
        }

        public MainWindow EnableKillSwitch()
        {
            ClickOnObjectWithId("KillSwitchToggle");
            ClickOnObjectWithClassName("SwitchOn");
            return this;
        }

        public MainWindow ConfirmAppExit()
        {
            WaitUntilDisplayed(By.Name("Exit"), seconds: 5);
            ClickOnObjectWithName("Exit");
            return this;
        }

        public MainWindow MoveMouseToCountryByName(string countryName)
        {
            WindowsElement inputSearch = Session.FindElementByAccessibilityId("SearchInput");
            inputSearch.SendKeys(countryName);
            WindowsElement countryBox = Session.FindElementByName(countryName);
            MoveToElement(countryBox);
            return this;
        }

        public MainWindow ConnectToCountryViaPin(string countryCode)
        {
            MoveMouseToCountryPin(countryCode);
            WaitUntilElementExistsByAutomationId(countryCode, timeoutInSeconds: 5);
            ClickOnObjectWithId(countryCode);
            return this;
        }

        public MainWindow DisconnectFromCountryViaPin(string countryCode)
        {
            ConnectToCountryViaPin(countryCode);
            return this;
        }

        public MainWindow ConnectToCountryViaPinSecureCore(string countryCode)
        {
            MoveMouseToCountryPin(countryCode);
            RefreshSession();
            Actions actions = new Actions(Session);
            WindowsElement countryCodeElement = Session.FindElementByAccessibilityId(countryCode);
            actions.MoveToElement(countryCodeElement)
                .MoveByOffset(0, -25)
                .Click()
                .Perform();
            return this;
        }

        public MainWindow DisconnectFromCountryViaPinSecureCore(string countryCode)
        {
            ConnectToCountryViaPinSecureCore(countryCode);
            return this;
        }

        public string GetTextBlockIpAddress()
        {
            string textBlockIpAddress = Session.FindElementByAccessibilityId("IPAddressTextBlock").Text.RemoveExtraText();
            return textBlockIpAddress;
        }

        private void MoveMouseToCountryPin(string countryCode)
        {
            WindowsElement countryPin = Session.FindElementByAccessibilityId(countryCode);
            Actions actions = new Actions(Session);
            actions.MoveToElement(countryPin)
                .MoveByOffset(0, 25)
                .Perform();
        }
    }
}