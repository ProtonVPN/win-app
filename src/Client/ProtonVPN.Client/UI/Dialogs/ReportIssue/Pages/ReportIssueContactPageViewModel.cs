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

using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Models.ReportIssue;
using ProtonVPN.Client.Core.Models.ReportIssue.Fields;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Feedback.Contracts;
using ProtonVPN.Client.Mappers;
using ProtonVPN.Client.UI.Dialogs.ReportIssue.Bases;
using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.UI.Dialogs.ReportIssue.Pages;

public partial class ReportIssueContactPageViewModel : ReportIssuePageViewModelBase
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

    public override string Title => Category?.Name ?? string.Empty;

    public NotifyErrorObservableCollection<IssueInputField> InputFields { get; }

    public ReportIssueContactPageViewModel(
        IReportIssueDataProvider dataProvider,
        IReportIssueSender reportIssueSender,
        IReportIssueViewNavigator parentViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, viewModelHelper)
    {
        _dataProvider = dataProvider;
        _reportIssueSender = reportIssueSender;

        InputFields = [];
        InputFields.ItemErrorsChanged += OnInputFieldsItemErrorsChanged;

        IncludeLogs = true;
    }

    public override void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        base.OnNavigatedTo(parameter, isBackNavigation);

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

            Result result = await _reportIssueSender.SendAsync(Category!.Key, email, serializedFields, IncludeLogs);

            isReportSent = result.Success;
        }
        catch (Exception e)
        {
            Logger.Error<AppLog>("An error occured while sending the issue report", e);
        }
        finally
        {
            if (IsSendingReport)
            {
                IsSendingReport = false;
                await ParentViewNavigator.NavigateToResultViewAsync(isReportSent);
            }
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
        ParentViewNavigator.CanNavigate = !value;
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        if (IsSendingReport)
        {
            // If window is closed while sending the report, keep sending the report in background but reset the flag on the view model
            IsSendingReport = false;
        }
    }
}