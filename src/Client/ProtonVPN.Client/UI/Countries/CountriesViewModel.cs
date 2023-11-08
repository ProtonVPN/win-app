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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Countries.CountriesFeatureTabs;

namespace ProtonVPN.Client.UI.Countries;

public partial class CountriesViewModel : NavigationPageViewModelBase
{
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private NavigationPageViewModelBase? _selectedNavigationPage;

    [ObservableProperty]
    private PageViewModelBase? _selectedFeatureTab;

    public override string Title => Localizer.Get("Countries_Page_Title");

    public override IconElement Icon { get; } = new Earth();

    public override bool IsBackEnabled => false;

    public ICountriesFeatureTabsViewNavigator CountriesFeatureTabsViewNavigator { get; }

    [ObservableProperty]
    private ObservableCollection<CountriesTabViewModelBase> _featureTabPages;

    public CountriesViewModel(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ICountriesFeatureTabsViewNavigator countriesFeatureTabsViewNavigator,
        AllCountriesPageViewModel allCountriesPageViewModel,
        SecureCoreCountriesPageViewModel secureCoreCountriesPageViewModel,
        P2PCountriesPageViewModel p2PCountriesPageViewModel,
        TorCountriesPageViewModel torCountriesPageViewModel)
        : base(viewNavigator, localizationProvider)
    {
        CountriesFeatureTabsViewNavigator = countriesFeatureTabsViewNavigator;

        _featureTabPages = new()
        {
            allCountriesPageViewModel,
            secureCoreCountriesPageViewModel,
            p2PCountriesPageViewModel,
            torCountriesPageViewModel,
        };
    }

    public override void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        foreach (CountriesTabViewModelBase page in FeatureTabPages)
        {
            page.LoadItems();
        }

        CountriesFeatureTabsViewNavigator.NavigateToAsync<AllCountriesPageViewModel>();
        SelectedFeatureTab = FeatureTabPages.First();
    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();

        SearchQuery = string.Empty;
    }

    partial void OnSearchQueryChanged(string value)
    {
        foreach (CountriesTabViewModelBase tab in FeatureTabPages)
        {
            tab.FilterItems(value);
        }
    }
}