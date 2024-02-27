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
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Countries.Controls;
using ProtonVPN.Client.UI.Dialogs.Overlays;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Countries.CountriesFeatureTabs;

public partial class TorCountriesPageViewModel : CountriesTabViewModelBase
{
    public override IconElement Icon => new BrandTor();

    public override string Title => Localizer.Get("Countries_Tor");

    protected override CountryFeature CountryFeature => CountryFeature.Tor;

    public TorCountriesPageViewModel(
        IMainViewNavigator mainViewNavigator,
        IOverlayActivator overlayActivator,
        ICountriesFeatureTabsViewNavigator countriesFeatureTabsViewNavigator,
        ILocalizationProvider localizationProvider,
        IServersLoader serversLoader,
        ILogger logger,
        IIssueReporter issueReporter,
        NoSearchResultsViewModel noSearchResultsViewModel,
        CountryViewModelsFactory countryViewModelsFactory)
        : base(mainViewNavigator, overlayActivator, serversLoader, countriesFeatureTabsViewNavigator,
            localizationProvider, logger, issueReporter, noSearchResultsViewModel, countryViewModelsFactory)
    {
    }

    [RelayCommand]
    public async Task ShowInfoOverlayAsync()
    {
        await OverlayActivator.ShowOverlayAsync<TorOverlayViewModel>();
    }

    protected override IEnumerable<string> GetCountryCodes()
    {
        return ServersLoader.GetCountryCodesByFeatures(ServerFeatures.Tor);
    }

    protected override IEnumerable<City> GetCities()
    {
        return new List<City>();
    }

    protected override IEnumerable<Server> GetServers()
    {
        return ServersLoader.GetServersByFeatures(ServerFeatures.Tor);
    }

    protected override IEnumerable<SecureCoreCountryPair> GetSecureCoreCountries()
    {
        return new List<SecureCoreCountryPair>();
    }

    protected override int GetCountryItemsCount(string countryCode)
    {
        return ServersLoader.GetServersByFeaturesAndCountryCode(ServerFeatures.Tor, countryCode).Count();
    }
}