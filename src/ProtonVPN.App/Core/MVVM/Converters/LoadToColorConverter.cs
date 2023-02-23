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
using System.Windows.Data;
using System.Windows.Media;
using ProtonVPN.Resource.Colors;
using ProtonVPN.Resources.Colors;

namespace ProtonVPN.Core.MVVM.Converters
{
    public class LoadToColorConverter : IValueConverter
    {
        private static readonly IColorPalette _colorPalette = ColorPaletteFactory.Create();
        private readonly Lazy<string> _serverLoadGreenColor = new(() => _colorPalette.GetStringByResourceName("SignalSuccessColor"));
        private readonly Lazy<string> _serverLoadYellowColor = new(() => _colorPalette.GetStringByResourceName("SignalWarningColor"));
        private readonly Lazy<string> _serverLoadRedColor = new(() => _colorPalette.GetStringByResourceName("SignalDangerColor"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return _serverLoadGreenColor;
            }

            string rgb = ConvertLoadToRgb((int)value);

            return (SolidColorBrush)new BrushConverter().ConvertFrom(rgb);
        }

        private string ConvertLoadToRgb(int load)
        {
            return load switch
            {
                <= 75 => _serverLoadGreenColor.Value,
                <= 90 => _serverLoadYellowColor.Value,
                _ => _serverLoadRedColor.Value
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
