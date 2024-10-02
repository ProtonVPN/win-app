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

using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.UI.Dialogs.ReportIssue.Bases;

namespace ProtonVPN.Client.UI.Dialogs.ReportIssue.Pages;

public partial class ReportIssueResultPageViewModel : ReportIssuePageViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    private bool _isReportSent;

    public string Header => IsReportSent
        ? Localizer.Get("Dialogs_ReportIssue_Result_Success")
        : Localizer.Get("Dialogs_ReportIssue_Result_Fail");

    public ReportIssueResultPageViewModel(
        IReportIssueViewNavigator parentViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(parentViewNavigator, localizer, logger, issueReporter)
    { }

    public override void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        base.OnNavigatedTo(parameter, isBackNavigation);

        IsReportSent = Convert.ToBoolean(parameter);
    }
}