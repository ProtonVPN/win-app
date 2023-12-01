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

using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Results
{
    public class ProfilesResult : UIActions
    {
        public ProfilesResult CheckIfProfileIsDisplayed(string profileName)
        {
            return WaitUntilElementExistsByName(profileName, TestData.VeryShortTimeout);
        }

        public ProfilesResult CheckIfProfileCreationErrorIsNotShown()
        {
            return CheckIfDoesNotExistByClassName("FormErrorsPanel");
        }

        public ProfilesResult CheckIfDefaultProfilesAreDisplayed()
        {
            WaitUntilElementExistsByName("Fastest", TestData.VeryShortTimeout);
            WaitUntilElementExistsByName("Random", TestData.VeryShortTimeout);
            return this;
        }

        public ProfilesResult CheckIfProfileNameErrorDisplayed()
        {
            CheckIfDisplayedByName("Please select a profile name");
            return this;
        }

        public ProfilesResult CheckIfCountryErrorDisplayed()
        {
            CheckIfDisplayedByName("Please select a country");
            return this;
        }

        public ProfilesResult CheckIfProfileIsNotDisplayed(string profile)
        {
            CheckIfDoesNotExistByAutomationId(profile);
            return this;
        }
    }
}
