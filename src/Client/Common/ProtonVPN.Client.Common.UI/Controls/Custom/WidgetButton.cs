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
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class WidgetButton : Button
{
    private const int OPEN_FLYOUT_DELAY_IN_MS = 400;

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(WidgetButton), new PropertyMetadata(default));

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(WidgetButton), new PropertyMetadata(default));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private DispatcherTimer _timer ;

    public WidgetButton()
    {
        _timer = new DispatcherTimer() 
        { 
            Interval = TimeSpan.FromMilliseconds(OPEN_FLYOUT_DELAY_IN_MS) 
        };
        _timer.Tick += OnOpenFlyoutTimerTick;
    }

    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        base.OnPointerEntered(e);

        if (Flyout == null)
        {
            return;
        }

        if (!Flyout.IsOpen)
        {
            StartTimer();
        }
        else
        {
            // When pointer is over the button, switch to transient mode so the flyout cannot be dismissed
            Flyout.ShowMode = FlyoutShowMode.Transient;
        }
    }

    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        base.OnPointerExited(e);

        if (Flyout == null)
        {
            return;
        }

        StopTimer();

        if (Flyout.IsOpen)
        {
            // When pointer is not over the button, flyout can be dismissed when the pointer moves away
            Flyout.ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
        }
    }

    private void OnOpenFlyoutTimerTick(object? sender, object e)
    {

        StopTimer();

        FlyoutShowOptions options = new()
        {
            ShowMode = FlyoutShowMode.Transient,
            Placement = FlyoutPlacementMode.LeftEdgeAlignedTop,
            Position = new Point(-16, -1)
        };
        Flyout.ShowAt(this, options);
    }

    private void StartTimer()
    {
        if (!_timer.IsEnabled)
        {
            _timer.Start();
        }
    }

    private void StopTimer()
    {
        if (_timer.IsEnabled)
        {
            _timer.Stop();
        }
    }
}