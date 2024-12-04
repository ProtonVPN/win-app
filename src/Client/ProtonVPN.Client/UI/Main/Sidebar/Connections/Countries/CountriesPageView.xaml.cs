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
using ProtonVPN.Client.Core.Bases;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Countries;

public sealed partial class CountriesPageView : IContextAware
{
    public CountriesPageViewModel ViewModel { get; }

    public CountriesPageView()
    {
        ViewModel = App.GetService<CountriesPageViewModel>();

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        ViewModel.ResetContentScrollRequested += OnResetContentScrollRequested;
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Activate();

        // Pivot control selected item does not refresh correctly after login
        CountriesPivot.SelectedIndex = ViewModel.SelectedCountriesComponent.SortIndex;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();
    }

    private void OnResetContentScrollRequested(object? sender, EventArgs e)
    {
        ConnectionItemsControl.ResetContentScroll();
    }
}