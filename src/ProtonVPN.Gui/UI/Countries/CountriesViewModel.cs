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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.Contracts.ViewModels;
using ProtonVPN.Gui.Models;
using ProtonVPN.Gui.UI.Countries.Pages;

namespace ProtonVPN.Gui.UI.Countries;

public partial class CountriesViewModel : NavigationPageViewModelBase
{
    public CountriesViewModel(INavigationService navigationService)
        : base(navigationService)
    {
        Countries = new ObservableCollection<Country>()
        {
            new Country("France"),
            new Country("Italy"),
            new Country("Lithuania"),
            new Country("Portugal"),
            new Country("Switzerland"),
            new Country("Spain"),
        };
    }

    public ObservableCollection<Country> Countries { get; }

    [RelayCommand]
    public void NavigateToCountry(Country country)
    {
        NavigationService.NavigateTo(typeof(CountryViewModel).FullName, country);
    }

    public override string? Title => Localizer.Get("Countries_Page_Title");

    public override string IconGlyphCode => "\uE909";
}