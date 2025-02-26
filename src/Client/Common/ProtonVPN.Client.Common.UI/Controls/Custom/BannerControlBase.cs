/*
 * Copyright (c) 2025 Proton AG
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

using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public abstract class BannerControlBase : Control
{
    private const int WIDTH_THRESHOLD = 600;

    public static readonly DependencyProperty ActionCommandProperty =
        DependencyProperty.Register(nameof(ActionCommand), typeof(ICommand), typeof(BannerControlBase), new PropertyMetadata(default));

    public static readonly DependencyProperty ActionButtonTextProperty =
        DependencyProperty.Register(nameof(ActionButtonText), typeof(string), typeof(BannerControlBase), new PropertyMetadata(default));

    public static readonly DependencyProperty IsActionButtonVisibleProperty =
        DependencyProperty.Register(nameof(IsActionButtonVisible), typeof(bool), typeof(BannerControlBase), new PropertyMetadata(default));

    public static readonly DependencyProperty DismissCommandProperty =
        DependencyProperty.Register(nameof(DismissCommand), typeof(ICommand), typeof(BannerControlBase), new PropertyMetadata(default));

    public static readonly DependencyProperty IsDismissButtonVisibleProperty =
        DependencyProperty.Register(nameof(IsDismissButtonVisible), typeof(bool), typeof(BannerControlBase), new PropertyMetadata(default));

    protected BannerControlBase()
    {
        SizeChanged += OnBannerControlSizeChanged;
    }

    private void OnBannerControlSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ActualWidth >= WIDTH_THRESHOLD)
        {
            VisualStateManager.GoToState(this, "WideState", true);
        }
        else
        {
            VisualStateManager.GoToState(this, "NarrowState", true);
        }
    }

    public ICommand ActionCommand
    {
        get => (ICommand)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public string ActionButtonText
    {
        get => (string)GetValue(ActionButtonTextProperty);
        set => SetValue(ActionButtonTextProperty, value);
    }

    public bool IsActionButtonVisible
    {
        get => (bool)GetValue(IsActionButtonVisibleProperty);
        set => SetValue(IsActionButtonVisibleProperty, value);
    }

    public ICommand DismissCommand
    {
        get => (ICommand)GetValue(DismissCommandProperty);
        set => SetValue(DismissCommandProperty, value);
    }

    public bool IsDismissButtonVisible
    {
        get => (bool)GetValue(IsDismissButtonVisibleProperty);
        set => SetValue(IsDismissButtonVisibleProperty, value);
    }
}