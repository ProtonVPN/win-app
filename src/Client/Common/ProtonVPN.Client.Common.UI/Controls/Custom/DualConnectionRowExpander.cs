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
using ProtonVPN.Client.Common.UI.Controls.Bases;
using Windows.Foundation;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class DualConnectionRowExpander : DualConnectionRowButtonBase
{
    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(DualConnectionRowExpander), new PropertyMetadata(default, OnIsExpandedPropertyChanged));

    public static readonly DependencyProperty ExpanderContentProperty =
        DependencyProperty.Register(nameof(ExpanderContent), typeof(object), typeof(DualConnectionRowExpander), new PropertyMetadata(default));

    private static readonly BringIntoViewOptions _bringIntoViewOptions = new()
    {
        VerticalAlignmentRatio = 0.2,
        AnimationDesired = true,
        TargetRect = new Rect(0, 0, 0, 44)
    };

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public object ExpanderContent
    {
        get => GetValue(ExpanderContentProperty);
        set => SetValue(ExpanderContentProperty, value);
    }

    private static void OnIsExpandedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DualConnectionRowExpander control && control.IsExpanded)
        {
            control.StartBringIntoView(_bringIntoViewOptions);
        }
    }
}