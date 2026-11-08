/*
 * Copyright (c) 2026 Proton AG
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

using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Client.Logic.Feedback.Models;

namespace ProtonVPN.Client.Logic.Feedback.Tests;

[TestClass]
public class ReportIssueDataProviderTest
{
    private const string DEFAULT_FORM_RESOURCE_NAME = "ReportIssueDefaultForm.json";

    private IApiClient? _apiClient;

    [TestInitialize]
    public void TestInitialize()
    {
        _apiClient = Substitute.For<IApiClient>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _apiClient = null;
    }

    [TestMethod]
    public async Task GetCategoriesAsync_ShouldReturnValidFallbackCategories_WhenApiFails()
    {
        // Arrange
        MockApiFailure();
        ReportIssueDataProvider provider = new(_apiClient!);

        // Act
        List<IssueCategoryResponse> categories = await provider.GetCategoriesAsync();

        // Assert
        categories.Should().NotBeEmpty();
        categories.Should().OnlyContain(c =>
            !string.IsNullOrEmpty(c.Label) &&
            !string.IsNullOrEmpty(c.SubmitLabel) &&
            c.InputFields != null &&
            c.InputFields.Count > 0);
        categories.SelectMany(c => c.InputFields).Should().OnlyContain(i =>
            !string.IsNullOrEmpty(i.Label) &&
            !string.IsNullOrEmpty(i.SubmitLabel) &&
            (i.Type == InputType.SingleLineInput || i.Type == InputType.MultiLineInput) &&
            (i.Constraints == null ||
             i.Constraints.MinimumLength == null ||
             i.Constraints.MaximumLength == null ||
             i.Constraints.MinimumLength <= i.Constraints.MaximumLength));
    }

    [TestMethod]
    public void DefaultForm_ShouldOnlyContainPropertiesKnownByApiContract()
    {
        // Arrange
        // Deserialization is lenient at runtime, so a contract change would silently drop the outdated JSON properties
        JsonSerializerSettings strictSettings = new() { MissingMemberHandling = MissingMemberHandling.Error };
        using Stream stream = typeof(ReportIssueDataProvider).Assembly.GetManifestResourceStream(DEFAULT_FORM_RESOURCE_NAME)!;
        using StreamReader reader = new(stream);
        string json = reader.ReadToEnd();

        // Act
        Action act = () => JsonConvert.DeserializeObject<ReportAnIssueFormResponse>(json, strictSettings);

        // Assert
        act.Should().NotThrow("the embedded default form must match the API contract, update it with a fresh API response");
    }

    [TestMethod]
    public async Task GetCategoriesAsync_ShouldRetryApi_WhenPreviousCallFellBackToDefaultCategories()
    {
        // Arrange
        MockApiFailure();
        ReportIssueDataProvider provider = new(_apiClient!);

        // Act
        await provider.GetCategoriesAsync();
        await provider.GetCategoriesAsync();

        // Assert
        await _apiClient!.Received(2).GetReportAnIssueFormData();
    }

    [TestMethod]
    public async Task GetCategoriesAsync_ShouldCacheApiCategories_WhenApiSucceeds()
    {
        // Arrange
        List<IssueCategoryResponse> apiCategories =
        [
            new()
            {
                Label = "Category",
                SubmitLabel = "Category",
                InputFields =
                [
                    new() { Label = "Field", SubmitLabel = "Field", Type = InputType.MultiLineInput }
                ]
            }
        ];
        _apiClient!.GetReportAnIssueFormData().Returns(
            ApiResponseResult<ReportAnIssueFormResponse>.Ok(new HttpResponseMessage(), new() { Categories = apiCategories }));
        ReportIssueDataProvider provider = new(_apiClient);

        // Act
        await provider.GetCategoriesAsync();
        List<IssueCategoryResponse> categories = await provider.GetCategoriesAsync();

        // Assert
        categories.Should().BeEquivalentTo(apiCategories);
        await _apiClient.Received(1).GetReportAnIssueFormData();
    }

    private void MockApiFailure()
    {
        _apiClient!.GetReportAnIssueFormData().Returns(
            ApiResponseResult<ReportAnIssueFormResponse>.Fail(new HttpResponseMessage(), "API unreachable"));
    }
}
