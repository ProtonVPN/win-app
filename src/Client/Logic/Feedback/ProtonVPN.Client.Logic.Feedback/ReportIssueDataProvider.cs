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

using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts.Messages;
using ProtonVPN.Client.Logic.Feedback.Contracts;
using ProtonVPN.Client.Logic.Feedback.Models;

namespace ProtonVPN.Client.Logic.Feedback;

public class ReportIssueDataProvider : IReportIssueDataProvider, IEventMessageReceiver<LanguageChangedMessage>
{
    private readonly IApiClient _apiClient;
    private readonly List<string> _validInputTypes = new() { InputType.SingleLineInput, InputType.MultiLineInput };
    private List<IssueCategoryResponse> _categories = new();

    public ReportIssueDataProvider(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<IssueCategoryResponse>> GetCategoriesAsync()
    {
        if (!_categories.Any())
        {
            List<IssueCategoryResponse>? categories = await FetchDataAsync();
            if (categories is null)
            {
                // Fallback is not cached so the API is tried again next time
                return ReportIssueDefaultDataProvider.GetCategories();
            }

            _categories = categories;
        }

        return _categories.ToList();
    }

    public void Receive(LanguageChangedMessage message)
    {
        // Clear categories to force fetching them again next time
        _categories.Clear();
    }

    private async Task<List<IssueCategoryResponse>?> FetchDataAsync()
    {
        try
        {
            ApiResponseResult<ReportAnIssueFormResponse> response = await _apiClient.GetReportAnIssueFormData();

            if (!response.Success)
            {
                throw new HttpRequestException("Error trying to retrieve Report an issue form data from the API");
            }

            if (!IsDataValid(response.Value.Categories))
            {
                throw new InvalidDataException("Invalid data received from the API for Report an issue form data ");
            }

            return response.Value.Categories;
        }
        catch
        {
            return null;
        }
    }

    private bool IsDataValid(IList<IssueCategoryResponse> categories)
    {
        return categories.All(category =>
            !string.IsNullOrEmpty(category.Label) &&
            !string.IsNullOrEmpty(category.SubmitLabel) &&
            category.InputFields != null &&
            !category.InputFields.Any(
                input => string.IsNullOrEmpty(input.Label) ||
                         string.IsNullOrEmpty(input.SubmitLabel) ||
                         !_validInputTypes.Contains(input.Type) ||
                         !AreConstraintsValid(input.Constraints)));
    }

    private static bool AreConstraintsValid(IssueInputConstraintsResponse constraints)
    {
        if (constraints == null)
        {
            return true;
        }

        return (constraints.MinimumLength is null || constraints.MinimumLength >= 0) &&
               (constraints.MaximumLength is null || constraints.MaximumLength > 0) &&
               (constraints.MinimumLength is null || constraints.MaximumLength is null ||
                constraints.MinimumLength <= constraints.MaximumLength);
    }
}