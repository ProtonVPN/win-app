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

using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Legacy.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Feedback.Contracts;
using ProtonVPN.Client.Legacy.Mappers;
using ProtonVPN.Client.Legacy.Models.Activation.Custom;
using ProtonVPN.Client.Legacy.UI.ReportIssue.Models;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.UI.Home.Help;

public partial class HelpViewModel : ViewModelBase
{
    private readonly IReportIssueDialogActivator _reportIssueDialogActivator;
    private readonly IReportIssueDataProvider _dataProvider;

    public bool HasCategories => Categories.Any();

    public SmartObservableCollection<IssueCategory> Categories { get; }

    public HelpViewModel(
        ILocalizationProvider localizationProvider,
        IReportIssueDialogActivator reportIssueDialogActivator,
        IReportIssueDataProvider dataProvider,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizationProvider, logger, issueReporter)
    {
        _reportIssueDialogActivator = reportIssueDialogActivator;
        _dataProvider = dataProvider;

        Categories = new();
        Categories.CollectionChanged += OnCategoriesCollectionChanged;
    }

    [RelayCommand]
    public async Task ShowCategoriesAsync()
    {
        List<IssueCategoryResponse> categories = await _dataProvider.GetCategoriesAsync();
        Categories.Reset(categories.Select(ReportIssueMapper.Map));
    }

    [RelayCommand]
    public async Task OpenReportIssueDialogAsync(IssueCategory category)
    {
        await _reportIssueDialogActivator.ShowDialogAsync(category);
    }

    [RelayCommand]
    public async Task ReportIssueAsync()
    {
        // Flyouts are causing issues with resize (WinUI framework issue).
        // In the meanwhile, clicking on the help button will open the Report issue dialog directly.

        await _reportIssueDialogActivator.ShowDialogAsync();
    }

    private void OnCategoriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasCategories));
    }
}