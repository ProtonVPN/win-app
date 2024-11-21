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
using Microsoft.UI.Xaml.Media;

namespace ProtonVPN.Client.Common.UI.AttachedProperties;

public class AncestorSource
{
    public static readonly DependencyProperty AncestorTypeProperty =
        DependencyProperty.RegisterAttached("AncestorType", typeof(Type), typeof(AncestorSource), new PropertyMetadata(default(Type), OnAncestorTypePropertyChanged));

    public static void SetAncestorType(FrameworkElement element, Type value)
    {
        element.SetValue(AncestorTypeProperty, value);
    }

    public static Type GetAncestorType(FrameworkElement element)
    {
        return (Type)element.GetValue(AncestorTypeProperty);
    }

    private static void OnAncestorTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement target)
        {
            if (target.IsLoaded)
            {
                SetDataContext(target);
            }
            else
            {
                target.Loaded += OnTargetLoaded;
            }
        }
    }

    private static void OnTargetLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement target)
        {
            target.Loaded -= OnTargetLoaded;
            SetDataContext(target);
        }
    }

    private static void SetDataContext(FrameworkElement target)
    {
        Type ancestorType = GetAncestorType(target);
        if (ancestorType != null)
        {
            target.DataContext = FindParent(target, ancestorType);
        }
    }

    private static object? FindParent(DependencyObject dependencyObject, Type ancestorType)
    {
        DependencyObject parent = VisualTreeHelper.GetParent(dependencyObject);

        return parent == null
            ? null
            : ancestorType.IsAssignableFrom(parent.GetType())
                ? parent
                : FindParent(parent, ancestorType);
    }
}