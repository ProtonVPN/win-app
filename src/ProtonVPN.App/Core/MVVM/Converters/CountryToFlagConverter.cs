﻿/*
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
    public class CountryToFlagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string countryCode = (string)value;
            if (string.IsNullOrEmpty(countryCode))
            {
                countryCode = "zz";
            }

            if (parameter == null)
            {
                return $"/ProtonVPN;component/Resources/Assets/Images/Flags/{countryCode}.png";
            }

            Type type = Type.GetType($"ProtonVPN.Resource.Graphics.Flags.{countryCode.ToUpper()}, ProtonVPN.Resource");
            if (type != null && Activator.CreateInstance(type) is UserControl control)
            {
                return control;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}