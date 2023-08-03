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
using System.Windows.Controls;
using System.Windows.Data;

namespace ProtonVPN.Core.MVVM.Converters
{
    public class InvertedBooleanToVisibilityConverter : IValueConverter
    {
        private readonly BooleanToVisibilityConverter _boolToVisConverter;

        public InvertedBooleanToVisibilityConverter()
        {
            _boolToVisConverter = new BooleanToVisibilityConverter();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object invertedValue = value;
            if (value is bool boolValue1)
            {
                invertedValue = !boolValue1;
            }
            else if (value is string stringValue && bool.TryParse(stringValue, out bool boolValue2))
            {
                invertedValue = (!boolValue2).ToString();
            }
            return _boolToVisConverter.Convert(invertedValue, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result = _boolToVisConverter.Convert(value, targetType, parameter, culture);
            if (result is bool boolResult1)
            {
                return !boolResult1;
            }
            else if (result is string stringResult && bool.TryParse(stringResult, out bool boolResult2))
            {
                return (!boolResult2).ToString();
            }
            return result;
        }
    }
}