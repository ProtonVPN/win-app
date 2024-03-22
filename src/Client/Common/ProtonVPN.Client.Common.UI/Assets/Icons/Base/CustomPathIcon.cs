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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

namespace ProtonVPN.Client.Common.UI.Assets.Icons.Base;

public abstract class CustomPathIcon : PathIcon
{
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(PathIconSize), typeof(CustomPathIcon), new PropertyMetadata(PathIconSize.Pixels16, OnSizePropertyChanged));

    public PathIconSize Size
    {
        get => (PathIconSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    protected abstract string IconGeometry16 { get; }
    protected abstract string IconGeometry20 { get; }
    protected abstract string IconGeometry24 { get; }
    protected abstract string IconGeometry32 { get; }

    public CustomPathIcon()
    {
        InvalidateData();
    }

    private static void OnSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CustomPathIcon icon)
        {
            icon.InvalidateData();
        }
    }

    private void InvalidateData()
    {
        Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), GetGeometry(Size));
    }

    private string GetGeometry(PathIconSize size)
    {
        string geometry = Size switch
        {
            PathIconSize.Pixels16 => IconGeometry16,
            PathIconSize.Pixels20 => IconGeometry20,
            PathIconSize.Pixels24 => IconGeometry24,
            PathIconSize.Pixels32 => IconGeometry32,
            _ => IconGeometry20,
        };

        return string.IsNullOrEmpty(geometry)
            ? Size switch
            {
                // If requested size is not available, fallback onto smaller size
                PathIconSize.Pixels20 => GetGeometry(PathIconSize.Pixels16),
                PathIconSize.Pixels24 => GetGeometry(PathIconSize.Pixels20),
                PathIconSize.Pixels32 => GetGeometry(PathIconSize.Pixels24),
                _ => string.Empty,
            }
            : geometry;
    }
}