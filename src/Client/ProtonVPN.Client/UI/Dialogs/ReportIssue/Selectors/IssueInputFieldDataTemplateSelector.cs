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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Core.Models.ReportIssue.Fields;

namespace ProtonVPN.Client.UI.Dialogs.ReportIssue.Selectors;

public class IssueInputFieldDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? SingleLineTextInputFieldTemplate { get; set; }

    public DataTemplate? MultiLineTextInputFieldTemplate { get; set; }

    public DataTemplate? EmailInputFieldTemplate { get; set; }

    public DataTemplate? CheckboxInputFieldTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        DataTemplate? template = item switch
        {
            SingleLineTextInputField => SingleLineTextInputFieldTemplate,
            MultiLineTextInputField => MultiLineTextInputFieldTemplate,
            EmailInputField => EmailInputFieldTemplate,
            CheckboxInputField => CheckboxInputFieldTemplate,
            _ => SingleLineTextInputFieldTemplate
        };

        return template
            ?? SingleLineTextInputFieldTemplate
            ?? throw new InvalidOperationException("Issue input field data template is undefined");
    }
}