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

using System.Reflection;
using Newtonsoft.Json;
using ProtonVPN.Api.Contracts.ReportAnIssue;

namespace ProtonVPN.Client.Logic.Feedback;

// Fallback form used when the API is unreachable. The embedded JSON is a copy of the (English) API response body,
// so it can be refreshed by pasting a new response. Never paste request headers (tokens, cookies) into it.
internal class ReportIssueDefaultDataProvider
{
    private const string RESOURCE_NAME = "ReportIssueDefaultForm.json";

    public static List<IssueCategoryResponse> GetCategories()
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCE_NAME)
            ?? throw new InvalidOperationException($"Embedded resource '{RESOURCE_NAME}' not found");
        using StreamReader reader = new(stream);

        ReportAnIssueFormResponse? response = JsonConvert.DeserializeObject<ReportAnIssueFormResponse>(reader.ReadToEnd());

        return response?.Categories ?? [];
    }
}
