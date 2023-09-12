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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Models;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Countries.Pages;
using ProtonVPN.Client.UI.Home;

namespace ProtonVPN.Client.UI.Countries;

public partial class CountriesViewModel : NavigationPageViewModelBase
{
    private readonly IConnectionManager _connectionManager;
    private readonly IRecentConnectionsProvider _recentConnectionsProvider;

    [ObservableProperty]
    private string _exitCountryCode;

    [ObservableProperty]
    private string? _entryCountryCode;

    [ObservableProperty]
    private string? _cityState;

    [ObservableProperty]
    private string? _serverNumber;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSecureCore))]
    private string _selectedFeature;

    public ObservableCollection<Country> Countries { get; }

    public ObservableCollection<string> Features { get; }

    public override string? Title => Localizer.Get("Countries_Page_Title");

    public override IconElement Icon { get; } = new Earth();

    public override bool IsBackEnabled => false;

    public bool IsSecureCore => SelectedFeature == "Secure Core";

    public CountriesViewModel(IMainViewNavigator viewNavigator,
        IConnectionManager connectionManager,
        IRecentConnectionsProvider recentConnectionsProvider,
        ILocalizationProvider localizationProvider)
        : base(viewNavigator, localizationProvider)
    {
        _connectionManager = connectionManager;
        _recentConnectionsProvider = recentConnectionsProvider;

        _exitCountryCode = string.Empty;

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
    public async Task NavigateToCountryAsync(Country country)
    {
        await ViewNavigator.NavigateToAsync<CountryViewModel>(country);
    }

    public bool IsNotEmpty(string value)
    {
        return !string.IsNullOrEmpty(value);
    }

    [RelayCommand]
    public async Task ConnectAsync()
    {
        await ViewNavigator.NavigateToAsync<HomeViewModel>();

        ILocationIntent locationIntent = IsNotEmpty(ExitCountryCode) && IsNotEmpty(CityState) && IsNotEmpty(ServerNumber)
            ? new ServerLocationIntent(ExitCountryCode, CityState, int.Parse(ServerNumber))
            : IsNotEmpty(ExitCountryCode) && IsNotEmpty(CityState)
                ? new CityStateLocationIntent(ExitCountryCode, CityState)
                : new CountryLocationIntent(ExitCountryCode);

        IFeatureIntent? featureIntent = SelectedFeature switch
        {
            "Secure Core" => new SecureCoreFeatureIntent(EntryCountryCode),
            "TOR" => new TorFeatureIntent(),
            "P2P" => new P2PFeatureIntent(),
            _ => null
        };

        await _connectionManager.ConnectAsync(featureIntent != null
            ? new ConnectionIntent(locationIntent, featureIntent)
            : new ConnectionIntent(locationIntent));
    }

    [RelayCommand]
    public async Task FreeConnectAsync()
    {
        await ViewNavigator.NavigateToAsync<HomeViewModel>();

        ILocationIntent locationIntent = IsNotEmpty(ExitCountryCode) && IsNotEmpty(ServerNumber)
            ? new FreeServerLocationIntent(ExitCountryCode, int.Parse(ServerNumber))
            : new CountryLocationIntent(ExitCountryCode);

        IFeatureIntent? featureIntent = SelectedFeature switch
        {
            "Secure Core" => new SecureCoreFeatureIntent(EntryCountryCode),
            "TOR" => new TorFeatureIntent(),
            "P2P" => new P2PFeatureIntent(),
            _ => null
        };

        await _connectionManager.ConnectAsync(featureIntent != null
            ? new ConnectionIntent(locationIntent, featureIntent)
            : new ConnectionIntent(locationIntent));
    }

    [RelayCommand]
    public async Task SimulateManyConnectionsAsync()
    {
        await ViewNavigator.NavigateToAsync<HomeViewModel>();

        List<IConnectionIntent> intents = new()
        {
            new ConnectionIntent(new CityStateLocationIntent("US", "New York City"), new SecureCoreFeatureIntent()),
            new ConnectionIntent(new CountryLocationIntent("LT"), new P2PFeatureIntent()),
            new ConnectionIntent(new ServerLocationIntent("GB", "London", 10)),
            new ConnectionIntent(new CountryLocationIntent("AE"), new SecureCoreFeatureIntent("SE")),
            new ConnectionIntent(new ServerLocationIntent("CH", "Zurich", 30), new TorFeatureIntent()),
            new ConnectionIntent(new CityStateLocationIntent("AU", "Sydney"))
        };

        foreach (IConnectionIntent intent in intents)
        {
            await _connectionManager.ConnectAsync(intent);
        }

        IEnumerable<IRecentConnection> recentConnections = _recentConnectionsProvider.GetRecentConnections().Take(2);

        foreach (IRecentConnection? connection in recentConnections)
        {
            _recentConnectionsProvider.Pin(connection);
        }
    }
}