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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Actions
{
    [TestClass]
    public class ReportBug : UIActions
    {
        [TestMethod]
        public static void EnterYourEmail(string text)
        {
            InsertTextIntoFieldWithId("Email", text);
        }

        [TestMethod]
        public static void BugReportWindowIsOpened()
        {
            CheckIfObjectWithNameIsDisplayed("Bug Report");
        }

        [TestMethod]
        public static void ClickEnterEmail()
        {
           ClickOnObjectWithName("Your E-mail Address");
        }

        [TestMethod]
        public static void EnterFeedback(string text)
        {
            InsertTextIntoFieldWithId("Feedback", text);
        }

        [TestMethod]
        public static void PressSend()
        {
            ClickOnObjectWithName("Send");
        }

        [TestMethod]
        public static void SendingSuccessful()
        {
            CheckIfObjectWithNameIsDisplayed("The Bug Report has been successfully sent.");
        }
    }
}
