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
using ProtonVPN.Client.Common.Enums;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class SimpleCountryFlag : Control
{
    public static readonly DependencyProperty FlagTypeProperty =
        DependencyProperty.Register(nameof(FlagType), typeof(FlagType), typeof(SimpleCountryFlag), new PropertyMetadata(FlagType.Fastest));

    public static readonly DependencyProperty CountryCodeProperty =
        DependencyProperty.Register(nameof(CountryCode), typeof(string), typeof(SimpleCountryFlag), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsSecureCoreProperty =
        DependencyProperty.Register(nameof(IsSecureCore), typeof(bool), typeof(SimpleCountryFlag), new PropertyMetadata(false));

    public static readonly DependencyProperty UseShorterSecureCoreOutlineProperty =
        DependencyProperty.Register(nameof(UseShorterSecureCoreOutline), typeof(bool), typeof(SimpleCountryFlag), new PropertyMetadata(false));

    public FlagType FlagType
    {
        get => (FlagType)GetValue(FlagTypeProperty);
        set => SetValue(FlagTypeProperty, value);
    }

    public string CountryCode
    {
        get => (string)GetValue(CountryCodeProperty);
        set => SetValue(CountryCodeProperty, value);
    }

    public bool IsSecureCore
    {
        get => (bool)GetValue(IsSecureCoreProperty);
        set => SetValue(IsSecureCoreProperty, value);
    }

    public bool UseShorterSecureCoreOutline
    {
        get => (bool)GetValue(UseShorterSecureCoreOutlineProperty);
        set => SetValue(UseShorterSecureCoreOutlineProperty, value);
    }
}
