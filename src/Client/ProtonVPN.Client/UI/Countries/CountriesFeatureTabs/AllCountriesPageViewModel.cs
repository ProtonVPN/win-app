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

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Countries.Controls;

namespace ProtonVPN.Client.UI.Countries.CountriesFeatureTabs;

public class AllCountriesPageViewModel : CountriesTabViewModelBase
{
    public override IconElement? Icon => null;

    public override string Title => Localizer.Get("Countries_All");

    protected override CountryFeature CountryFeature => CountryFeature.None;

    public AllCountriesPageViewModel(
        IMainViewNavigator mainViewNavigator,
        ICountriesFeatureTabsViewNavigator countriesFeatureTabsViewNavigator,
        IServerManager serverManager,
        ILocalizationProvider localizationProvider,
        NoSearchResultsViewModel noSearchResultsViewModel,
        CountryViewModelsFactory countryViewModelsFactory) : base(mainViewNavigator, serverManager,
        countriesFeatureTabsViewNavigator, localizationProvider, noSearchResultsViewModel, countryViewModelsFactory)
    {
    }

    protected override List<string> GetCountryCodes()
    {
        return ServerManager.GetCountryCodes();
    }

    protected override List<City> GetCities()
    {
        return ServerManager.GetCities();
    }

    protected override List<Server> GetServers()
    {
        return ServerManager.GetServers().Where(s => !s.Features.IsSupported(ServerFeatures.SecureCore)).ToList();
    }

    protected override int GetCountryItemsCount(string countryCode)
    {
        return ServerManager.GetCitiesByCountry(countryCode).Count;
    }
}