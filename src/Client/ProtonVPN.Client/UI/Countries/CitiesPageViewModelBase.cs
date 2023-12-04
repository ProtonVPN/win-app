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

using System.Collections;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Home;

namespace ProtonVPN.Client.UI.Countries;

public abstract partial class CitiesPageViewModelBase : CountryTabViewModelBase
{
    private readonly IConnectionManager _connectionManager;
    protected readonly IServersLoader ServersLoader;

    public override IconElement? Icon => null;

    protected CitiesPageViewModelBase(
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IOverlayActivator overlayActivator,
        IServersLoader serversLoader,
        ICountryFeatureTabsViewNavigator viewNavigator,
        CountryViewModelsFactory countryViewModelsFactory,
        ILocalizationProvider localizationProvider)
        : base(mainViewNavigator,
               overlayActivator,
               countryViewModelsFactory,
               viewNavigator,
               localizationProvider)
    {
        _connectionManager = connectionManager;
        ServersLoader = serversLoader;
    }

    [RelayCommand]
    public async Task ConnectAsync(CityViewModel cityViewModel)
    {
        await MainViewNavigator.NavigateToAsync<HomeViewModel>();
        await _connectionManager.ConnectAsync(
            new ConnectionIntent(new CityStateLocationIntent(CurrentCountryCode, cityViewModel.Name)));
    }

    public override void OnNavigatedTo(object parameter)
    {
        CurrentCountryCode = parameter as string ?? string.Empty;

        base.OnNavigatedTo(parameter);
    }

    protected override IList GetItems()
    {
        return GetCities().Select(GetCity).ToList();
    }

    protected override IList<SortDescription> GetSortDescriptions()
    {
        return new List<SortDescription> { new(nameof(CityViewModel.Name), SortDirection.Ascending) };
    }

    protected abstract IEnumerable<City> GetCities();
}