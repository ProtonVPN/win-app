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

using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml;

namespace ProtonVPN.Client.UI.Countries.Controls;

public sealed partial class SecureCoreCountryListControl
{
    public static readonly DependencyProperty SecureCoreCountriesProperty = DependencyProperty.Register(
        nameof(SecureCoreCountries),
        typeof(AdvancedCollectionView),
        typeof(SecureCoreCountryListControl),
        new PropertyMetadata(null));

    public AdvancedCollectionView SecureCoreCountries
    {
        get => (AdvancedCollectionView)GetValue(SecureCoreCountriesProperty);
        set => SetValue(SecureCoreCountriesProperty, value);
    }

    public SecureCoreCountryListControl()
    {
        InitializeComponent();
    }
}