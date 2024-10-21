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
using Microsoft.UI.Xaml.Media.Animation;

namespace ProtonVPN.Client.Common.UI.Controls.Map;

public sealed partial class CountryPin
{
    public static readonly DependencyProperty IsConnectedProperty = DependencyProperty.Register(
        nameof(IsConnected),
        typeof(bool),
        typeof(CountryPin),
        new PropertyMetadata(false, OnIsConnectedChanged));

    public static readonly DependencyProperty IsConnectingProperty = DependencyProperty.Register(
        nameof(IsConnecting),
        typeof(bool),
        typeof(CountryPin),
        new PropertyMetadata(false));

    public static readonly DependencyProperty IsMainWindowVisibleProperty = DependencyProperty.Register(
        nameof(IsMainWindowVisible),
        typeof(bool),
        typeof(CountryPin),
        new PropertyMetadata(false, OnIsMainWindowActiveChanged));

    public bool IsConnected
    {
        get => (bool)GetValue(IsConnectedProperty);
        set => SetValue(IsConnectedProperty, value);
    }

    public bool IsConnecting
    {
        get => (bool)GetValue(IsConnectingProperty);
        set => SetValue(IsConnectingProperty, value);
    }

    public bool IsMainWindowVisible
    {
        get => (bool)GetValue(IsMainWindowVisibleProperty);
        set => SetValue(IsMainWindowVisibleProperty, value);
    }

    private readonly Storyboard _initialScaleStoryboard = new();
    private readonly Storyboard _pulseStoryboard = new()
    {
        AutoReverse = true,
        RepeatBehavior = new RepeatBehavior(2)
    };

    public CountryPin()
    {
        InitializeComponent();

        CreateDisconnectedStateAnimations();
        CreatePulseAnimations();
    }

    private void CreateDisconnectedStateAnimations()
    {
        DoubleAnimationUsingKeyFrames innerDotAnimationX = GetDotAnimation(1);
        DoubleAnimationUsingKeyFrames innerDotAnimationY = GetDotAnimation(1);

        DoubleAnimationUsingKeyFrames outerDotAnimationX = GetDotAnimation(1);
        DoubleAnimationUsingKeyFrames outerDotAnimationY = GetDotAnimation(1);

        DoubleAnimationUsingKeyFrames fadeAnimationX = GetFadeAnimation();
        DoubleAnimationUsingKeyFrames fadeAnimationY = GetFadeAnimation();

        _initialScaleStoryboard.Children.Add(innerDotAnimationX);
        _initialScaleStoryboard.Children.Add(innerDotAnimationY);
        _initialScaleStoryboard.Children.Add(outerDotAnimationX);
        _initialScaleStoryboard.Children.Add(outerDotAnimationY);
        _initialScaleStoryboard.Children.Add(fadeAnimationX);
        _initialScaleStoryboard.Children.Add(fadeAnimationY);
        _initialScaleStoryboard.Completed += OnInitialScaleAnimationCompleted;

        Storyboard.SetTarget(innerDotAnimationX, InnerDotScaleTransform);
        Storyboard.SetTarget(innerDotAnimationY, InnerDotScaleTransform);
        Storyboard.SetTargetProperty(innerDotAnimationX, "ScaleX");
        Storyboard.SetTargetProperty(innerDotAnimationY, "ScaleY");

        Storyboard.SetTarget(outerDotAnimationX, OuterDotScaleTransform);
        Storyboard.SetTarget(outerDotAnimationY, OuterDotScaleTransform);
        Storyboard.SetTargetProperty(outerDotAnimationX, "ScaleX");
        Storyboard.SetTargetProperty(outerDotAnimationY, "ScaleY");

        Storyboard.SetTarget(fadeAnimationX, FadeScaleTransform);
        Storyboard.SetTarget(fadeAnimationY, FadeScaleTransform);
        Storyboard.SetTargetProperty(fadeAnimationX, "ScaleX");
        Storyboard.SetTargetProperty(fadeAnimationY, "ScaleY");
    }

    private DoubleAnimationUsingKeyFrames GetDotAnimation(double value)
    {
        return new DoubleAnimationUsingKeyFrames
        {
            KeyFrames =
            {
                new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 0 },
                CreateEasingDoubleKeyFrame(TimeSpan.FromSeconds(0.6), value)
            }
        };
    }

    private EasingDoubleKeyFrame CreateEasingDoubleKeyFrame(TimeSpan keyTime, double value, EasingMode easingMode = EasingMode.EaseInOut)
    {
        return new()
        {
            KeyTime = KeyTime.FromTimeSpan(keyTime),
            Value = value,
            EasingFunction = new CubicEase { EasingMode = easingMode }
        };
    }

    private DoubleAnimationUsingKeyFrames GetFadeAnimation()
    {
        return new DoubleAnimationUsingKeyFrames
        {
            KeyFrames =
            {
                new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 0 },
                new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.7)), Value = 0 },
                CreateEasingDoubleKeyFrame(TimeSpan.FromSeconds(1.3), 1.5)
            }
        };
    }

    private void OnInitialScaleAnimationCompleted(object? sender, object e)
    {
        BeginPulseAnimation();
    }

    private void CreatePulseAnimations()
    {
        DoubleAnimationUsingKeyFrames fadeAnimationX = GetPulseAnimation();
        DoubleAnimationUsingKeyFrames fadeAnimationY = GetPulseAnimation();

        _pulseStoryboard.Children.Add(fadeAnimationX);
        _pulseStoryboard.Children.Add(fadeAnimationY);

        Storyboard.SetTarget(fadeAnimationX, FadeScaleTransform);
        Storyboard.SetTarget(fadeAnimationY, FadeScaleTransform);
        Storyboard.SetTargetProperty(fadeAnimationX, "ScaleX");
        Storyboard.SetTargetProperty(fadeAnimationY, "ScaleY");
    }

    private DoubleAnimationUsingKeyFrames GetPulseAnimation()
    {
        return new DoubleAnimationUsingKeyFrames
        {
            KeyFrames = { CreateEasingDoubleKeyFrame(TimeSpan.FromSeconds(1), 1) }
        };
    }

    public void BeginAnimation()
    {
        StopPulseAnimation();
        _initialScaleStoryboard.Begin();
    }

    private void BeginPulseAnimation()
    {
        _pulseStoryboard.Begin();
    }

    public void StopPulseAnimation()
    {
        if (_pulseStoryboard.GetCurrentState() == ClockState.Active)
        {
            _pulseStoryboard.Stop();
        }
    }

    private static void OnIsConnectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CountryPin control)
        {
            return;
        }

        if (!control.IsConnecting)
        {
            control.BeginAnimation();
        }
    }

    private static void OnIsMainWindowActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CountryPin control)
        {
            return;
        }

        if (control.IsMainWindowVisible && !control.IsConnecting
                                        && control._initialScaleStoryboard.GetCurrentState() != ClockState.Active
                                        && control._pulseStoryboard.GetCurrentState() != ClockState.Active)
        {
            control.BeginPulseAnimation();
        }
    }
}