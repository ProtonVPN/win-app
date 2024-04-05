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

public sealed partial class CountryListControl
{
    public static readonly DependencyProperty CountriesProperty =
        DependencyProperty.Register(nameof(Countries), typeof(AdvancedCollectionView), typeof(CountryListControl), new PropertyMetadata(null));

    public static readonly DependencyProperty UpsellBannerProperty =
        DependencyProperty.Register(nameof(UpsellBanner), typeof(FrameworkElement), typeof(CountryListControl), new PropertyMetadata(default));

    public static readonly DependencyProperty IsUpsellBannerVisibleProperty =
        DependencyProperty.Register(nameof(IsUpsellBannerVisible), typeof(bool), typeof(CountryListControl), new PropertyMetadata(default));

    public bool IsUpsellBannerVisible
    {
        get => (bool)GetValue(IsUpsellBannerVisibleProperty);
        set => SetValue(IsUpsellBannerVisibleProperty, value);
    }

    public AdvancedCollectionView Countries
    {
        get => (AdvancedCollectionView)GetValue(CountriesProperty);
        set => SetValue(CountriesProperty, value);
    }

    public FrameworkElement UpsellBanner
    {
        get => (FrameworkElement)GetValue(UpsellBannerProperty);
        set => SetValue(UpsellBannerProperty, value);
    }

    public CountryListControl()
    {
        InitializeComponent();
    }
}