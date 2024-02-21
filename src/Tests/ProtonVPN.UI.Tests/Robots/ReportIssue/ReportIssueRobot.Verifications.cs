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
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots.ReportIssue;

public partial class ReportIssueRobot
{
    private const string STEP_1_PAGE_HEADER = "What's the issue?";
    private const string STEP_2_PAGE_HEADER = "Quick fixes";
    private const string STEP_2_PAGE_DESCRIPTION = "These tips could help to solve your issue faster.";

    public ReportIssueRobot VerifyReportIssueWindowIsOpened()
    {
        Assert.IsNotNull(ReportIssueTitleBar);
        Assert.IsNotNull(MinimizeButton);
        Assert.IsNotNull(CloseButton);
        return this;
    }

    public ReportIssueRobot VerifyReportIssueWindowTitle(string title)
    {
        Assert.AreEqual(title, ReportIssueWindowTitle.Name);
        return this;
    }

    public ReportIssueRobot VerifyReportIssueStep(int expectedStep, int expectedTotalSteps = 3)
    {
        VerifyPageContent(expectedStep);

        string expectedStepHeader = $"Step {expectedStep} of {expectedTotalSteps}";

        Label stepsControlHeader = StepsControlHeader;

        Assert.IsNotNull(stepsControlHeader);
        Assert.AreEqual(expectedStepHeader, stepsControlHeader.Text);

        ProgressBar stepsControlProgressBar = StepsControlProgressBar;

        Assert.IsNotNull(stepsControlProgressBar);
        Assert.AreEqual(expectedStep, stepsControlProgressBar.Value);
        Assert.AreEqual(expectedTotalSteps, stepsControlProgressBar.Maximum);

        return this;
    }

    public ReportIssueRobot VerifyReportIsSent()
    {
        WaitUntilElementExistsByAutomationId("ReportImage", TestConstants.MediumTimeout);
        WaitUntilElementExistsByName("Report sent", TestConstants.VeryShortTimeout);
        WaitUntilElementExistsByName("Done", TestConstants.VeryShortTimeout);
        return this;
    }

    private void VerifyPageContent(int step)
    {
        switch (step)
        {
            case 1:
                Assert.AreEqual(STEP_1_PAGE_HEADER, CategorySelectionPageHeader.Text);
                break;

            case 2:
                Assert.AreEqual(STEP_2_PAGE_HEADER, QuickFixesPageHeader.Text);
                Assert.AreEqual(STEP_2_PAGE_DESCRIPTION, QuickFixesPageDescription.Text);
                Assert.IsNotNull(ContactUsButton);
                break;

            case 3:
                Assert.IsNotNull(SendReportButton);
                break;

            default:
                throw new AssertionException($"Step '{step}' is out of range");
        }
    }
}