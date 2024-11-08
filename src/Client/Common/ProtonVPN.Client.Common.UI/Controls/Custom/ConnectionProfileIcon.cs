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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Enums;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class ConnectionProfileIcon : Control
{
    public static readonly DependencyProperty CountryCodeProperty =
        DependencyProperty.Register(nameof(CountryCode), typeof(string), typeof(ConnectionProfileIcon), new PropertyMetadata(default));

    public static readonly DependencyProperty ProfileColorProperty =
        DependencyProperty.Register(nameof(ProfileColor), typeof(ProfileColor), typeof(ConnectionProfileIcon), new PropertyMetadata(ProfileColor.Purple));

    public static readonly DependencyProperty ProfileCategoryProperty =
        DependencyProperty.Register(nameof(ProfileCategory), typeof(ProfileCategory), typeof(ConnectionProfileIcon), new PropertyMetadata(ProfileCategory.Terminal));

    public static readonly DependencyProperty IsFlagVisibleProperty =
        DependencyProperty.Register(nameof(IsFlagVisible), typeof(bool), typeof(ConnectionProfileIcon), new PropertyMetadata(true));

    public static readonly DependencyProperty FlagTypeProperty =
        DependencyProperty.Register(nameof(FlagType), typeof(FlagType), typeof(ConnectionProfileIcon), new PropertyMetadata(default));

    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(ConnectionProfileIcon), new PropertyMetadata(default));

    public string CountryCode
    {
        get => (string)GetValue(CountryCodeProperty);
        set => SetValue(CountryCodeProperty, value);
    }

    public ProfileColor ProfileColor
    {
        get => (ProfileColor)GetValue(ProfileColorProperty);
        set => SetValue(ProfileColorProperty, value);
    }

    public ProfileCategory ProfileCategory
    {
        get => (ProfileCategory)GetValue(ProfileCategoryProperty);
        set => SetValue(ProfileCategoryProperty, value);
    }

    public bool IsFlagVisible
    {
        get => (bool)GetValue(IsFlagVisibleProperty);
        set => SetValue(IsFlagVisibleProperty, value);
    }

    public FlagType FlagType
    {
        get => (FlagType)GetValue(FlagTypeProperty);
        set => SetValue(FlagTypeProperty, value);
    }

    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }
}