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

using System.Windows.Input;
using Microsoft.UI.Xaml;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class SettingsPageContentControl : PageContentControl
{
    public static readonly DependencyProperty ReconnectCommandProperty =
        DependencyProperty.Register(nameof(ReconnectCommand), typeof(ICommand), typeof(SettingsPageContentControl), new PropertyMetadata(default, OnReconnectCommandPropertyChanged));

    public static readonly DependencyProperty ReconnectCommandParameterProperty =
        DependencyProperty.Register(nameof(ReconnectCommandParameter), typeof(object), typeof(SettingsPageContentControl), new PropertyMetadata(default, OnReconnectCommandPropertyChanged));

    public static readonly DependencyProperty ReconnectCommandTextProperty =
        DependencyProperty.Register(nameof(ReconnectCommandText), typeof(string), typeof(SettingsPageContentControl), new PropertyMetadata(default));

    public ICommand ReconnectCommand
    {
        get => (ICommand)GetValue(ReconnectCommandProperty);
        set => SetValue(ReconnectCommandProperty, value);
    }

    public object ReconnectCommandParameter
    {
        get => GetValue(ReconnectCommandParameterProperty);
        set => SetValue(ReconnectCommandParameterProperty, value);
    }

    public string ReconnectCommandText
    {
        get => (string)GetValue(ReconnectCommandTextProperty);
        set => SetValue(ReconnectCommandTextProperty, value);
    }

    protected bool IsReconnectCommandEmpty => ReconnectCommand == null || !ReconnectCommand.CanExecute(ReconnectCommandParameter);

    public SettingsPageContentControl()
    {
        DefaultStyleKey = typeof(SettingsPageContentControl);
    }

    protected override void InvalidateHeaderVisibility()
    {
        if (PART_HeaderContainer == null)
        {
            return;
        }

        PART_HeaderContainer.Visibility = IsPageHeaderEmpty && IsBackCommandEmpty && IsReconnectCommandEmpty
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private static void OnReconnectCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SettingsPageContentControl control)
        {
            control.InvalidateHeaderVisibility();
        }
    }
}