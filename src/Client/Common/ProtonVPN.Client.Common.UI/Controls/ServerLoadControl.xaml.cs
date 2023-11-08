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

namespace ProtonVPN.Client.Common.UI.Controls;

public sealed partial class ServerLoadControl
{
    public const double MEDIUM_SERVER_LOAD_THRESHOLD = 0.75;
    public const double HIGH_SERVER_LOAD_THRESHOLD = 0.90;

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(ServerLoadControl), new PropertyMetadata(default, OnValuePropertyChanged));

    public static readonly DependencyProperty FirstThresholdProperty =
        DependencyProperty.Register(nameof(FirstThreshold), typeof(double), typeof(ServerLoadControl), new PropertyMetadata(MEDIUM_SERVER_LOAD_THRESHOLD, OnThresholdPropertyChanged));

    public static readonly DependencyProperty SecondThresholdProperty =
        DependencyProperty.Register(nameof(SecondThreshold), typeof(double), typeof(ServerLoadControl), new PropertyMetadata(HIGH_SERVER_LOAD_THRESHOLD, OnThresholdPropertyChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double FirstThreshold
    {
        get => (double)GetValue(FirstThresholdProperty);
        set => SetValue(FirstThresholdProperty, value);
    }

    public double SecondThreshold
    {
        get => (double)GetValue(SecondThresholdProperty);
        set => SetValue(SecondThresholdProperty, value);
    }

    public ServerLoadControl()
    {
        InitializeComponent();
    }

    private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ServerLoadControl control)
        {
            control.InvalidateIndicatorSize();
            control.InvalidateIndicatorColor();
        }
    }

    private static void OnThresholdPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ServerLoadControl control)
        {
            control.InvalidateIndicatorColor();
        }
    }

    private void InvalidateIndicatorSize()
    {
        if (Value is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(Value), $"Server load is out of range. Value must be between 0 and 1 (current: {Value})");
        }

        PART_FillColumn.Width = new GridLength(Value, GridUnitType.Star);
        PART_EmptyColumn.Width = new GridLength(1 - Value, GridUnitType.Star);
    }

    private void InvalidateIndicatorColor()
    {
        switch (Value)
        {
            case double when Value <= FirstThreshold:
                VisualStateManager.GoToState(this, "LowServerLoad", true);
                break;

            case double when Value <= SecondThreshold:
                VisualStateManager.GoToState(this, "MediumServerLoad", true);
                break;

            default:
                VisualStateManager.GoToState(this, "HighServerLoad", true);
                break;
        }
    }
}