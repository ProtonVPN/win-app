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

namespace ProtonVPN.UI.Test.Pages
{
    public class MainWindow : UITestSession
    {
        public readonly HamburgerMenu HamburgerMenu;

        public MainWindow()
        {
            HamburgerMenu = new HamburgerMenu();
        }

        public MainWindow VerifyUserIsLoggedIn()
        {
            UIActions.CheckIfObjectWithIdIsDisplayed("MenuHamburgerButton");
            return this;
        }

        public MainWindow ClickHamburgerMenu()
        {
            UIActions.ClickOnObjectWithId("MenuHamburgerButton");
            return this;
        }

        public MainWindow ClickQuickConnectButton()
        {
            UIActions.ClickOnObjectWithId("SidebarQuickConnectButton");
            return this;
        }

        public MainWindow ConnectViaQuickConnect()
        {
            var quickConnectElement = Session.FindElementByAccessibilityId("SidebarQuickConnectButton");
            if (quickConnectElement.GetAttribute("Name") == "Disconnect")
            {
                ClickQuickConnectButton();
            }
            return ClickQuickConnectButton();
        }

        public MainWindow VerifyConnecting()
        {
            UIActions.CheckIfObjectWithIdIsDisplayed("ConnectingScreenId");
            return this;
        }

        public MainWindow EnableSecureCore()
        {
            UIActions.ClickOnObjectWithId("SecureCoreCheckbox");
            return this;
        }
    }
}
