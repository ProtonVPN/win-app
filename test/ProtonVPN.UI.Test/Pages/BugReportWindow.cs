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
    public class BugReportWindow
    {
        public BugReportWindow EnterYourEmail(string text)
        {
            UIActions.InsertTextIntoFieldWithId("Email", text);
            return this;
        }

        //public static void BugReportWindowIsOpened()
        //{
        //    CheckIfObjectWithNameIsDisplayed("Bug Report");
        //}

        //[TestMethod]
        //public static void ClickEnterEmail()
        //{
        //    ClickOnObjectWithName("Your E-mail Address");
        //}

        public BugReportWindow EnterFeedback(string text)
        {
            UIActions.InsertTextIntoFieldWithId("Feedback", text);
            return this;
        }

        public BugReportWindow ClickSend()
        {
            UIActions.ClickOnObjectWithName("Send");
            return this;
        }

        public BugReportWindow VerifySendingIsSuccessful()
        {
            UIActions.CheckIfObjectWithNameIsDisplayed("The Bug Report has been successfully sent.");
            return this;
        }
    }
}
