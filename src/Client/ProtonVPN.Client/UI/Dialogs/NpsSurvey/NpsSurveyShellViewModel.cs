/*
 * Copyright (c) 2025 Proton AG
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
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Client.Common.Constants;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.UI.Dialogs.NpsSurvey;

public partial class NpsSurveyShellViewModel : ShellViewModelBase<INpsSurveyWindowActivator>
{
    private readonly ILogger _logger;
    private readonly IApiClient _apiClient;
    private readonly IApplicationThemeSelector _themeSelector;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmissionStatusIllustration))]
    [NotifyPropertyChangedFor(nameof(IsRetryButtonVisible))]
    [NotifyPropertyChangedFor(nameof(SubmissionTitle))]
    [NotifyPropertyChangedFor(nameof(SubmissionSubtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubmissionResult))]
    private bool _isSubmitted;

    [ObservableProperty]
    private string _comment = string.Empty;

    [ObservableProperty]
    private bool _isSending;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmissionStatusIllustration))]
    [NotifyPropertyChangedFor(nameof(IsRetryButtonVisible))]
    [NotifyPropertyChangedFor(nameof(SubmissionTitle))]
    [NotifyPropertyChangedFor(nameof(SubmissionSubtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubmissionResult))]
    private bool _hasRequestFailed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSubmissionResult))]
    private bool _isRequestSuccessful;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsScoreSelected))]
    [NotifyCanExecuteChangedFor(nameof(SubmitSurveyCommand))]
    private int _score = NpsSurveyConstants.DEFAULT_SCORE;

    public bool IsScoreSelected => Score != NpsSurveyConstants.DEFAULT_SCORE;

    public ImageSource? SubmissionStatusIllustration => IsSubmitted ?
        HasRequestFailed
            ? ResourceHelper.GetIllustration("ReportErrorIllustrationSource", _themeSelector.GetTheme())
            : ResourceHelper.GetIllustration("ReportSentIllustrationSource", _themeSelector.GetTheme())
        : null;

    public string? SubmissionTitle => IsSubmitted ?
        HasRequestFailed
            ? Localizer.Get("Dialogs_NpsSurvey_ErrorTitle")
            : Localizer.Get("Dialogs_NpsSurvey_SuccessTitle")
        : null;

    public string? SubmissionSubtitle => IsSubmitted ?
        HasRequestFailed
            ? Localizer.Get("Dialogs_NpsSurvey_ErrorSubtitle")
            : Localizer.Get("Dialogs_NpsSurvey_SuccessSubtitle")
        : null;

    public bool HasSubmissionResult => IsSubmitted && (IsRequestSuccessful || HasRequestFailed);

    public bool IsRetryButtonVisible => IsSubmitted && HasRequestFailed;

    public NpsSurveyShellViewModel(
        ILogger logger,
        IApiClient apiClient,
        IApplicationThemeSelector themeSelector,
        INpsSurveyWindowActivator windowActivator,
        IViewModelHelper viewModelHelper)
        : base(windowActivator, viewModelHelper)
    {
        _logger = logger;
        _apiClient = apiClient;
        _themeSelector = themeSelector;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        IsSubmitted = false;
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private Task SubmitSurveyAsync()
    {
        return SendSurveyRequestAsync();
    }

    private async Task SendSurveyRequestAsync()
    {
        try
        {
            IsSubmitted = true;
            IsSending = true;

            ApiResponseResult<BaseResponse> result = await _apiClient.SubmitNpsSurveyAsync(new()
            {
                Score = Score,
                Comment = Comment,
            });

            IsRequestSuccessful = result.Success;
            HasRequestFailed = result.Failure;
        }
        catch (Exception e)
        {
            HasRequestFailed = true;
            _logger.Error<AppLog>("Failed to send NPS survey request.", e);
        }
        finally
        {
            IsSending = false;
        }
    }

    private bool CanSubmit()
    {
        return IsScoreSelected && !IsSending;
    }

    protected async override void OnDeactivated()
    {
        base.OnDeactivated();

        if (!IsSubmitted)
        {
            try
            {
                await _apiClient.DismissNpsSurveyAsync();
            }
            catch (Exception e)
            {
                _logger.Error<AppLog>("Failed to send NPS survey dismiss request.", e);
            }
        }

        IsSending = false;
        IsSubmitted = false;
        HasRequestFailed = false;
        IsRequestSuccessful = false;

        Comment = string.Empty;
        Score = NpsSurveyConstants.DEFAULT_SCORE;
    }
}