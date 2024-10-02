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

using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace ProtonVPN.Client.Legacy.UI.Connections.Countries;

public sealed partial class CountriesPage
{
    public CountriesPageViewModel ViewModel { get; }

    public CountriesPage()
    {
        ViewModel = App.GetService<CountriesPageViewModel>();
        InitializeComponent();
    }

    protected override void OnKeyDown(KeyRoutedEventArgs e)
    {
        PageContentHost.FocusSearchQueryBox(e.Key);

        base.OnKeyDown(e);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        PageContentHost.FocusSearchQueryBoxWithDelayAsync();
    }
}