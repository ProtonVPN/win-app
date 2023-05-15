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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Connection.Contracts;
using ProtonVPN.Connection.Contracts.Models.Intents;
using ProtonVPN.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.Contracts.ViewModels;
using ProtonVPN.Gui.Models;
using ProtonVPN.Gui.UI.Countries.Pages;
using ProtonVPN.Gui.UI.Home;

namespace ProtonVPN.Gui.UI.Countries;

public partial class CountriesViewModel : NavigationPageViewModelBase
{
    private readonly IConnectionService _connectionService;

    [ObservableProperty]
    private string _exitCountryCode;

    [ObservableProperty]
    private string _entryCountryCode;

    [ObservableProperty]
    private string _cityState;

    [ObservableProperty]
    private string _serverNumber;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSecureCore))]
    private string _selectedFeature;

    public ObservableCollection<Country> Countries { get; }

    public ObservableCollection<string> Features { get; }

    public override string? Title => Localizer.Get("Countries_Page_Title");

    public override string IconGlyphCode => "\uE909";

    public bool IsSecureCore => SelectedFeature == "Secure Core";

    public CountriesViewModel(INavigationService navigationService, IConnectionService connectionService)
                                                    : base(navigationService)
    {
        _connectionService = connectionService;

        Countries = new ObservableCollection<Country>()
        {
            new Country("France"),
            new Country("Italy"),
            new Country("Lithuania"),
            new Country("Portugal"),
            new Country("Switzerland"),
            new Country("Spain"),
        };

        Features = new ObservableCollection<string>()
        {
            "None",
            "Secure Core",
            "TOR",
            "P2P"
        };
        _selectedFeature = Features.First();
    }

    [RelayCommand]
    public void NavigateToCountry(Country country)
    {
        NavigationService.NavigateTo(typeof(CountryViewModel).FullName, country);
    }

    public bool IsNotEmpty(string value)
    {
        return !value.IsNullOrEmpty();
    }

    [RelayCommand]
    public async Task ConnectAsync()
    {
        NavigationService.NavigateTo(typeof(HomeViewModel).FullName);

        var locationIntent = IsNotEmpty(ServerNumber)
            ? new ServerLocationIntent(ExitCountryCode, CityState, int.Parse(ServerNumber))
            : IsNotEmpty(CityState)
                ? new CityStateLocationIntent(ExitCountryCode, CityState)
                : new CountryLocationIntent(ExitCountryCode);

        IFeatureIntent? featureIntent = SelectedFeature switch
        {
            "Secure Core" => new SecureCoreFeatureIntent(EntryCountryCode),
            "TOR" => new TorFeatureIntent(),
            "P2P" => new P2PFeatureIntent(),
            _ => (IFeatureIntent?)null
        };

        //await _connectionService.ConnectAsync(new ConnectionIntent(locationIntent), token);
        await _connectionService.ConnectAsync(new ConnectionIntent(locationIntent, featureIntent));
    }
}