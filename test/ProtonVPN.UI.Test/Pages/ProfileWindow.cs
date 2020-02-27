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

using System.Linq;
using OpenQA.Selenium;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Pages
{
    public class ProfileWindow : UITestSession
    {
        public ProfileWindow VerifyWindowIsOpened()
        {
            UIActions.CheckIfObjectWithNameIsDisplayed("Fastest");
            UIActions.CheckIfObjectWithNameIsDisplayed("Random");
            return this;
        }

        public ProfileWindow ClickToCreateNewProfile()
        {
            UIActions.ClickOnObjectWithId("NewProfileButton");
            return this;
        }

        public ProfileWindow EnterProfileName(string text)
        {
            UIActions.InsertTextIntoFieldWithId("textSource", text);
            return this;
        }

        public ProfileWindow ClickSaveButton()
        {
            UIActions.ClickOnObjectWithName("Save");
            return this;
        }

        public ProfileWindow SelectCountryFromList(string country)
        {
            var countrySelect = Session.FindElementsByAccessibilityId("CountryBox");
            countrySelect.First().Click();

            var listBoxes = Session.FindElementsByClassName("ListBoxItem");
            foreach (var box in listBoxes)
            {
                if (box.Size.Height != 0 && box.Text == "ProtonVPN.Profiles.Form.CountryViewModel" && box.Displayed)
                {
                    var childBox = box.FindElementByClassName("TextBlock");
                    if (childBox.Text == country)
                    {
                        box.SendKeys(Keys.Enter);
                        break;
                    }
                }
            }

            return this;
        }


        public ProfileWindow SelectServerFromList(string server)
        {
            var serverSelect = Session.FindElementsByAccessibilityId("ServerBox");
            serverSelect.First().Click();

            var listBoxes = Session.FindElementsByClassName("ListBoxItem");
            foreach (var box in listBoxes)
            {
                if (box.Text == "ProtonVPN.Profiles.Servers.ServerViewModel" && box.Displayed)
                {
                    var childBox = box.FindElementByClassName("TextBlock");
                    if (childBox.Text == server)
                    {
                        box.SendKeys(Keys.Enter);
                        break;
                    }
                }
            }

            return this;
        }

        public ProfileWindow ClickSecureCore()
        {
            UIActions.ClickOnObjectWithName("Secure Core");
            return this;
        }

        public ProfileWindow ClickP2P()
        {
            UIActions.ClickOnObjectWithName("P2P");
            return this;
        }

        public ProfileWindow ConnectToProfile(string name)
        {
            UIActions.ClickOnObjectWithId($"Connect-{name}");
            return this;
        }

        public ProfileWindow ClickTor()
        {
            UIActions.ClickOnObjectWithName("Tor");
            return this;
        }

        public ProfileWindow ClickToDiscard()
        {
            UIActions.ClickOnObjectWithName("Discard");
            return this;
        }

        public ProfileWindow ClickToCancel()
        {
            UIActions.ClickOnObjectWithXPath("//*[@Name = 'Cancel']");
            return this;
        }

        public ProfileWindow DeleteProfileByByName(string name)
        {
            UIActions.ClickOnObjectWithId($"Delete-{name}");
            return this;
        }

        public ProfileWindow ClickContinueDeletion()
        {
            UIActions.ClickOnObjectWithId("ContinueButton");
            return this;
        }

        public ProfileWindow ClickEditProfile(string name)
        {
            UIActions.ClickOnObjectWithId($"Edit-{name}");
            return this;
        }

        public ProfileWindow ClearProfileName()
        {
            UIActions.ClearInput("textSource");
            return this;
        }

        public ProfileWindow ClicktoConnect()
        {
            UIActions.ClickOnObjectWithXPath("//*[@Name = 'Connect'][3]");
            return this;
        }

        public ProfileWindow ClickSetAsProfile()
        {
            UIActions.ClickOnObjectWithName("Set as profile");
            return this;
        }

        public ProfileWindow VerifyProfileNameErrorDisplayed()
        {
            UIActions.CheckIfObjectWithNameIsDisplayed("Please select a profile name");
            return this;
        }

        public ProfileWindow VerifyCountryErrorDisplayed()
        {
            UIActions.CheckIfObjectWithNameIsDisplayed("Please select a country");
            return this;
        }

        public ProfileWindow VerifyServerErrorDisplayed()
        {
            UIActions.CheckIfObjectWithNameIsDisplayed("Please select a server");
            return this;
        }

        public ProfileWindow VerifyProfileExists(string profileName)
        {
            UIActions.CheckIfObjectWithNameIsDisplayed(profileName);
            return this;
        }

        public ProfileWindow VerifyUserIsConnectedToProfile(string status)
        {
            UIActions.CheckIfObjectWithNameIsDisplayed(status);
            return this;
        }

        public ProfileWindow VerifyProfileDoesNotExist(string profileName)
        {
            UIActions.CheckIfObjectWithAutomationIdDoesNotExist(profileName);
            return this;
        }
    }
}
