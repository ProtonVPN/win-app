/*
 * Copyright (c) 2024 Proton AG
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
    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(WidgetButton), new PropertyMetadata(default));

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(WidgetButton), new PropertyMetadata(default));

    public static readonly DependencyProperty OnHoverFlyoutProperty =
        DependencyProperty.Register(nameof(OnHoverFlyout), typeof(FlyoutBase), typeof(WidgetButton), new PropertyMetadata(default));

    private const int OPEN_FLYOUT_DELAY_IN_MS = 300;

    private DispatcherTimer _timer;
    private bool _isPressed;

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

    public FlyoutBase OnHoverFlyout
    {
        get => (FlyoutBase)GetValue(OnHoverFlyoutProperty);
        set => SetValue(OnHoverFlyoutProperty, value);
    }

    public WidgetButton()
    {
        _timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(OPEN_FLYOUT_DELAY_IN_MS)
        };
        _timer.Tick += OnOpenFlyoutTimerTick;

        IsEnabledChanged += OnIsEnabledChanged;
    }

    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        base.OnPointerPressed(e);

        _isPressed = true;
    }

    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isPressed)
        {
            _isPressed = false;

            if (OnHoverFlyout == null)
            {
                return;
            }

            StopTimer();

            if (OnHoverFlyout.IsOpen)
            {
                OnHoverFlyout.Hide();
            }
        }
    }

    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        base.OnPointerEntered(e);

        if (OnHoverFlyout == null)
        {
            return;
        }

        if (!OnHoverFlyout.IsOpen)
        {
            StartTimer();
        }
        else
        {
            // When pointer is over the button, switch to transient mode so the flyout cannot be dismissed
            OnHoverFlyout.ShowMode = FlyoutShowMode.Transient;
        }
    }

    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        base.OnPointerExited(e);

        if (OnHoverFlyout == null)
        {
            return;
        }

        StopTimer();

        if (OnHoverFlyout.IsOpen)
        {
            // When pointer is not over the button, flyout can be dismissed when the pointer moves away
            OnHoverFlyout.ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
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
        OnHoverFlyout.ShowAt(this, options);
    }

    private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (!IsEnabled)
        {
            if (OnHoverFlyout == null)
            {
                return;
            }

            StopTimer();

            if (OnHoverFlyout.IsOpen)
            {
                OnHoverFlyout.Hide();
            }
        }
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