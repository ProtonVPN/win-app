/*
 * Copyright (c) 2025 Proton AG
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
using System.Linq;
using System.Threading;
using FlaUI.Core.Definitions;
using FlaUI.Core.AutomationElements;
using ProtonVPN.UI.Tests.UiTools;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots;

public class SupportRobot
{
    private readonly Func<Window?> _windowFunc;

    private static readonly string _connectionHelpHeaderTranslated = LanguageHelper.GetTranslatedString("Dialogs_Troubleshooting_Title");
    private static readonly string _reportSentLabelTranslated = LanguageHelper.GetTranslatedString("Dialogs_ReportIssue_Result_Success");

    protected Element ConnectionHelpHeader => Element.ByName(_connectionHelpHeaderTranslated);
    protected Element ReportSentLabel => Element.ByName(_reportSentLabelTranslated);
    protected Element ContactUsButton => Element.ByAutomationId("ContactUsButton");
    protected Element SendReportButton => Element.ByAutomationId("SendReportButton");
    protected Element NoLogsAttachedWarning => Element.ByAutomationId("Message");
    protected Element IncludeLogsCheckbox => Element.ByAutomationId("IncludeLogsCheckbox");
    public Element EmailInputField => Element.ByAutomationId("EmailInputField");
    protected Element DoneButton => Element.ByAutomationId("ReportIssueCloseButton");
    protected Element CloseButton => Element.ByAutomationId("Close");

    public SupportRobot(Func<Window?> windowFunc)
    {
        _windowFunc = windowFunc;
    }

    public SupportRobot FillEmailInBugReportForm(string emailField = "testing@email.com")
    {
        EmailInputField.WaitUntilDisplayed();
        EmailInputField.SetText(emailField);
        return this;
    }

    public SupportRobot FillOtherFieldsInBugReportForm(string otherFields = "Ignore report. Testing")
    {
        foreach (AutomationElement field in GetOtherFieldsElements())
        {
            field.Patterns.ScrollItem.Pattern.ScrollIntoView();
            TextBox textBox = field.AsTextBox();
            if (textBox is not null)
            {
                textBox.Text = otherFields;
            }
        }
        return this;
    }

    public SupportRobot SelectBugType(string bugType)
    {
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        Element.ByName(bugType).ClickUntilElementDisappears();
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        return this;
    }

    public SupportRobot ClickContactUs()
    {
        Thread.Sleep(TestConstants.NavigationDelay);
        ContactUsButton.Invoke();
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

    public SupportRobot CloseSupportWindow()
    {
        CloseButton.Click();
        return this;
    }

    public SupportRobot ScrollUpAndDown()
    {
        EmailInputField.ScrollIntoView();
        Thread.Sleep(TestConstants.OneSecondTimeout);
        IncludeLogsCheckbox.ScrollIntoView();
        Thread.Sleep(TestConstants.OneSecondTimeout);
        EmailInputField.ScrollIntoView();
        Thread.Sleep(TestConstants.OneSecondTimeout);
        IncludeLogsCheckbox.ScrollIntoView();
        Thread.Sleep(TestConstants.OneSecondTimeout);
        EmailInputField.ScrollIntoView();
        return this;
    }

    public class Verifications : SupportRobot
    {
        public Verifications(Func<Window?> windowFunc) : base(windowFunc)
        {
        }

        public Verifications IsSendingSuccessful()
        {
            ReportSentLabel.WaitUntilExists(TestConstants.ThirtySecondsTimeout);
            DoneButton.Click();
            Thread.Sleep(TestConstants.NavigationDelay);
            return this;
        }

        public Verifications IsConnectionHelpDisplayed(TimeSpan? timeout = null)
        {
            timeout ??= TestConstants.TwoMinutesTimeout;
            ConnectionHelpHeader.WaitUntilExists(timeout);
            return this;
        }

        public Verifications IsNoLogsAttachedWarningDisplayed()
        {
            NoLogsAttachedWarning.WaitUntilDisplayed();
            return this;
        }
    }

    public AutomationElement[] GetOtherFieldsElements()
    {
        AutomationElement[]? allEditFields = _windowFunc()?.FindAllDescendants(cf => cf.ByControlType(ControlType.Edit));

        if (allEditFields is null || allEditFields.Length == 0)
        {
            throw new Exception("Could not find input fields for bug report.");
        }

        return allEditFields.Where(e => e.Properties.AutomationId.ValueOrDefault != "EmailInputField").ToArray();
    }

    public Verifications Verify => new(_windowFunc);
}