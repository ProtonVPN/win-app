/*
 * Copyright (c) 2024 Proton AG
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
using System.Threading;
using FlaUI.Core.AutomationElements;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class SupportRobot
{
    private readonly Func<Window> _windowFunc;

    protected Element ContactUsButton => Element.ByName("Contact us");
    protected Element SendReportButton => Element.ByAutomationId("SendReportButton");
    protected Element ReportSentLabel => Element.ByName("Report sent");
    protected Element NoLogsAttachedWarning => Element.ByAutomationId("Message");
    protected Element IncludeLogsCheckbox => Element.ByAutomationId("IncludeLogsCheckbox");
    protected Element EmailInputField => Element.ByAutomationId("EmailInputField");

    public SupportRobot(Func<Window> windowFunc)
    {
        _windowFunc = windowFunc;
    }

    public SupportRobot FillBugReportForm(string bugType)
    {
        // Allow some time for the framework to process UI.
        // Elements were found, but clicks were performed too early.
        Thread.Sleep(TestConstants.OneSecondTimeout);

        Element
            .ByName(bugType)
            .WaitUntilExists(TestConstants.FiveSecondsTimeout)
            .DoubleClick();

        ContactUsButton.Invoke();
        EmailInputField.WaitUntilDisplayed();
        AutomationElement[] bugReportInputFields = _windowFunc().FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit));
        bugReportInputFields[0].AsTextBox().Text = "testing@email.com";

        for (int i = 1; i < bugReportInputFields.Length; i++)
        {
            bugReportInputFields[i].AsTextBox().Text = "Ignore report. Testing";
        }
        return this;
    }

    public SupportRobot SendBugReport()
    {
        SendReportButton.Click();
        return this;
    }

    public SupportRobot TickIncludeLogsCheckbox()
    {
        IncludeLogsCheckbox.ScrollIntoView().Click();
        return this;
    }

    public class Verifications : SupportRobot
    {
        public Verifications(Func<Window> windowFunc) : base(windowFunc)
        {
        }

        public Verifications IsSendingSuccessful()
        {
            ReportSentLabel.WaitUntilExists(TestConstants.ThirtySecondsTimeout);
            return this;
        }

        public Verifications IsNoLogsAttachedWarningDisplayed()
        {
            NoLogsAttachedWarning.WaitUntilDisplayed();
            return this;
        }
    }

    public Verifications Verify => new(_windowFunc);
}