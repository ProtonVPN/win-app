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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Feedback.Contracts;
using ProtonVPN.Client.Mappers;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.ReportIssue.Models;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.ReportIssue.Steps;

public partial class CategorySelectionViewModel : PageViewModelBase<IReportIssueViewNavigator>
{
    private readonly IReportIssueDataProvider _dataProvider;

    private SemaphoreSlim _semaphore = new(1);

    public ObservableCollection<IssueCategory> Categories { get; }

    public CategorySelectionViewModel(IReportIssueViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IReportIssueDataProvider dataProvider,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator, localizationProvider, logger, issueReporter)
    {
        _dataProvider = dataProvider;

        Categories = new();
    }

    public override async void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        await InvalidateCategoriesAsync();
    }

    [RelayCommand]
    public async Task SelectCategoryAsync(IssueCategory category)
    {
        await ViewNavigator.NavigateToCategoryAsync(category);
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
}