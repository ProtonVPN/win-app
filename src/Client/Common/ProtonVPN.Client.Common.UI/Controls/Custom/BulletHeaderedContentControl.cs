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

using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class BulletHeaderedContentControl : HeaderedContentControl
{
    public static readonly DependencyProperty BulletContentProperty =
        DependencyProperty.Register(nameof(BulletContent), typeof(object), typeof(BulletHeaderedContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty BulletSizeProperty =
        DependencyProperty.Register(nameof(BulletSize), typeof(double), typeof(BulletHeaderedContentControl), new PropertyMetadata(24.0));

    public object BulletContent
    {
        get => GetValue(BulletContentProperty);
        set => SetValue(BulletContentProperty, value);
    }

    public double BulletSize
    {
        get => (double)GetValue(BulletSizeProperty);
        set => SetValue(BulletSizeProperty, value);
    }

    public BulletHeaderedContentControl()
    {
        DefaultStyleKey = typeof(BulletHeaderedContentControl);
    }
}