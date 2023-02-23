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
using System.Windows;
using System.Windows.Data;

namespace ProtonVPN.Sidebar.QuickSettings
{
    internal class PopupArrowPositionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue ||
                values[1] == DependencyProperty.UnsetValue ||
                values[2] == DependencyProperty.UnsetValue)
            {
                return new Thickness();
            }

            var totalButtons = System.Convert.ToInt32(values[0]);
            var buttonNumber = System.Convert.ToInt32(values[1]);
            var popupSize = System.Convert.ToInt32(values[2]);
            var arrowWidth = 20;
            var buttonMargin = 5;
            var buttonSize = (float)popupSize / totalButtons;
            var oneStep = buttonMargin + buttonSize / 2;
            var offset = oneStep + buttonNumber * oneStep * 2 - arrowWidth / 2.0 - buttonMargin * buttonNumber * 2;

            return new Thickness(offset, 2, 0, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Going back to what you had isn't supported.");
        }
    }
}