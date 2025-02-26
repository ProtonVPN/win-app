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

using Microsoft.UI.Xaml;
using ProtonVPN.Client.Common.Enums;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class ProminentBannerControl : BannerControlBase
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty BannerStyleProperty =
        DependencyProperty.Register(nameof(BannerStyle), typeof(ProminentBannerStyle), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public ProminentBannerStyle BannerStyle
    {
        get => (ProminentBannerStyle)GetValue(BannerStyleProperty);
        set => SetValue(BannerStyleProperty, value);
    }
}