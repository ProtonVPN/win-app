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
    private const PathIconSize DEFAULT_ICON_SIZE = PathIconSize.Pixels20;

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(PathIconSize), typeof(CustomPathIcon), new PropertyMetadata(DEFAULT_ICON_SIZE, OnSizePropertyChanged));

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

        double size = GetSizeInPixels(Size);

        Width = size;
        Height = size;
    }

    private string GetGeometry(PathIconSize size)
    {
        string geometry = size switch
        {
            PathIconSize.Pixels16 => IconGeometry16,
            PathIconSize.Pixels20 => IconGeometry20,
            PathIconSize.Pixels24 => IconGeometry24,
            PathIconSize.Pixels32 => IconGeometry32,
            _ => string.Empty,
        };

        return string.IsNullOrEmpty(geometry)
            ? size switch
            {
                // If requested size is not available, fallback onto smaller size
                PathIconSize.Pixels20 => GetGeometry(PathIconSize.Pixels16),
                PathIconSize.Pixels24 => GetGeometry(PathIconSize.Pixels20),
                PathIconSize.Pixels32 => GetGeometry(PathIconSize.Pixels24),
                _ => string.Empty,
            }
            : geometry;
    }

    private double GetSizeInPixels(PathIconSize size)
    {
        return Size switch
        {
            PathIconSize.Pixels16 => 16.0,
            PathIconSize.Pixels20 => 20.0,
            PathIconSize.Pixels24 => 24.0,
            PathIconSize.Pixels32 => 32.0,
            _ => 0.0,
        };
    }
}