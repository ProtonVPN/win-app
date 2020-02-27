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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProtonVPN.UI.Test.TestsHelper
{
    public class UIActions : UITestSession
    {
      
        public static void InsertTextIntoFieldWithId(string objectId, string text)
        {
            var field = Session.FindElementByAccessibilityId(objectId);
            field.SendKeys(text);
        }

        public static void ClearInput(string id)
        {
            Session.FindElementByAccessibilityId(id).Clear();
        }

        public static void InsertTextIntoFieldWithName(string objectName, string text)
        {
            var field = Session.FindElementByName(objectName);
            field.SendKeys(text);
        }

        public static void ClickOnObjectWithId(string objectId)
        {
            var button = Session.FindElementByAccessibilityId(objectId);
            button.Click();
        }

        public static void ClickOnObjectWithName(string objectName)
        {

            var button = Session.FindElementByName(objectName);
            button.Click();
        }

        public static void ClickOnObjectWithXPath(string objectpath)
        {
            var button = Session.FindElementsByXPath(objectpath);
            button[0].Click();
        }

        public static void CheckIfObjectWithIdIsDisplayed(string objectId)
        {
            var content = Session.FindElementByAccessibilityId(objectId).Displayed;
            Assert.IsTrue(content);
        }

        public static void CheckIfObjectWithAutomationIdDoesNotExist(string id)
        {
            var exists = false;
            try
            {
                Session.FindElementByAccessibilityId(id);
                exists = true;
            }
            catch (Exception)
            {
                // Ignored
            }

            Assert.IsFalse(exists);
        }

        public static void CheckIfObjectWithNameIsDisplayed(string objectName)
        {
            var content = Session.FindElementByName(objectName).Displayed;
            Assert.IsTrue(content);
        }

        public static void CheckIfObjectWithNameIsNotDisplayed(string objectName)
        {
            var elements = Session.FindElementsByName(objectName);
            Assert.IsTrue(elements.Count == 0);
        }

        public void CloseWindow()
        {
            var close = Session.FindElementsByXPath("//*[@AutomationId = 'CloseButton']");
            close[0].Click();
        }

        public static void CloseProfilesWindow()
        {
            var close = Session.FindElementByName("Close");
            close.Click();
        }

        public static void ClickHamburgerMenu()
        {
            ClickOnObjectWithId("MenuHamburgerButton");
        }
    }
}
