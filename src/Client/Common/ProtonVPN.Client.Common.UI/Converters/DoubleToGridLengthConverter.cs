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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Windows.Web.Syndication;

namespace ProtonVPN.Client.Common.UI.Converters;

public class DoubleToGridLengthConverter : IValueConverter
{
    public GridUnitType UnitType { get; set; } = GridUnitType.Star;

    public bool IsReverse { get; set; } = false;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        double size = System.Convert.ToDouble(value);

        return IsReverse
            ? new GridLength(1 - size, UnitType)
            : new GridLength(size, UnitType);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
