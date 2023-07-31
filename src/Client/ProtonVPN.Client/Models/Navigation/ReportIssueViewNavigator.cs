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

using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.UI.ReportIssue.Models;
using ProtonVPN.Client.UI.ReportIssue.Results;
using ProtonVPN.Client.UI.ReportIssue.Steps;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Models.Navigation;

public class ReportIssueViewNavigator : ViewNavigatorBase, IReportIssueViewNavigator
{
    public ReportIssueViewNavigator(ILogger logger, IViewMapper viewMapper, IThemeSelector themeSelector)
        : base(logger, viewMapper, themeSelector)
    {
    }

    public void NavigateToCategory(IssueCategory category)
    {
        if (category == null)
        {
            throw new ArgumentNullException(nameof(category), "Cannot navigate to the category page, no category defined");
        }

        if (category.Suggestions.Any())
        {
            NavigateTo<QuickFixesViewModel>(category);
        }
        else
        {
            NavigateTo<ContactFormViewModel>(category);
        }

        // This method can be called by the help component to jump from one category to another. 
        // Clear back stack so the back button does bring to the category selection page
        Frame?.BackStack.Clear();
    }
    
    public void NavigateToResult(bool isReportSent)
    {
        NavigateTo<ReportIssueResultViewModel>(isReportSent);
    }
}