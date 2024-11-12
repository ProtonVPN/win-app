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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Models.ReportIssue;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Feedback.Contracts;
using ProtonVPN.Client.Mappers;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Dialogs.ReportIssue;

public partial class ReportIssueComponentViewModel : ViewModelBase,
    IEventMessageReceiver<LoggedInMessage>
{
    private readonly IReportIssueViewNavigator _reportIssueViewNavigator;
    private readonly IReportIssueDataProvider _dataProvider;
    private readonly IReportIssueWindowActivator _reportIssueWindowActivator;

    private SemaphoreSlim _semaphore = new(1);

    public ObservableCollection<IssueCategory> Categories { get; }

    public ReportIssueComponentViewModel(
        IReportIssueViewNavigator reportIssueViewNavigator,
        IReportIssueDataProvider dataProvider,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IReportIssueWindowActivator reportIssueWindowActivator)
        : base(localizer, logger, issueReporter)
    {
        _reportIssueViewNavigator = reportIssueViewNavigator;
        _dataProvider = dataProvider;
        _reportIssueWindowActivator = reportIssueWindowActivator;

        Categories = [];
    }

    [RelayCommand]
    private void ReportIssue()
    {
        _reportIssueWindowActivator.Activate();
    }

    [RelayCommand]
    public async Task SelectCategoryAsync(IssueCategory category)
    {
        ReportIssue();
        // TODO: find a proper solution
        await Task.Delay(50);
        await _reportIssueViewNavigator.NavigateToCategoryViewAsync(category);
    }

    protected override async void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        await InvalidateCategoriesAsync();
    }

    private async Task InvalidateCategoriesAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            List<IssueCategoryResponse> categories = await _dataProvider.GetCategoriesAsync();

            Categories.Clear();

            foreach (IssueCategoryResponse category in categories)
            {
                Categories.Add(ReportIssueMapper.Map(category));
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(async () => await InvalidateCategoriesAsync());
    }
}