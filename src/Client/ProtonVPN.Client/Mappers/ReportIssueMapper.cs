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

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Core.Models.ReportIssue;
using ProtonVPN.Client.Core.Models.ReportIssue.Fields;

namespace ProtonVPN.Client.Mappers;

public static class ReportIssueMapper
{
    public const string BROWSING_SPEED_CATEGORY = "Browsing speed";
    public const string CONNECTING_TO_VPN_CATEGORY = "Connecting to VPN";
    public const string WEAK_CONNECTION_CATEGORY = "Weak or unstable connection";
    public const string USING_APP_CATEGORY = "Using the app";
    public const string STREAMING_CATEGORY = "Streaming";
    public const string SOMETHING_ELSE_CATEGORY = "Something else";
    public const string PROMOTIONS_CATEGORY = "Promotions and Offers";
    public const string CAPTCHA_ISSUES_CATEGORY = "CAPTCHA issues";
    public const string CAN_NOT_ACCESS_WEBSITE_CATEGORY = "Can't access a website or service";
    public const string LAN_CONNECTIONS_CATEGORY = "LAN (local network) connections";

    public const string TEXT_FIELD_TYPE = "TextSingleLine";
    public const string MULTI_TEXT_FIELD_TYPE = "TextMultiLine";

    public static IssueCategory Map(IssueCategoryResponse category)
    {
        return new IssueCategory()
        {
            Key = category.SubmitLabel,
            Name = category.Label,
            Icon = GetCategoryIcon(category.SubmitLabel),
            Suggestions = category.Suggestions?.Select(s => Map(s)).ToList() ?? new(),
            InputFields = category.InputFields?.Select(i => Map(i)).ToList() ?? new(),
        };
    }

    public static IssueSuggestion Map(IssueSuggestionResponse suggestion)
    {
        return new IssueSuggestion()
        {
            Name = suggestion.Text,
            Link = suggestion.Link,
        };
    }

    public static IssueInputField Map(IssueInputResponse inputField)
    {
        IssueInputField input = inputField.Type switch
        {
            TEXT_FIELD_TYPE => new SingleLineTextInputField(inputField.IsMandatory),
            MULTI_TEXT_FIELD_TYPE => new MultiLineTextInputField(inputField.IsMandatory),
            _ => throw new InvalidOperationException($"Unknown input type for Report an issue contact form ('{inputField.Type}')")
        };

        input.Name = inputField.Label;
        input.Key = inputField.SubmitLabel;
        input.Placeholder = inputField.Placeholder;
        input.Type = inputField.Type;

        return input;
    }

    private static IconElement? GetCategoryIcon(string categoryKey)
    {
        return categoryKey switch
        {
            BROWSING_SPEED_CATEGORY => new Bolt(),
            CONNECTING_TO_VPN_CATEGORY => new PowerOff(),
            WEAK_CONNECTION_CATEGORY => new ExclamationCircle(),
            USING_APP_CATEGORY => new BrandProtonVpn(),
            STREAMING_CATEGORY => new Play(),
            SOMETHING_ELSE_CATEGORY => new ThreeDotsHorizontal(),
            PROMOTIONS_CATEGORY => new BagPercent(),
            CAPTCHA_ISSUES_CATEGORY => new Key(),
            CAN_NOT_ACCESS_WEBSITE_CATEGORY => new CircleSlash(),
            LAN_CONNECTIONS_CATEGORY => new Servers(),
            _ => new Placeholder()
        };
    }
}