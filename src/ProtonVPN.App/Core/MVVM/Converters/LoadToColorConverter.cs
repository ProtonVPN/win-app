/*
 * Copyright (c) 2020 Proton Technologies AG
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
using System.Windows.Data;
using System.Windows.Media;

namespace ProtonVPN.Core.MVVM.Converters
{
    public class LoadToColorConverter : IValueConverter
    {
        public const string ORANGE_COLOR = "#ea4c21";
        public const string GREEN_COLOR = "#56b366";
        public const string YELLOW_COLOR = "#e7ca2a";
        public const string RED_COLOR = "#d90e0e";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return ORANGE_COLOR;
            }

            string rgb = ConvertLoadToRgb((int)value);

            return (SolidColorBrush)new BrushConverter().ConvertFrom(rgb);
        }

        private string ConvertLoadToRgb(int load)
        {
            return load switch
            {
                <= 75 => GREEN_COLOR,
                <= 90 => YELLOW_COLOR,
                _ => RED_COLOR
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
