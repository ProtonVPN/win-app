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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class WindowContentControl : ContentControl
{
    public static readonly DependencyProperty IconSourceProperty =
        DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(WindowContentControl), new PropertyMetadata(default, OnIconSourcePropertyChanged));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(WindowContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty IsTitleBarVisibleProperty =
        DependencyProperty.Register(nameof(IsTitleBarVisible), typeof(bool), typeof(WindowContentControl), new PropertyMetadata(true));

    public static readonly DependencyProperty TitleBarOpacityProperty =
        DependencyProperty.Register(nameof(TitleBarOpacity), typeof(double), typeof(WindowContentControl), new PropertyMetadata(1.0));

    public static readonly DependencyProperty TitleBarHeightProperty =
        DependencyProperty.Register(nameof(TitleBarHeight), typeof(double), typeof(WindowContentControl), new PropertyMetadata(36.0));

    public static readonly DependencyProperty TitleBarPaddingProperty =
        DependencyProperty.Register(nameof(TitleBarPadding), typeof(Thickness), typeof(WindowContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty TitleBarButtonsLengthProperty =
        DependencyProperty.Register(nameof(TitleBarButtonsLength), typeof(GridLength), typeof(WindowContentControl), new PropertyMetadata(140.0));

    public static readonly DependencyProperty InnerBackgroundProperty =
        DependencyProperty.Register(nameof(InnerBackground), typeof(Brush), typeof(WindowContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty TitleBarSpacingProperty =
        DependencyProperty.Register(nameof(TitleBarSpacing), typeof(double), typeof(WindowContentControl), new PropertyMetadata(8.0));

    public static readonly DependencyProperty TitleBarLeftContentProperty =
        DependencyProperty.Register(nameof(TitleBarLeftContent), typeof(object), typeof(WindowContentControl), new PropertyMetadata(default));

    private Image? PART_WindowIcon;

    public Brush InnerBackground
    {
        get => (Brush)GetValue(InnerBackgroundProperty);
        set => SetValue(InnerBackgroundProperty, value);
    }

    public double TitleBarSpacing
    {
        get => (double)GetValue(TitleBarSpacingProperty);
        set => SetValue(TitleBarSpacingProperty, value);
    }

    public bool IsTitleBarVisible
    {
        get => (bool)GetValue(IsTitleBarVisibleProperty);
        set => SetValue(IsTitleBarVisibleProperty, value);
    }

    public double TitleBarHeight
    {
        get => (double)GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }

    public Thickness TitleBarPadding
    {
        get => (Thickness)GetValue(TitleBarPaddingProperty);
        set => SetValue(TitleBarPaddingProperty, value);
    }

    public GridLength TitleBarButtonsLength
    {
        get => (GridLength)GetValue(TitleBarButtonsLengthProperty);
        set => SetValue(TitleBarButtonsLengthProperty, value);
    }

    public ImageSource IconSource
    {
        get => (ImageSource)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public double TitleBarOpacity
    {
        get => (double)GetValue(TitleBarOpacityProperty);
        set => SetValue(TitleBarOpacityProperty, value);
    }

    public object TitleBarLeftContent
    {
        get => GetValue(TitleBarLeftContentProperty);
        set => SetValue(TitleBarLeftContentProperty, value);
    }

    public WindowContentControl()
    {
        DefaultStyleKey = typeof(WindowContentControl);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PART_WindowIcon = GetTemplateChild("WindowIcon") as Image;
        InvalidateIconVisibility();
    }

    private static void OnIconSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WindowContentControl control)
        {
            control.InvalidateIconVisibility();
        }
    }

    private void InvalidateIconVisibility()
    {
        if (PART_WindowIcon == null)
        {
            return;
        }

        PART_WindowIcon.Visibility = IconSource is null
            ? Visibility.Collapsed
            : Visibility.Visible;
    }
}