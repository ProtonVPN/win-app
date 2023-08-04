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

using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots.ReportIssue;

public partial class ReportIssueRobot : UIActions
{
    protected Window ReportIssueWindow { get; private set; }

    protected AutomationElement ReportIssueWindowTitle => ReportIssueWindow.ElementByAutomationId("WindowTitleLabel").AsLabel();
    protected TitleBar ReportIssueTitleBar => ReportIssueWindow.ElementByAutomationId("TitleBar").AsTitleBar();
    protected Button MinimizeButton => ReportIssueTitleBar.FindFirstDescendant(c => c.ByName("Minimize")).AsButton();
    protected Button CloseButton => ReportIssueTitleBar.FindFirstDescendant(c => c.ByName("Close")).AsButton();

    protected Label StepsControlHeader => ReportIssueWindow.ElementByAutomationId("StepsControlHeader").AsLabel();
    protected ProgressBar StepsControlProgressBar => ReportIssueWindow.ElementByAutomationId("StepsControlProgressBar").AsProgressBar();    
    protected Label CategorySelectionPageHeader => ReportIssueWindow.ElementByAutomationId("CategorySelectionPageHeader").AsLabel();
    protected Label QuickFixesPageHeader => ReportIssueWindow.ElementByAutomationId("QuickFixesPageHeader").AsLabel();
    protected Label QuickFixesPageDescription => ReportIssueWindow.ElementByAutomationId("QuickFixesPageDescription").AsLabel();
    protected Button IssueCategorySettingsCard => ReportIssueWindow.ElementByAutomationId("IssueCategorySettingsCard").AsButton();
    protected Button ContactUsButton => ReportIssueWindow.ElementByAutomationId("ContactUsButton").AsButton();    
    protected Button SendReportButton => ReportIssueWindow.ElementByAutomationId("SendReportButton").AsButton();
    protected Button MoveBackwardButton => ReportIssueWindow.ElementByAutomationId("MoveBackwardButton").AsButton();

    public ReportIssueRobot RefreshReportIssueWindow()
    {
        this.Wait(TestConstants.InitializationDelay);

        ReportIssueWindow = App.GetAllTopLevelWindows(new UIA3Automation()).FirstOrDefault(w => Window != w);

        return this;
    }
}