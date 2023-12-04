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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Dialogs.Overlays;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.UI.Countries;

public partial class CityViewModel : ViewModelBase, ISearchableItem
{
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly IConnectionManager _connectionManager;

    [ObservableProperty]
    private bool _isActiveConnection;

    public string CountryCode => Servers.FirstOrDefault()?.ExitCountryCode ?? string.Empty;

    public string Name { get; init; } = string.Empty;

    public CountryFeature CountryFeature { get; init; }

    public List<ServerViewModel> Servers { get; init; } = new();

    public CityViewModel(ILocalizationProvider localizationProvider, IMainViewNavigator mainViewNavigator,
        IConnectionManager connectionManager) : base(localizationProvider)
    {
        _mainViewNavigator = mainViewNavigator;
        _connectionManager = connectionManager;
    }

    [RelayCommand]
    public async Task ConnectAsync()
    {
        await _mainViewNavigator.NavigateToAsync<HomeViewModel>();

        IFeatureIntent? featureIntent = CountryFeature.GetFeatureIntent();
        ILocationIntent locationIntent = new CityStateLocationIntent(CountryCode, Name);

        await _connectionManager.ConnectAsync(new ConnectionIntent(locationIntent, featureIntent));
    }

    [RelayCommand]
    public async Task ShowServerLoadOverlayAsync()
    {
        await _mainViewNavigator.ShowOverlayAsync<ServerLoadOverlayViewModel>();
    }

    public bool MatchesSearchQuery(string query)
    {
        return !string.IsNullOrWhiteSpace(Name) && Name.ContainsIgnoringCase(query);
    }
}