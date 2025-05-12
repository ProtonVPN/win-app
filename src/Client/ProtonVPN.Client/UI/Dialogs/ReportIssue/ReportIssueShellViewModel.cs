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

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.UI.Dialogs.ReportIssue.Pages;

namespace ProtonVPN.Client.UI.Dialogs.ReportIssue;

public partial class ReportIssueShellViewModel : ShellViewModelBase<IReportIssueWindowActivator, IReportIssueViewNavigator>,
    IEventMessageReceiver<ReportIssueCategoryChangedMessage>,
    IEventMessageReceiver<ClientUpdateStateChangedMessage>
{
    private readonly IUpdatesManager _updatesManager;

    public string BaseTitle => Localizer.Get("Dialogs_ReportIssue_Title");

    public override string Title => string.IsNullOrEmpty(CurrentPageTitle)
        ? BaseTitle
        : $"{BaseTitle} - {CurrentPageTitle}";

    public int TotalSteps => 3;

    public int CurrentStep => ChildViewNavigator.GetCurrentPageContext() switch
    {
        ReportIssueCategoriesPageViewModel => 1,
        ReportIssueCategoryPageViewModel => 2,
        ReportIssueContactPageViewModel => 3,
        _ => 0,
    };

    public string CurrentPageTitle => ChildViewNavigator.GetCurrentPageContext()?.Title ?? string.Empty;

    public string StepsHeader => Localizer.GetFormat("Dialogs_ReportIssue_Steps", CurrentStep, TotalSteps);

    public bool IsHeaderVisible => CurrentStep > 0 && CurrentStep <= TotalSteps;

    public bool IsUpdateAvailable => _updatesManager.IsUpdateAvailable;

    public ReportIssueShellViewModel(
        IUpdatesManager updatesManager,
        IReportIssueWindowActivator windowActivator,
        IReportIssueViewNavigator childViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(windowActivator, childViewNavigator, viewModelHelper)
    {
        _updatesManager = updatesManager;
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBackward))]
    public Task NavigateBackwardAsync()
    {
        return ChildViewNavigator.CanGoBack
            ? ChildViewNavigator.GoBackAsync()
            : ChildViewNavigator.NavigateToCategoriesViewAsync();
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

    public void Receive(ClientUpdateStateChangedMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsUpdateAvailable)));
    }

    protected override void OnChildNavigation(NavigationEventArgs e)
    {
        base.OnChildNavigation(e);

        InvalidateWindowTitle();

        OnPropertyChanged(nameof(CurrentStep));
        OnPropertyChanged(nameof(StepsHeader));
        OnPropertyChanged(nameof(IsHeaderVisible));

        NavigateBackwardCommand.NotifyCanExecuteChanged();
        NavigateForwardCommand.NotifyCanExecuteChanged();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        InvalidateWindowTitle();
        OnPropertyChanged(nameof(StepsHeader));
    }

    private void InvalidateWindowTitle()
    {
        OnPropertyChanged(nameof(BaseTitle));
        OnPropertyChanged(nameof(CurrentPageTitle));
        OnPropertyChanged(nameof(Title));
    }

    public void Receive(ReportIssueCategoryChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(CurrentPageTitle));
            OnPropertyChanged(nameof(Title));
        });
    }
}