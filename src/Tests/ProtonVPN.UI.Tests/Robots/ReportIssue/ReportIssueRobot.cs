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

namespace ProtonVPN.UI.Tests.Robots.ReportIssue;

public partial class ReportIssueRobot : UIActions
{

    protected AutomationElement ReportIssueWindowTitle => ElementByAutomationId("WindowTitleLabel").AsLabel();
    protected TitleBar ReportIssueTitleBar => ElementByAutomationId("TitleBar").AsTitleBar();
    protected Button MinimizeButton => ElementByName("Minimize").AsButton();
    protected Button CloseButton => ElementByName("Close").AsButton();
    protected Label StepsControlHeader => ElementByAutomationId("StepsControlHeader").AsLabel();
    protected ProgressBar StepsControlProgressBar => ElementByAutomationId("StepsControlProgressBar").AsProgressBar();    
    protected Label CategorySelectionPageHeader => ElementByAutomationId("CategorySelectionPageHeader").AsLabel();
    protected Label QuickFixesPageHeader => ElementByAutomationId("QuickFixesPageHeader").AsLabel();
    protected Label QuickFixesPageDescription => ElementByAutomationId("QuickFixesPageDescription").AsLabel();
    protected Button IssueCategorySettingsCard => ElementByAutomationId("IssueCategorySettingsCard").AsButton();
    protected Button ContactUsButton => ElementByAutomationId("ContactUsButton").AsButton();    
    protected Button SendReportButton => ElementByAutomationId("SendReportButton").AsButton();
    protected Button MoveBackwardButton => ElementByAutomationId("MoveBackwardButton").AsButton();
    protected TextBox EmailTextBox => ElementByName("Email").AsTextBox();
    protected TextBox BugReportQuestionTextBox(string question)
    {
        return ElementByName(question).AsTextBox();
    }
}