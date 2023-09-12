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
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Feedback.Contracts;
using ProtonVPN.Client.Mappers;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.ReportIssue;
using ProtonVPN.Client.UI.ReportIssue.Models;

namespace ProtonVPN.Client.UI.Home.Help;

public partial class HelpViewModel : ViewModelBase
{
    private readonly IDialogActivator _dialogActivator;
    private readonly IReportIssueViewNavigator _reportIssueViewNavigator;
    private readonly IReportIssueDataProvider _dataProvider;

    public bool HasCategories => Categories.Any();

    public ObservableCollection<IssueCategory> Categories { get; } = new();

    public HelpViewModel(ILocalizationProvider localizationProvider, IDialogActivator dialogActivator, IReportIssueViewNavigator reportIssueViewNavigator, IReportIssueDataProvider dataProvider)
        : base(localizationProvider)
    {
        _dialogActivator = dialogActivator;
        _reportIssueViewNavigator = reportIssueViewNavigator;
        _dataProvider = dataProvider;

        Categories = new();
        Categories.CollectionChanged += OnCategoriesCollectionChanged;
    }

    [RelayCommand]
    public async Task ShowCategoriesAsync()
    {
        Categories.Clear();

        List<IssueCategoryResponse> categories = await _dataProvider.GetCategoriesAsync();

        foreach (IssueCategoryResponse category in categories)
        {
            Categories.Add(ReportIssueMapper.Map(category));
        }
    }

    [RelayCommand]
    public async Task OpenReportIssueDialogAsync(IssueCategory category)
    {
        _dialogActivator.ShowDialog<ReportIssueShellViewModel>();

        await _reportIssueViewNavigator.NavigateToCategoryAsync(category);
    }

    private void OnCategoriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasCategories));
    }
}