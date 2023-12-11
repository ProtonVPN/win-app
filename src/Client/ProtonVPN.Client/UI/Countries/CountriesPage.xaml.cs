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

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.UI.Settings;

namespace ProtonVPN.Client.UI.Countries;

public sealed partial class CountriesPage
{
    public CountriesPage()
    {
        ViewModel = App.GetService<CountriesViewModel>();
        InitializeComponent();

        ViewModel.CountriesFeatureTabsViewNavigator.Frame = CountriesFeatureTabFrame;
    }

    public CountriesViewModel ViewModel { get; }

    private async void OnNavigationViewItemInvokedAsync(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        bool isNavigationCompleted = false;

        NavigationViewItem? selectedItem = args.InvokedItemContainer as NavigationViewItem;

        if (selectedItem?.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
        {
            isNavigationCompleted = await ViewModel.CountriesFeatureTabsViewNavigator.NavigateToAsync(pageKey);
        }

        if (!isNavigationCompleted)
        {
            NavigationViewControl.SelectedItem = ViewModel.SelectedFeatureTab;
        }
    }
}