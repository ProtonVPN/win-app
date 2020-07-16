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
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Results
{
    public class ProfileResult : UIActions
    {
        public ProfileResult VerifyWindowIsOpened()
        {
            CheckIfObjectWithNameIsDisplayed("Fastest", "Profiles window was not opened or missing Fastest profile option.");
            CheckIfObjectWithNameIsDisplayed("Random", "Profiles window was not opened or missing Random profile option.");
            return this;
        }

        public ProfileResult VerifyProfileNameErrorDisplayed()
        {
            CheckIfObjectWithNameIsDisplayed("Please select a profile name", "Profile name error was not displayed.");
            return this;
        }

        public ProfileResult VerifyCountryErrorDisplayed()
        {
            CheckIfObjectWithNameIsDisplayed("Please select a country", "Country error was not displayed.");
            return this;
        }

        public ProfileResult VerifyProfileExists(string profileName)
        {
            WaitUntilDisplayed(By.Name(profileName), 5);
            CheckIfObjectWithNameIsDisplayed(profileName, "Profile " + profileName + " does not exist.");
            return this;
        }

        public ProfileResult VerifyProfileDoesNotExist(string profileName)
        {
            CheckIfObjectWithAutomationIdDoesNotExist(profileName, "Expected profile to not exist, but it exists.");
            return this;
        }
    }
}
