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

using System.Threading.Tasks;
using ProtonVPN.Client.Core.Models.ReportIssue;
using ProtonVPN.Client.Core.Services.Navigation.Bases;

namespace ProtonVPN.Client.Core.Services.Navigation;

public interface IReportIssueViewNavigator : IViewNavigator
{
    Task<bool> NavigateToCategoriesViewAsync();

    Task<bool> NavigateToCategoryViewAsync(IssueCategory category);

    Task<bool> NavigateToContactViewAsync(IssueCategory category);

    Task<bool> NavigateToResultViewAsync(bool isReportSent);
}