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

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.Controls;

public sealed partial class ConnectionItemsControl
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(ConnectionItemsControl), new PropertyMetadata(default));

    public static readonly DependencyProperty ScrollBarVisibilityProperty =
        DependencyProperty.Register(nameof(ScrollBarVisibility), typeof(ScrollBarVisibility), typeof(ConnectionItemsControl), new PropertyMetadata(ScrollBarVisibility.Auto));

    public object ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public ScrollBarVisibility ScrollBarVisibility
    {
        get => (ScrollBarVisibility)GetValue(ScrollBarVisibilityProperty);
        set => SetValue(ScrollBarVisibilityProperty, value);
    }

    public ConnectionItemsControl()
    {
        InitializeComponent();
    }
}