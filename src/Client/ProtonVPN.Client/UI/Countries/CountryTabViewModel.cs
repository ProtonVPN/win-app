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
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Countries.CountryFeatureTabs;
using ProtonVPN.Client.UI.Dialogs.Overlays;

namespace ProtonVPN.Client.UI.Countries;

public partial class CountryTabViewModel : PageViewModelBase<IMainViewNavigator>
{
    private readonly IServersLoader _serversLoader;
    private readonly IOverlayActivator _overlayActivator;
    private readonly CitiesPageViewModel _citiesPageViewModel;
    private readonly P2PCitiesPageViewModel _p2pCitiesPageViewModel;
    private readonly SecureCoreCountryPageViewModel _secureCoreCountryPageViewModel;
    private readonly TorServersPageViewModel _torServersPageViewModel;

    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(Title))]
    private string _currentCountryCode;

    [ObservableProperty]
    private string _smartRoutingLabel;

    [ObservableProperty]
    private bool _isVirtual;

    [ObservableProperty]
    private NavigationPageViewModelBase? _selectedNavigationPage;

    [ObservableProperty]
    private PageViewModelBase? _selectedFeatureTab;

    public ObservableCollection<CountryTabViewModelBase> FeatureTabPages { get; }

    public CountryTabViewModelBase? CurrentTab => CountryFeatureTabsViewNavigator?.Frame?.GetPageViewModel() as CountryTabViewModelBase;

    public override string Title => Localizer.GetCountryName(CurrentCountryCode);

    public ICountryFeatureTabsViewNavigator CountryFeatureTabsViewNavigator { get; }

    public CountryTabViewModel(
        IServersLoader serversLoader,
        IMainViewNavigator mainViewNavigator,
        ILocalizationProvider localizationProvider,
        IOverlayActivator overlayActivator,
        ICountryFeatureTabsViewNavigator countryFeatureTabsViewNavigator,
        CitiesPageViewModel citiesPageViewModel,
        P2PCitiesPageViewModel p2pCitiesPageViewModel,
        SecureCoreCountryPageViewModel secureCoreCountryPageViewModel,
        TorServersPageViewModel torServersPageViewModel)
        : base(mainViewNavigator, localizationProvider)
    {
        _serversLoader = serversLoader;
        _overlayActivator = overlayActivator;
        _citiesPageViewModel = citiesPageViewModel;
        _p2pCitiesPageViewModel = p2pCitiesPageViewModel;
        _secureCoreCountryPageViewModel = secureCoreCountryPageViewModel;
        _torServersPageViewModel = torServersPageViewModel;

        CountryFeatureTabsViewNavigator = countryFeatureTabsViewNavigator;
        CountryFeatureTabsViewNavigator.Navigated += OnNavigated;

        FeatureTabPages = new()
        {
            _citiesPageViewModel,
            _secureCoreCountryPageViewModel,
            _p2pCitiesPageViewModel,
            _torServersPageViewModel,
        };
    }

    public override void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        if (parameter is not CountryViewModel country)
        {
            return;
        }

        CurrentCountryCode = country.ExitCountryCode;
        UpdateSmartRouting();

        foreach (CountryTabViewModelBase countryTabViewModelBase in FeatureTabPages)
        {
            countryTabViewModelBase.LoadItems(CurrentCountryCode);
        }

        switch (country.CountryFeature)
        {
            case CountryFeature.None:
                CountryFeatureTabsViewNavigator.NavigateToAsync<CitiesPageViewModel>(CurrentCountryCode);
                SelectedFeatureTab = _citiesPageViewModel;
                break;
            case CountryFeature.SecureCore:
                CountryFeatureTabsViewNavigator.NavigateToAsync<SecureCoreCountryPageViewModel>(CurrentCountryCode);
                SelectedFeatureTab = _secureCoreCountryPageViewModel;
                break;
            case CountryFeature.P2P:
                CountryFeatureTabsViewNavigator.NavigateToAsync<P2PCitiesPageViewModel>(CurrentCountryCode);
                SelectedFeatureTab = _p2pCitiesPageViewModel;
                break;
            case CountryFeature.Tor:
                CountryFeatureTabsViewNavigator.NavigateToAsync<TorServersPageViewModel>(CurrentCountryCode);
                SelectedFeatureTab = _torServersPageViewModel;
                break;
        }
    }

    public void UpdateSmartRouting()
    {
        string? hostCountryCode = _serversLoader.GetHostCountryCode(CurrentCountryCode);
        if (string.IsNullOrEmpty(hostCountryCode))
        {
            IsVirtual = false;
            SmartRoutingLabel = string.Empty;
        }
        else
        {
            IsVirtual = true;
            SmartRoutingLabel = Localizer.GetFormat("Countries_SmartRouting_Description", Localizer.GetCountryName(hostCountryCode), Title);
        }
    }

    [RelayCommand]
    public async Task ShowSmartRoutingOverlayCommandAsync()
    {
        await _overlayActivator.ShowOverlayAsync<SmartRoutingOverlayViewModel>();
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        OnPropertyChanged(nameof(IsBackEnabled));
        OnPropertyChanged(nameof(CurrentTab));

        SelectedFeatureTab = CurrentTab ?? FeatureTabPages.FirstOrDefault();
    }
}