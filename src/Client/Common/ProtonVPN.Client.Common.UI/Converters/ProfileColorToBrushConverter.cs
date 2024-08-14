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
using System.Collections.Generic;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.Enums;
using Windows.UI;

namespace ProtonVPN.Client.Common.UI.Converters;

public class ProfileColorToBrushConverter : IValueConverter
{
    public Color PurpleColor { get; set; }
    public Color BlueColor { get; set; }
    public Color GreenColor { get; set; }
    public Color RedColor { get; set; }
    public Color OrangeColor { get; set; }
    public Color YellowColor { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        ProfileColor profileColor = value is ProfileColor c ? c : ProfileColor.Purple;

        Color color = profileColor switch
        {
            ProfileColor.Blue => BlueColor,
            ProfileColor.Purple => PurpleColor,
            ProfileColor.Green => GreenColor,
            ProfileColor.Red => RedColor,
            ProfileColor.Orange => OrangeColor,
            ProfileColor.Yellow => YellowColor,
            _ => PurpleColor,
        };

        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}