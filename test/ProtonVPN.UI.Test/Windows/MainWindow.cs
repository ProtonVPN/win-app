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

using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
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

        public MainWindow ClickProfilesButton()
        {
            ClickOnObjectWithId("SidebarProfilesButton");
            return this;
        }

        public MainWindow ConnectToAProfileByName(string profileName)
        {
            WindowsElement element = Session.FindElementByName(profileName);
            MoveToElement(element);
            ClickOnObjectWithName("Connect");
            return this;
        }

        public MainWindow ClickOnSidebarModeButton()
        {
            ClickOnObjectWithId("SidebarModeButton");
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