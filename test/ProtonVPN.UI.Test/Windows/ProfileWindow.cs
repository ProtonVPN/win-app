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

using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Windows
{
    public class ProfileWindow : UIActions
    {
        public ProfileWindow ClickToCreateNewProfile()
        {
            ClickOnObjectWithId("NewProfileButton");
            return this;
        }

        public ProfileWindow EnterProfileName(string text)
        {
            InsertTextIntoFieldWithId("textSource", text);
            return this;
        }

        public ProfileWindow ClickSaveButton()
        {
            ClickOnObjectWithName("Save");
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
                        MoveToElement(childBox);
                        box.SendKeys(Keys.Enter);
                        break;
                    }
                }
            }

            return this;
        }

        public ProfileWindow ClickSecureCore()
        {
            ClickOnObjectWithName("Secure Core");
            return this;
        }

        public ProfileWindow ClickP2P()
        {
            ClickOnObjectWithName("P2P");
            return this;
        }

        public ProfileWindow ClickTor()
        {
            ClickOnObjectWithName("Tor");
            return this;
        }

        public ProfileWindow ClickToDiscard()
        {
            ClickOnObjectWithName("Discard");
            return this;
        }

        public ProfileWindow ClickToCancel()
        {
            ClickOnObjectWithXPath("//*[@Name = 'Cancel']");
            return this;
        }

        public ProfileWindow DeleteProfileByByName(string name)
        {
            Thread.Sleep(3000);
            ClickOnObjectWithId($"Delete-{name}");
            return this;
        }

        public ProfileWindow ClickContinueDeletion()
        {
            ClickOnObjectWithId("ContinueButton");
            return this;
        }

        public ProfileWindow ClickEditProfile(string name)
        {
            Thread.Sleep(3000);
            var buttonEdit = Session.FindElementByAccessibilityId($"Edit-{name}");
            MoveToElement(buttonEdit);
            ClickOnObjectWithId($"Edit-{name}");
            return this;
        }

        public ProfileWindow ClearProfileName()
        {
            ClearInput("textSource");
            return this;
        }
    }
}
