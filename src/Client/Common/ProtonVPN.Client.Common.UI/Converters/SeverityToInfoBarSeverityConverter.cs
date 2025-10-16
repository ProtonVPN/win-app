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

using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using ProtonVPN.Client.Common.Enums;

namespace ProtonVPN.Client.Common.UI.Converters;

public class SeverityToInfoBarSeverityConverter : IValueConverter
{
    private const InfoBarSeverity DEFAULT_SEVERITY = InfoBarSeverity.Informational;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (Enum.TryParse(value.ToString(), out Severity severity))
        {
            return severity switch
            {
                Severity.Informational => InfoBarSeverity.Informational,
                Severity.Warning => InfoBarSeverity.Warning,
                Severity.Error => InfoBarSeverity.Error,
                Severity.Success => InfoBarSeverity.Success,
                _ => DEFAULT_SEVERITY,
            };
        }

        return DEFAULT_SEVERITY;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

