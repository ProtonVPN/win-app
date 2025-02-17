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

namespace ProtonVPN.Client.Common.UI.AttachedProperties;

public class CustomTags
{
    public static readonly DependencyProperty IsTaggedProperty =
        DependencyProperty.RegisterAttached("IsTagged", typeof(bool), typeof(CustomTags), new PropertyMetadata(default));

    public static readonly DependencyProperty TagProperty =
        DependencyProperty.RegisterAttached("Tag", typeof(string), typeof(CustomTags), new PropertyMetadata(null));

    public static bool GetIsTagged(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsTaggedProperty);
    }

    public static void SetIsTagged(DependencyObject obj, bool value)
    {
        obj.SetValue(IsTaggedProperty, value);
    }

    public static string GetTag(DependencyObject obj)
    {
        return (string)obj.GetValue(TagProperty);
    }

    public static void SetTag(DependencyObject obj, string value)
    {
        obj.SetValue(TagProperty, value);
    }
}