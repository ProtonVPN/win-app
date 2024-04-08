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
using Microsoft.UI.Xaml.Media;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class UpsellFeatureContentControl : ContentControl
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(UpsellFeatureContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty IllustrationSourceProperty =
        DependencyProperty.Register(nameof(IllustrationSource), typeof(ImageSource), typeof(UpsellFeatureContentControl), new PropertyMetadata(default));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public ImageSource IllustrationSource
    {
        get => (ImageSource)GetValue(IllustrationSourceProperty);
        set => SetValue(IllustrationSourceProperty, value);
    }

    public UpsellFeatureContentControl()
    {
        DefaultStyleKey = typeof(UpsellFeatureContentControl);
    }
}