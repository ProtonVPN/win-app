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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Home;

namespace ProtonVPN.Client.UI.Countries;

public partial class CountryViewModel : ViewModelBase, IComparable
{
    private readonly IMainViewNavigator _mainViewNavigator;

    public string ExitCountryCode { get; init; } = string.Empty;

    public string ExitCountryName { get; init; } = string.Empty;

    public string EntryCountryCode { get; init; } = string.Empty;

    public string EntryCountryName { get; init; } = string.Empty;

    public string SecondaryActionLabel { get; init; } = string.Empty;

    public bool IsUnderMaintenance { get; init; }

    public bool IsSecureCore { get; init; }

    public CountryFeature CountryFeature { get; init; }

    public CountryViewModel(ILocalizationProvider localizationProvider, IMainViewNavigator mainViewNavigator) :
        base(localizationProvider)
    {
        _mainViewNavigator = mainViewNavigator;
    }

    [RelayCommand]
    public async Task ConnectAsync(CountryViewModel country)
    {
        await _mainViewNavigator.NavigateToAsync<HomeViewModel>();
        // TODO: connect
    }

    [RelayCommand]
    public async Task NavigateToCountryAsync(CountryViewModel country)
    {
        await _mainViewNavigator.NavigateToAsync<CountryTabViewModel>(country);
    }

    public int CompareTo(object? obj)
    {
        if (obj is not CountryViewModel country)
        {
            return 0;
        }

        if (string.IsNullOrEmpty(ExitCountryCode) && !string.IsNullOrEmpty(country.ExitCountryCode))
        {
            return -1;
        }

        if (string.IsNullOrEmpty(country.ExitCountryCode) && !string.IsNullOrEmpty(ExitCountryCode))
        {
            return 1;
        }

        return string.Compare(ExitCountryName, country.ExitCountryName, StringComparison.OrdinalIgnoreCase);
    }
}