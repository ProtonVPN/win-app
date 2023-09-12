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

using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Feedback.Contracts;
using ProtonVPN.Client.Mappers;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.ReportIssue.Models;
using ProtonVPN.Client.UI.ReportIssue.Models.Fields;
using ProtonVPN.Common.Abstract;

namespace ProtonVPN.Client.UI.ReportIssue.Steps;

public partial class ContactFormViewModel : PageViewModelBase<IReportIssueViewNavigator>
{
    private readonly IReportIssueDataProvider _dataProvider;
    private readonly IReportIssueSender _reportIssueSender;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyCanExecuteChangedFor(nameof(SendReportCommand))]
    private IssueCategory? _category;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendReportCommand))]
    private bool _isSendingReport;

    [ObservableProperty]
    private bool _includeLogs;

    public override string? Title => Category?.Name;

    public NotifyErrorObservableCollection<IssueInputField> InputFields { get; }

    public ContactFormViewModel(IReportIssueViewNavigator viewNavigator, ILocalizationProvider localizationProvider, IReportIssueDataProvider dataProvider, IReportIssueSender reportIssueSender)
        : base(viewNavigator, localizationProvider)
    {
        _dataProvider = dataProvider;
        _reportIssueSender = reportIssueSender;

        InputFields = new();
        InputFields.ItemErrorsChanged += OnInputFieldsItemErrorsChanged;
    }

    public override void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        Category = parameter as IssueCategory;
    }

    [RelayCommand(CanExecute = nameof(CanSendReport))]
    public async Task SendReportAsync()
    {
        bool isReportSent = false;

        try
        {
            IsSendingReport = true;

            string email =
                InputFields.First()
                           .Serialize().value;

            Dictionary<string, string> serializedFields =
                InputFields.Skip(1)
                           .Select(f => f.Serialize())
                           .ToDictionary(a => a.key, a => a.value);

            Result result = await _reportIssueSender.SendAsync(Category.Key, email, serializedFields, IncludeLogs);

            isReportSent = result.Success;
        }
        finally
        {
            IsSendingReport = false;
            await ViewNavigator.NavigateToResultAsync(isReportSent);
        }
    }

    public bool CanSendReport()
    {
        return !IsSendingReport
            && Category != null
            && InputFields.All(f => !f.HasErrors);
    }

    protected override async void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        await InvalidateCategoryAsync();
    }

    private void OnInputFieldsItemErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        SendReportCommand.NotifyCanExecuteChanged();
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

    private void InvalidateInputFields()
    {
        InputFields.Clear();

        if (Category == null)
        {
            throw new ArgumentException(nameof(Category), "Cannot load contact form with no category");
        }

        InputFields.Add(new EmailInputField(true)
        {
            Key = "Email",
            Name = Localizer.Get("Dialogs_ReportIssue_ContactForm_Email"),
            Placeholder = Localizer.Get("Dialogs_ReportIssue_ContactForm_Email_Description"),
        });

        foreach (IssueInputField field in Category.InputFields)
        {
            InputFields.Add(field);
        }
    }

    partial void OnCategoryChanged(IssueCategory? value)
    {
        InvalidateInputFields();
    }

    partial void OnIsSendingReportChanged(bool value)
    {
        ViewNavigator.CanNavigate = !value;
    }
}