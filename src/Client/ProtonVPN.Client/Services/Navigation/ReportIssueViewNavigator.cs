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

using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Core.Services.Mapping;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Navigation.Bases;
using ProtonVPN.Client.UI.Dialogs.ReportIssue.Pages;
using ProtonVPN.Client.Core.Models.ReportIssue;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Common.Dispatching;

namespace ProtonVPN.Client.Services.Navigation;

public class ReportIssueViewNavigator : ViewNavigatorBase, IReportIssueViewNavigator
{
    public override bool IsNavigationStackEnabled => true;

    public override FrameLoadedBehavior LoadBehavior { get; protected set; } = FrameLoadedBehavior.NavigateToDefaultViewIfEmpty;

    public ReportIssueViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher)
        : base(logger, pageViewMapper, uiThreadDispatcher)
    { }

    public Task<bool> NavigateToCategoriesViewAsync()
    {
        return NavigateToAsync<ReportIssueCategoriesPageViewModel>();
    }

    public async Task<bool> NavigateToCategoryViewAsync(IssueCategory category)
    {
        bool navigated = category.Suggestions.Any()
            ? await NavigateToAsync<ReportIssueCategoryPageViewModel>(category)
            : await NavigateToContactViewAsync(category);

        // Clear back stack so the back button brings to the category selection page
        ClearBackStack();

        return navigated;
    }

    public Task<bool> NavigateToContactViewAsync(IssueCategory category)
    {
        return NavigateToAsync<ReportIssueContactPageViewModel>(category);
    }

    public Task<bool> NavigateToResultViewAsync(bool isReportSent)
    {
        return NavigateToAsync<ReportIssueResultPageViewModel>(isReportSent);
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return NavigateToCategoriesViewAsync();
    }
}