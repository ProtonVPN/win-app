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

using System.Collections.Generic;

namespace ProtonVPN.UI.Tests.Robots.ReportIssue;

public partial class ReportIssueRobot
{
    public ReportIssueRobot DoSelectBrowsingSpeedCategory()
    {
        IssueCategorySettingsCard.FocusAndClick();
        return this;
    }

    public ReportIssueRobot DoGoToContactForm()
    {
        ContactUsButton.FocusAndClick();
        return this;
    }

    public ReportIssueRobot DoGoBack()
    {
        MoveBackwardButton.FocusAndClick();
        return this;
    }

    public ReportIssueRobot DoSendReport()
    {
        SendReportButton.FocusAndClick();
        return this;
    }

    public ReportIssueRobot DoFillData(string email, List<string> _questions)
    {
        EmailTextBox.Text = email;
        foreach (string question in _questions)
        {
            BugReportQuestionTextBox(question).Text = "Ignore this report";
        }
        return this;
    }
}