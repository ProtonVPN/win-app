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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.UI.Dialogs.ReportIssue.Bases;

namespace ProtonVPN.Client.UI.Dialogs.ReportIssue.Pages;

public partial class ReportIssueResultPageViewModel : ReportIssuePageViewModelBase
{
    private readonly IReportIssueWindowActivator _reportIssueWindowActivator;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    [NotifyPropertyChangedFor(nameof(Description))]
    [NotifyCanExecuteChangedFor(nameof(CloseCommand))]
    [NotifyCanExecuteChangedFor(nameof(RetryCommand))]
    private bool _isReportSent;

    public string Header => IsReportSent
        ? Localizer.Get("Dialogs_ReportIssue_Result_Success")
        : Localizer.Get("Dialogs_ReportIssue_Result_Fail");

    public string Description => IsReportSent
        ? Localizer.Get("Dialogs_ReportIssue_Result_Success_Description")
        : Localizer.Get("Dialogs_ReportIssue_Result_Fail_Description");

    public ReportIssueResultPageViewModel(
        IReportIssueWindowActivator reportIssueWindowActivator,
        IReportIssueViewNavigator parentViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, viewModelHelper)
    {
        _reportIssueWindowActivator = reportIssueWindowActivator;
    }

    public override void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        base.OnNavigatedTo(parameter, isBackNavigation);

        IsReportSent = Convert.ToBoolean(parameter);
    }

    [RelayCommand(CanExecute = nameof(CanClose))]
    public void Close()
    {
        _reportIssueWindowActivator.Hide();
    }

    public bool CanClose()
    {
        return IsReportSent;
    }

    [RelayCommand(CanExecute = nameof(CanRetry))]
    public async Task RetryAsync()
    {
        await ParentViewNavigator.GoBackAsync();
    }

    public bool CanRetry()
    {
        return !IsReportSent;
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(Description));
    }
}