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
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Feedback.Contracts;
using ProtonVPN.Client.Mappers;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.UI.ReportIssue.Models;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.ReportIssue.Steps;

public partial class QuickFixesViewModel : PageViewModelBase<IReportIssueViewNavigator>
{
    private readonly IReportIssueDataProvider _dataProvider;
    private readonly IUrls _urls;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Suggestions))]
    private IssueCategory? _category;

    public List<IssueSuggestion> Suggestions => Category?.Suggestions ?? new();

    public override string? Title => Category?.Name;

    public QuickFixesViewModel(IReportIssueViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IReportIssueDataProvider dataProvider,
        IUrls urls,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator, localizationProvider, logger, issueReporter)
    {
        _dataProvider = dataProvider;
        _urls = urls;
    }

    public override void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        Category = parameter as IssueCategory;
    }

    [RelayCommand]
    public async Task GoToContactFormAsync()
    {
        await ViewNavigator.NavigateToContactFormAsync(Category);
    }

    [RelayCommand]
    public void BrowseLink(string parameter)
    {
        if (!string.IsNullOrEmpty(parameter))
        {
            _urls.NavigateTo(parameter);
        }
    }

    protected override async void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        await InvalidateCategoryAsync();
    }

    private async Task InvalidateCategoryAsync()
    {
        if (Category == null)
        {
            return;
        }

        List<IssueCategoryResponse> categories = await _dataProvider.GetCategoriesAsync();

        Category = ReportIssueMapper.Map(categories.First(c => c.SubmitLabel == Category.Key));
    }
}