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

using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.UI.ReportIssue;
using ProtonVPN.Client.Legacy.UI.ReportIssue.Models;

namespace ProtonVPN.Client.Legacy.Models.Activation.Custom;

public class ReportIssueDialogActivator : IReportIssueDialogActivator
{
    private readonly IDialogActivator _dialogActivator;
    private readonly IReportIssueViewNavigator _viewNavigator;

    public ReportIssueDialogActivator(
        IDialogActivator dialogActivator,
        IReportIssueViewNavigator viewNavigator)
    {
        _dialogActivator = dialogActivator;
        _viewNavigator = viewNavigator;
    }

    public async Task ShowDialogAsync()
    {
        _dialogActivator.ShowDialog<ReportIssueShellViewModel>();

        await _viewNavigator.NavigateToCategorySelectionAsync();
    }

    public async Task ShowDialogAsync(IssueCategory category)
    {
        _dialogActivator.ShowDialog<ReportIssueShellViewModel>();

        await _viewNavigator.NavigateToCategoryAsync(category);
    }

    public void CloseDialog()
    {
        _dialogActivator.CloseDialog<ReportIssueShellViewModel>();
    }
}