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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Legacy.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.UI.ReportIssue.Steps;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.UI.ReportIssue;

public partial class ReportIssueShellViewModel : ShellViewModelBase<IReportIssueViewNavigator>
{
    public string BaseTitle => Localizer.Get("Dialogs_ReportIssue_Title");

    public override string Title => string.IsNullOrEmpty(CurrentPage?.Title)
        ? BaseTitle
        : $"{BaseTitle} - {CurrentPage.Title}";

    public int TotalSteps => 3;

    public int CurrentStep => CurrentPage switch
    {
        CategorySelectionViewModel => 1,
        QuickFixesViewModel => 2,
        ContactFormViewModel => 3,
        _ => 0
    };

    public string StepsHeader => Localizer.GetFormat("Dialogs_ReportIssue_Steps", CurrentStep, TotalSteps);

    public bool IsHeaderVisible => CurrentStep > 0 && CurrentStep <= TotalSteps;

    public ReportIssueShellViewModel(IReportIssueViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator, localizationProvider, logger, issueReporter)
    { }

    [RelayCommand(CanExecute = nameof(CanNavigateBackward))]
    public async Task NavigateBackwardAsync()
    {
        if (ViewNavigator.CanGoBack)
        {
            await ViewNavigator.GoBackAsync();
        }
        else
        {
            await ViewNavigator.NavigateToCategorySelectionAsync();
        }
    }

    public bool CanNavigateBackward()
    {
        return CurrentStep > 1;
    }

    [RelayCommand(CanExecute = nameof(CanNavigateForward))]
    public void NavigateForward()
    { }

    public bool CanNavigateForward()
    {
        return false;
    }

    protected override void OnNavigated()
    {
        base.OnNavigated();

        InvalidateWindowTitle();

        OnPropertyChanged(nameof(CurrentStep));
        OnPropertyChanged(nameof(StepsHeader));
        OnPropertyChanged(nameof(IsHeaderVisible));

        NavigateBackwardCommand.NotifyCanExecuteChanged();
        NavigateForwardCommand.NotifyCanExecuteChanged();
    }

    protected override void OnLanguageChanged()
    {
        InvalidateWindowTitle();
        OnPropertyChanged(nameof(StepsHeader));
    }

    private void InvalidateWindowTitle()
    {
        OnPropertyChanged(nameof(BaseTitle));
        OnPropertyChanged(nameof(Title));

        if (ViewNavigator.Window.AppWindow != null)
        {
            ViewNavigator.Window.Title = Title;
            ViewNavigator.Window.AppWindow.Title = Title;
        }
    }
}