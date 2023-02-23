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

using System;
using System.Globalization;
using System.Security;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Core.MVVM.Converters
{
    public class StringNullOrEmptyToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility targetVisibility = Visibility.Hidden;
            if (parameter is Visibility parameterVisibility)
            {
                targetVisibility = parameterVisibility;
            }
            return IsStringNullOrEmpty(value)
                ? targetVisibility
                : Visibility.Visible;
        }

        private bool IsStringNullOrEmpty(object value)
        {
            return value is SecureString secureString
                ? secureString.IsNullOrEmpty()
                : string.IsNullOrEmpty(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}