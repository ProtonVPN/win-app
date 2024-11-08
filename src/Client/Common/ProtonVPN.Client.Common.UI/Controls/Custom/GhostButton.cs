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

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class GhostButton : Button
{
    public static readonly DependencyProperty LeftIconProperty =
        DependencyProperty.Register(nameof(LeftIcon), typeof(IconElement), typeof(GhostButton), new PropertyMetadata(default));

    public static readonly DependencyProperty RightIconProperty =
        DependencyProperty.Register(nameof(RightIcon), typeof(IconElement), typeof(GhostButton), new PropertyMetadata(default));

    public IconElement LeftIcon
    {
        get => (IconElement)GetValue(LeftIconProperty);
        set => SetValue(LeftIconProperty, value);
    }

    public IconElement RightIcon
    {
        get => (IconElement)GetValue(RightIconProperty);
        set => SetValue(RightIconProperty, value);
    }

    public GhostButton()
    {
        DefaultStyleKey = typeof(GhostButton);
    }
}