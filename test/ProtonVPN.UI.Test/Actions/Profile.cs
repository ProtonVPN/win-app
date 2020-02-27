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

using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Actions
{
    public class Profile : UIActions
    {
        public static void SelectProfiles()
        {
            ClickOnObjectWithId("MenuProfilesButton");
        }

        public static void ClickToCreateNewProfile()
        {
            ClickOnObjectWithId("NewProfileButton");
        }

        public static void EnterProfileName(string text)
        {
            InsertTextIntoFieldWithId("textSource", text);
        }

        public static void ClickSaveButton()
        {
            ClickOnObjectWithName("Save");
        }

        public void SelectCountryFromList(string country)
        {
            var countrySelect = Session.FindElementsByAccessibilityId("CountryBox");
            countrySelect.First().Click();

            var listBoxes = Session.FindElementsByClassName("ListBoxItem");
            var boxes = new List<WindowsElement>();

            foreach (var box in listBoxes)
            {
                if (box.Size.Height != 0 && box.Text == "ProtonVPN.Profiles.Form.CountryViewModel" && box.Displayed)
                {
                    var childBox = box.FindElementByClassName("TextBlock");
                    if (childBox.Text == country)
                        boxes.Add(box);
                }
            }

            boxes[0].SendKeys(Keys.Enter);
        }

        public void SelectServerFromList(string server)
        {
            var serverSelect = Session.FindElementsByAccessibilityId("ServerBox");
            serverSelect.First().Click();

            var listBoxes = Session.FindElementsByClassName("ListBoxItem");
            var boxes = new List<WindowsElement>();

            foreach (var box in listBoxes)
            {
                if (box.Text == "ProtonVPN.Profiles.Servers.ServerViewModel" && box.Displayed)
                {
                    var childBox = box.FindElementByClassName("TextBlock");
                    if (childBox.Text == server)
                        boxes.Add(box);
                }
            }

            boxes[0].SendKeys(Keys.Enter);
        }

        public static void ClickSecureCore()
        {
            ClickOnObjectWithName("Secure Core");
        }

        public static void ClickP2P()
        {
            ClickOnObjectWithName("P2P");
        }

        public static void ClickTor()
        {
            ClickOnObjectWithName("Tor");
        }

        public static void ClickToDiscard()
        {
            ClickOnObjectWithName("Discard");
        }

        public static void ClickToCancel()
        {
            ClickOnObjectWithXPath("//*[@Name = 'Cancel']");
        }

        public static void ClickDeleteProfile()
        {
            ClickOnObjectWithName("Delete");

        }
        public static void ClickContinueDeletion()
        {
            ClickOnObjectWithId("ContinueButton");
        }

        public static void ClickEditProfile()
        {
            ClickOnObjectWithName("Edit");
        }

        public static void ClicktoConnect()
        {
            ClickOnObjectWithXPath("//*[@Name = 'Connect'][3]");
        }

        public static void ClickSetAsProfile()
        {
            ClickOnObjectWithName("Set as profile");
        }
    }
}
