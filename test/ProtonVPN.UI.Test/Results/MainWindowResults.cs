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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Results
{
    public class MainWindowResults : UIActions
    {

        public MainWindowResults VerifyUserIsLoggedIn()
        {
            CheckIfObjectWithIdIsDisplayed("MenuHamburgerButton", "User was not logged in.");
            return this;
        }
        public MainWindowResults CheckIfSidebarModeIsEnabled()
        {
            CheckIfObjectIsNotDisplayed("Logo", "Failed to enable sidebar mode");
            return this;
        }

        public MainWindowResults CheckIfConnectButtonIsNotDisplayed()
        {
            Assert.IsFalse(CheckIfElementExistsByName("CONNECT"));
            return this;
        }

        public MainWindowResults CheckIfUpgradeRequiredModalIsShown(string serverName)
        {
            CheckIfObjectWithNameIsDisplayed("When you upgrade to Plus", "Free User is able to connect to " + serverName + " server");
            CheckIfObjectWithNameIsDisplayed("Upgrade", "Free User is able to connect to " + serverName + " server");
            return this;
        }
    }
}
