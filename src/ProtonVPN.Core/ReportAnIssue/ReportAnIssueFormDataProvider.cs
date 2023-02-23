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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.ReportAnIssue
{
    public class ReportAnIssueFormDataProvider : IReportAnIssueFormDataProvider, ISettingsAware, ILoggedInAware, ILogoutAware
    {
        private readonly IApiClient _apiClient;
        private readonly IAppSettings _appSettings;
        private readonly List<string> _validInputTypes = new() { InputType.SingleLineInput, InputType.MultiLineInput };
        private List<IssueCategoryResponse> _categories = new();
        private bool _isUserLoggedIn;

        public ReportAnIssueFormDataProvider(IApiClient apiClient, IAppSettings appSettings)
        {
            _apiClient = apiClient;
            _appSettings = appSettings;
        }

        public async Task FetchData()
        {
            try
            {
                ApiResponseResult<ReportAnIssueFormResponse> response = await _apiClient.GetReportAnIssueFormData();
                if (response.Success && IsDataValid(response.Value.Categories))
                {
                    _categories = response.Value.Categories;
                    if (_isUserLoggedIn)
                    {
                        _appSettings.ReportAnIssueFormData = response.Value.Categories;
                    }
                }
                else
                {
                    LoadCategoriesFromCache();
                }
            }
            catch
            {
                LoadCategoriesFromCache();
            }
        }

        public List<IssueCategoryResponse> GetCategories()
        {
            if (_categories.Count == 0)
            {
                LoadCategoriesFromCache();
            }

            return _categories.ToList();
        }

        public List<IssueSuggestionResponse> GetSuggestions(string category)
        {
            return _categories.FirstOrDefault(c => c.SubmitLabel == category)?.Suggestions;
        }

        public List<IssueInputResponse> GetInputs(string categorySubmitName)
        {
            return _categories.FirstOrDefault(c => c.SubmitLabel == categorySubmitName)?.InputFields;
        }

        public async void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAppSettings.Language))
            {
                await FetchData();
            }
        }

        private bool IsDataValid(IList<IssueCategoryResponse> categories)
        {
            return categories.All(category =>
                !category.Label.IsNullOrEmpty() &&
                !category.SubmitLabel.IsNullOrEmpty() &&
                category.InputFields != null &&
                !category.InputFields.Any(
                    input => input.Label.IsNullOrEmpty() ||
                             input.SubmitLabel.IsNullOrEmpty() ||
                             !_validInputTypes.Contains(input.Type)));
        }

        private void LoadCategoriesFromCache()
        {
            _categories = _isUserLoggedIn && _appSettings.ReportAnIssueFormData.Count > 0
                ? _appSettings.ReportAnIssueFormData
                : DefaultCategoryProvider.GetCategories();
        }

        public void OnUserLoggedIn()
        {
            _isUserLoggedIn = true;
        }

        public void OnUserLoggedOut()
        {
            _isUserLoggedIn = false;
        }
    }
}