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

using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class SettingsPageContentControl : PageContentControl
{
    public static readonly DependencyProperty ApplyCommandProperty =
        DependencyProperty.Register(nameof(ApplyCommand), typeof(ICommand), typeof(SettingsPageContentControl), new PropertyMetadata(default, OnApplyCommandPropertyChanged));

    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(SettingsPageContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty ApplyCommandParameterProperty =
        DependencyProperty.Register(nameof(ApplyCommandParameter), typeof(object), typeof(SettingsPageContentControl), new PropertyMetadata(default, OnApplyCommandPropertyChanged));

    public static readonly DependencyProperty ApplyCommandTextProperty =
        DependencyProperty.Register(nameof(ApplyCommandText), typeof(string), typeof(SettingsPageContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty CloseButtonTextProperty =
        DependencyProperty.Register(nameof(CloseButtonText), typeof(string), typeof(SettingsPageContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty StickyContentProperty =
        DependencyProperty.Register(nameof(StickyContent), typeof(object), typeof(SettingsPageContentControl), new PropertyMetadata(default));

    public ICommand ApplyCommand
    {
        get => (ICommand)GetValue(ApplyCommandProperty);
        set => SetValue(ApplyCommandProperty, value);
    }

    public ICommand CloseCommand
    {
        get => (ICommand)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public object ApplyCommandParameter
    {
        get => GetValue(ApplyCommandParameterProperty);
        set => SetValue(ApplyCommandParameterProperty, value);
    }

    public string ApplyCommandText
    {
        get => (string)GetValue(ApplyCommandTextProperty);
        set => SetValue(ApplyCommandTextProperty, value);
    }

    public string CloseButtonText
    {
        get => (string)GetValue(CloseButtonTextProperty);
        set => SetValue(CloseButtonTextProperty, value);
    }

    public object StickyContent
    {
        get => GetValue(StickyContentProperty);
        set => SetValue(StickyContentProperty, value);
    }

    protected bool IsApplyCommandEmpty => ApplyCommand == null || !ApplyCommand.CanExecute(ApplyCommandParameter);

    public SettingsPageContentControl()
    {
        DefaultStyleKey = typeof(SettingsPageContentControl);
        Loaded += OnSettingsPageContentControlLoaded;
    }

    private void OnSettingsPageContentControlLoaded(object sender, RoutedEventArgs e)
    {
        FrameworkElement? firstFocusableElement = (FrameworkElement?)FocusManager.FindFirstFocusableElement(this);
        firstFocusableElement?.Focus(FocusState.Pointer);
    }

    protected override void InvalidateHeaderVisibility()
    {
        if (PART_HeaderContainer == null)
        {
            return;
        }

        PART_HeaderContainer.Visibility = IsPageHeaderEmpty && IsBackCommandEmpty && IsApplyCommandEmpty
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private static void OnApplyCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SettingsPageContentControl control)
        {
            control.InvalidateHeaderVisibility();
        }
    }
}