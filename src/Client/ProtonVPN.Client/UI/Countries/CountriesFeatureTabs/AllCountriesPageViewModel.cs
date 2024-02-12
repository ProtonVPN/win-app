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
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Activation;
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
        IOverlayActivator overlayActivator,
        ICountriesFeatureTabsViewNavigator countriesFeatureTabsViewNavigator,
        IServersLoader serversLoader,
        ILocalizationProvider localizationProvider,
        NoSearchResultsViewModel noSearchResultsViewModel,
        CountryViewModelsFactory countryViewModelsFactory) : base(mainViewNavigator, overlayActivator, serversLoader,
        countriesFeatureTabsViewNavigator, localizationProvider, noSearchResultsViewModel, countryViewModelsFactory)
    {
    }

    protected override IEnumerable<string> GetCountryCodes()
    {
        return ServersLoader.GetCountryCodes();
    }

    protected override IEnumerable<City> GetCities()
    {
        return ServersLoader.GetCities();
    }

    protected override IEnumerable<Server> GetServers()
    {
        return ServersLoader.GetServers()
            .Where(s => !s.Features.IsSupported(ServerFeatures.SecureCore)
                     && !s.Features.IsSupported(ServerFeatures.B2B));
    }

    protected override IEnumerable<SecureCoreCountryPair> GetSecureCoreCountries()
    {
        return new List<SecureCoreCountryPair>();
    }

    protected override int GetCountryItemsCount(string countryCode)
    {
        return ServersLoader.GetCitiesByCountryCode(countryCode).Count();
    }
}