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
using OpenQA.Selenium.Interactions;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Windows
{
    public class BugReportWindow : UIActions
    {

        public BugReportWindow SelectIssue(string issueType)
        {
            ClickOnObjectWithName(issueType);
            return this;
        }

        public BugReportWindow PressContactUs()
        {
            ClickOnObjectWithName("Contact us");
            return this;
        }

        public BugReportWindow FillBugReportForm(string text, int amountOfFieldsToFill)
        {
            Actions actions = new Actions(Session);
            ClickOnObjectWithName("This email will be used to contact you regarding this issue.");
            actions.SendKeys("test@protonmail.com");
            for(int i = 0; i < amountOfFieldsToFill; i++)
            {
                actions.SendKeys(Keys.Tab);
                actions.SendKeys(text);
            }
            actions.Perform();
            return this;
        }

        public BugReportWindow ClickSend()
        {
            ClickOnObjectWithName("Send report");
            return this;
        }

        public BugReportWindow VerifySendingIsSuccessful()
        {
            WaitUntilDisplayed(By.ClassName("Thanks"), 20);
            CheckIfObjectWithNameIsDisplayed("We’ll get back to you as soon as we can.", "The Bug Report was unsuccessfully sent.");
            CheckIfObjectWithNameIsDisplayed("Done", "The Bug Report was unsuccessfully sent.");
            return this;
        }
    }
}
