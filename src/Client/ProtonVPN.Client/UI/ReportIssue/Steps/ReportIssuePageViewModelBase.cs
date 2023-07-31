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
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Feedback.Contracts;
using ProtonVPN.Client.Models.Navigation;

namespace ProtonVPN.Client.UI.ReportIssue.Steps;

public abstract partial class ReportIssuePageViewModelBase : PageViewModelBase<IReportIssueViewNavigator>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepsHeader))]
    private int _currentStep;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepsHeader))]
    private int _totalSteps;

    public string StepsHeader => Localizer.GetFormat("Dialogs_ReportIssue_Steps", CurrentStep, TotalSteps);

    protected IReportIssueDataProvider DataProvider { get; }

    protected ReportIssuePageViewModelBase(IReportIssueViewNavigator viewNavigator, ILocalizationProvider localizationProvider, IReportIssueDataProvider dataProvider)
        : base(viewNavigator, localizationProvider)
    {
        DataProvider = dataProvider;
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBackward))]
    public virtual void NavigateBackward()
    { }

    public virtual bool CanNavigateBackward()
    {
        return false;
    }

    [RelayCommand(CanExecute = nameof(CanNavigateForward))]
    public virtual void NavigateForward()
    { }

    public virtual bool CanNavigateForward()
    {
        return false;
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(StepsHeader));
    }
}