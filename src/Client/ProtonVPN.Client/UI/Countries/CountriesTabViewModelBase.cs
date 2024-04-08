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
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Activation.Custom;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Countries.Controls;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Countries;

public abstract partial class CountriesTabViewModelBase : PageViewModelBase<ICountriesFeatureTabsViewNavigator>, ICountriesTabViewModelBase,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    protected readonly IMainViewNavigator MainViewNavigator;
    protected readonly IOverlayActivator OverlayActivator;
    protected readonly IServersLoader ServersLoader;
    protected readonly NoSearchResultsViewModel NoSearchResultsViewModel;
    protected readonly ISettings Settings;
    private readonly IUpsellCarouselDialogActivator _upsellCarouselDialogActivator;

    protected string LastSearchQuery = string.Empty;
    private readonly CountryViewModelsFactory _countryViewModelsFactory;

    [ObservableProperty]
    private int _totalCountries;

    [ObservableProperty]
    private AdvancedCollectionView _countries = new();

    [ObservableProperty]
    private AdvancedCollectionView _cities = new();

    [ObservableProperty]
    private AdvancedCollectionView _servers = new();

    [ObservableProperty]
    private AdvancedCollectionView _secureCoreCountries = new();

    public bool HasCountries => TotalCountries > 0;
    public bool HasCities => Cities.Count > 0;
    public bool HasServers => Servers.Count > 0;
    public bool HasSecureCoreCountries => SecureCoreCountries.Count > 0;
    public bool HasItems => HasCountries || HasCities || HasServers || HasSecureCoreCountries;

    public bool IsUpsellBannerVisible => !Settings.IsPaid;

    public abstract IconElement? Icon { get; }

    protected abstract CountryFeature CountryFeature { get; }

    protected ModalSources UpsellModalSources => CountryFeature.GetUpsellModalSources();

    protected CountriesTabViewModelBase(
                    IMainViewNavigator mainViewNavigator,
        IOverlayActivator overlayActivator,
        IServersLoader serversLoader,
        ICountriesFeatureTabsViewNavigator countriesFeatureTabsViewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        NoSearchResultsViewModel noSearchResultsViewModel,
        ISettings settings,
        CountryViewModelsFactory countryViewModelsFactory,
        IUpsellCarouselDialogActivator upsellCarouselDialogActivator)
        : base(countriesFeatureTabsViewNavigator, localizationProvider, logger, issueReporter)
    {
        MainViewNavigator = mainViewNavigator;
        OverlayActivator = overlayActivator;
        ServersLoader = serversLoader;
        NoSearchResultsViewModel = noSearchResultsViewModel;
        Settings = settings;
        _countryViewModelsFactory = countryViewModelsFactory;
        _upsellCarouselDialogActivator = upsellCarouselDialogActivator;
    }

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

        SecureCoreCountries = new AdvancedCollectionView(GetSecureCoreCountryViewModels(), true);
        SecureCoreCountries.SortDescriptions.Add(new(SortDirection.Ascending));
        SecureCoreCountries.Filter = _ => false;

        NotifyPropertyChanges();
    }

    public virtual void FilterItems(string query)
    {
        LastSearchQuery = query;

        Countries.Filter = GetCountriesFilter(query);
        Cities.Filter = GetHiddenItemsFilter(query);
        Servers.Filter = GetHiddenItemsFilter(query);
        SecureCoreCountries.Filter = GetHiddenItemsFilter(query);

        InvalidateCountriesNumber();
        NotifyPropertyChanges();
    }

    public override void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        InvalidateUpsellBannerVisibility();

        if (!string.IsNullOrEmpty(LastSearchQuery))
        {
            FilterItems(LastSearchQuery);
        }
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateUpsellBannerVisibility);
    }

    protected abstract IEnumerable<string> GetCountryCodes();

    protected abstract IEnumerable<City> GetCities();

    protected abstract IEnumerable<Server> GetServers();

    protected abstract IEnumerable<SecureCoreCountryPair> GetSecureCoreCountries();

    protected abstract int GetCountryItemsCount(string countryCode);

    protected bool MatchesSearchQuery(object o, string query)
    {
        return o is ISearchableItem item && item.MatchesSearchQuery(query);
    }

    [RelayCommand]
    private void UpgradePlan()
    {
        _upsellCarouselDialogActivator.ShowDialog(UpsellModalSources);
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
        OnPropertyChanged(nameof(HasSecureCoreCountries));
    }

    private List<CountryViewModel> GetCountryViewModels()
    {
        IEnumerable<CountryViewModel> countries = GetCountryCodes()
            .Select(GetCountryViewModel);

        if (Settings.IsPaid)
        {
            countries = countries.Prepend(_countryViewModelsFactory.GetFastestCountryViewModel(CountryFeature));
        }

        return countries.ToList();
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

    private List<CountryViewModel> GetSecureCoreCountryViewModels()
    {
        return GetSecureCoreCountries()
            .Select(_countryViewModelsFactory.GetSecureCoreCountryViewModel)
            .ToList();
    }

    partial void OnCountriesChanged(AdvancedCollectionView? oldValue, AdvancedCollectionView newValue)
    {
        InvalidateCountriesNumber();
    }

    private void InvalidateCountriesNumber()
    {
        TotalCountries = Countries.OfType<CountryViewModel>().Count(c => !c.IsFastest);
    }

    private void InvalidateUpsellBannerVisibility()
    {
        OnPropertyChanged(nameof(IsUpsellBannerVisible));
    }
}