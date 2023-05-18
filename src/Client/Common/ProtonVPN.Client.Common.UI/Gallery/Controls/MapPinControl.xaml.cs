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
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI;

namespace ProtonVPN.Client.Common.UI.Gallery.Controls;

public sealed partial class MapPinControl : UserControl
{
    public const int NON_TARGET_PIN_SIZE = 12;
    public const int TARGET_PIN_SIZE = 24;

    public static readonly DependencyProperty IsTargetProperty =
        DependencyProperty.Register(nameof(IsTarget), typeof(bool), typeof(MapPinControl), new PropertyMetadata(false, OnIsTargetPropertyChanged));

    public static readonly SolidColorBrush NonTargetPinBrush = new(Color.FromArgb(255, 138, 110, 255));

    public static readonly DependencyProperty PinBrushProperty =
        DependencyProperty.Register(nameof(PinBrush), typeof(Brush), typeof(MapPinControl), new PropertyMetadata(null));

    public static readonly DependencyProperty PinSizeProperty =
        DependencyProperty.Register(nameof(PinSize), typeof(double), typeof(MapPinControl), new PropertyMetadata(8.0));

    public static readonly DependencyProperty PinStrokeThicknessProperty =
        DependencyProperty.Register(nameof(PinStrokeThickness), typeof(double), typeof(MapPinControl), new PropertyMetadata(4.0));

    public static readonly DependencyProperty PositionProperty =
        DependencyProperty.Register(nameof(Position), typeof(Point), typeof(MapPinControl), new PropertyMetadata(null, OnPositionPropertyChanged));

    public static readonly SolidColorBrush TargetPinBrush = new(Color.FromArgb(255, 35, 180, 147));

    public MapPinControl()
    {
        InitializeComponent();
    }

    public bool IsTarget
    {
        get => (bool)GetValue(IsTargetProperty);
        set => SetValue(IsTargetProperty, value);
    }

    public Brush PinBrush
    {
        get => (Brush)GetValue(PinBrushProperty);
        set => SetValue(PinBrushProperty, value);
    }

    public double PinSize
    {
        get => (double)GetValue(PinSizeProperty);
        set => SetValue(PinSizeProperty, value);
    }

    public double PinStrokeThickness
    {
        get => (double)GetValue(PinStrokeThicknessProperty);
        set => SetValue(PinStrokeThicknessProperty, value);
    }

    public Point Position
    {
        get => (Point)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    private static void OnIsTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MapPinControl pin)
        {
            pin.PinSize = pin.IsTarget ? TARGET_PIN_SIZE : NON_TARGET_PIN_SIZE;
            pin.PinStrokeThickness = pin.PinSize / 4.0;
            pin.PinBrush = pin.IsTarget ? TargetPinBrush : NonTargetPinBrush;

            pin.UpdatePinPlacement();
        }
    }

    private static void OnPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MapPinControl pin)
        {
            pin.UpdatePinPlacement();
        }
    }

    private void UpdatePinPlacement()
    {
        Canvas.SetLeft(this, Position.X - (PinSize / 2.0));
        Canvas.SetTop(this, Position.Y - (PinSize / 2.0));
    }
}