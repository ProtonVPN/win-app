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

using OpenQA.Selenium;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Windows
{
    public class HamburgerMenu : UIActions
    {
        public HamburgerMenu ClickAccount()
        {
            ClickOnObjectWithId("MenuAccountButton");
            return this;
        }

        public HamburgerMenu ClickLogout()
        {
            ClickOnObjectWithId("MenuLogoutButton");
            return this;
        }

        public HamburgerMenu ClickReportBug()
        {
            WaitUntilDisplayed(By.Name("Report an Issue"), 10);
            ClickOnObjectWithId("MenuReportBugButton");
            return this;
        }

        public HamburgerMenu ClickSettings()
        {
            ClickOnObjectWithId("MenuSettingsButton");
            return this;
        }

        public HamburgerMenu ClickProfiles()
        {
            ClickOnObjectWithId("MenuProfilesButton");
            return this;
        }

        public HamburgerMenu ClickExit()
        {
            ClickOnObjectWithName("Exit");
            return this;
        }
    }
}
