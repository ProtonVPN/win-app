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
            var inputSearch = Session.FindElementByAccessibilityId("SearchInput");
            inputSearch.SendKeys(countryName);
            var countryBox = Session.FindElementByName(countryName);
            MoveToElement(countryBox);
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

        public MainWindow CancelConnection()
        {
            ClickOnObjectWithName("Cancel");
            return this;
        }

        public MainWindow QuickConnect()
        {
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
            var element = Session.FindElementByName(profileName);
            MoveToElement(element);
            ClickOnObjectWithName("CONNECT");
            WaitUntilConnected();
            return this;
        }

        public MainWindow DisconnectUsingSidebarButton()
        {
            ClickQuickConnectButton();
            var quickConnectButton = Session.FindElementByAccessibilityId("SidebarQuickConnectButton");
            WaitUntilTextMatches(quickConnectButton, "Quick Connect", 20);
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
            var quickConnectButton = Session.FindElementByAccessibilityId("SidebarQuickConnectButton");
            WaitUntilTextMatches(quickConnectButton, "Disconnect", 20);
            return this;
        }

        public string GetTextBlockIpAddress()
        {
            var textBlockIpAddress = Session.FindElementByAccessibilityId("IPAddressTextBlock").Text.RemoveExtraText();
            return textBlockIpAddress;
        }
    }
}