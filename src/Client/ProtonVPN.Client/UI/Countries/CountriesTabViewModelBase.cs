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
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Countries.Controls;

namespace ProtonVPN.Client.UI.Countries;

public abstract partial class CountriesTabViewModelBase : PageViewModelBase<ICountriesFeatureTabsViewNavigator>, ICountriesTabViewModelBase
{
    protected readonly IMainViewNavigator MainViewNavigator;
    protected readonly IOverlayActivator OverlayActivator;
    protected readonly IServersLoader ServersLoader;
    protected readonly NoSearchResultsViewModel NoSearchResultsViewModel;

    private readonly CountryViewModelsFactory _countryViewModelsFactory;

    protected string LastSearchQuery = string.Empty;

    public int TotalCountries => string.IsNullOrWhiteSpace(LastSearchQuery) ? Countries.Count - 1 : Countries.Count;
    public bool HasCountries => TotalCountries > 0;
    public bool HasCities => Cities.Count > 0;
    public bool HasServers => Servers.Count > 0;
    public bool HasItems => HasCountries || HasCities || HasServers;

    [ObservableProperty]
    private AdvancedCollectionView _countries = new();

    [ObservableProperty]
    private AdvancedCollectionView _cities = new();

    [ObservableProperty]
    private AdvancedCollectionView _servers = new();

    protected CountriesTabViewModelBase(
        IMainViewNavigator mainViewNavigator,
        IOverlayActivator overlayActivator,
        IServersLoader serversLoader,
        ICountriesFeatureTabsViewNavigator countriesFeatureTabsViewNavigator,
        ILocalizationProvider localizationProvider,
        NoSearchResultsViewModel noSearchResultsViewModel,
        CountryViewModelsFactory countryViewModelsFactory) 
        : base(countriesFeatureTabsViewNavigator, 
               localizationProvider)
    {
        MainViewNavigator = mainViewNavigator;
        OverlayActivator = overlayActivator;    
        ServersLoader = serversLoader;
        NoSearchResultsViewModel = noSearchResultsViewModel;
        _countryViewModelsFactory = countryViewModelsFactory;
    }

    public abstract IconElement? Icon { get; }

    public void LoadItems()
    {
        Countries = new AdvancedCollectionView(GetCountryViewModels(), true);
        Countries.SortDescriptions.Add(new(SortDirection.Ascending));

        Cities = new AdvancedCollectionView(GetCityViewModels(), true);
        Cities.SortDescriptions.Add(new(nameof(CityViewModel.Name), SortDirection.Ascending));
        Cities.Filter = _ => false;

        Servers = new AdvancedCollectionView(GetServerViewModels(), true);
        Servers.SortDescriptions.Add(new(nameof(ServerViewModel.IsUnderMaintenance), SortDirection.Ascending));
        Servers.SortDescriptions.Add(new(nameof(ServerViewModel.Load), SortDirection.Ascending));
        Servers.Filter = _ => false;

        NotifyPropertyChanges();
    }

    public virtual void FilterItems(string query)
    {
        LastSearchQuery = query;
       
        Countries.Filter = GetCountriesFilter(query);
        Cities.Filter = GetHiddenItemsFilter(query);
        Servers.Filter = GetHiddenItemsFilter(query);

        NotifyPropertyChanges();
    }

    private Predicate<object>? GetCountriesFilter(string query)
    {
        return string.IsNullOrWhiteSpace(LastSearchQuery) ? null : o => MatchesSearchQuery(o, query);
    }

    private Predicate<object> GetHiddenItemsFilter(string query)
    {
        return string.IsNullOrWhiteSpace(LastSearchQuery) ? _ => false : o => MatchesSearchQuery(o, query);
    }

    private void NotifyPropertyChanges()
    {
        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(HasCountries));
        OnPropertyChanged(nameof(HasCities));
        OnPropertyChanged(nameof(HasServers));
        OnPropertyChanged(nameof(TotalCountries));
    }

    protected abstract IEnumerable<string> GetCountryCodes();

    protected abstract IEnumerable<City> GetCities();

    protected abstract IEnumerable<Server> GetServers();

    protected abstract int GetCountryItemsCount(string countryCode);

    protected abstract CountryFeature CountryFeature { get; }

    private List<CountryViewModel> GetCountryViewModels()
    {
        return GetCountryCodes()
            .Select(GetCountryViewModel)
            .Prepend(_countryViewModelsFactory.GetFastestCountryViewModel(CountryFeature))
            .ToList();
    }

    private CountryViewModel GetCountryViewModel(string countryCode)
    {
        return _countryViewModelsFactory.GetCountryViewModel(countryCode, CountryFeature, GetCountryItemsCount(countryCode));
    }

    private List<CityViewModel> GetCityViewModels()
    {
        return GetCities().Select(city =>
        {
            List<ServerViewModel> servers = ServersLoader.GetServersByCity(city)
                .Select(_countryViewModelsFactory.GetServerViewModel).ToList();
            return _countryViewModelsFactory.GetCityViewModel(city, servers, CountryFeature);
        }).ToList();
    }

    private List<ServerViewModel> GetServerViewModels()
    {
        return GetServers()
            .Select(_countryViewModelsFactory.GetServerViewModel)
            .ToList();
    }

    protected bool MatchesSearchQuery(object o, string query)
    {
        return o is ISearchableItem item && item.MatchesSearchQuery(query);
    }

    public override void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        if (!string.IsNullOrEmpty(LastSearchQuery))
        {
            FilterItems(LastSearchQuery);
        }
    }
}