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


using FlaUI.Core.AutomationElements;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Windows
{
    public class BugReportWindow : UIActions
    {
        private AutomationElement ContactUsButton => ElementByName("Contact us");
        private AutomationElement SendReportButton => ElementByName("Send report");

        public BugReportWindow FillBugReportForm(string bugType)
        {
            WaitUntilElementExistsByName(bugType, TestConstants.VeryShortTimeout);
            ElementByName(bugType).Click();
            ContactUsButton.Click();
            AutomationElement[] bugReportInputFields = Window.FindAllDescendants(cf => cf.ByAutomationId("AdornedTextBox"));
            bugReportInputFields[0].AsTextBox().Text = "testing@email.com";
            bugReportInputFields[1].AsTextBox().Text = "testing@email.com";
            for (int i = 2; i < bugReportInputFields.Length; i++)
            {
                bugReportInputFields[i].AsTextBox().Text = "Ignore report. Testing";
            }
            SendReportButton.Click();
            return this;
        }

        public BugReportWindow VerifySendingIsSuccessful()
        {
            WaitUntilElementExistsByClassName("Thanks", TestConstants.MediumTimeout);
            CheckIfDisplayedByName("We’ll get back to you as soon as we can.");
            CheckIfDisplayedByName("Done");
            return this;
        }
    }
}
