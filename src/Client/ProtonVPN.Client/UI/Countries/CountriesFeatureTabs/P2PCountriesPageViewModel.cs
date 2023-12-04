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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Countries.Controls;
using ProtonVPN.Client.UI.Dialogs.Overlays;

namespace ProtonVPN.Client.UI.Countries.CountriesFeatureTabs;

public partial class P2PCountriesPageViewModel : CountriesTabViewModelBase
{
    public override IconElement Icon => new ArrowRightArrowLeft();

    public override string Title => Localizer.Get("Countries_P2P");

    protected override CountryFeature CountryFeature => CountryFeature.P2P;

    public P2PCountriesPageViewModel(
        IMainViewNavigator mainViewNavigator,
        ICountriesFeatureTabsViewNavigator countriesFeatureTabsViewNavigator,
        ILocalizationProvider localizationProvider,
        IServerManager serverManager,
        NoSearchResultsViewModel noSearchResultsViewModel,
        CountryViewModelsFactory countryViewModelsFactory) : base(
        mainViewNavigator, serverManager, countriesFeatureTabsViewNavigator, localizationProvider,
        noSearchResultsViewModel, countryViewModelsFactory)
    {
    }

    [RelayCommand]
    public void ShowInfoOverlay()
    {
        MainViewNavigator.ShowOverlayAsync<P2POverlayViewModel>();
    }

    protected override List<string> GetCountryCodes()
    {
        return ServerManager.GetP2PCountryCodes();
    }

    protected override List<City> GetCities()
    {
        return ServerManager.GetP2PCities();
    }

    protected override List<Server> GetServers()
    {
        return ServerManager.GetP2PServers();
    }

    protected override int GetCountryItemsCount(string countryCode)
    {
        return ServerManager.GetP2PServersByExitCountry(countryCode).Count;
    }
}