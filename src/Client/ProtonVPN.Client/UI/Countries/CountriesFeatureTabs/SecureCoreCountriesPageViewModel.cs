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

namespace ProtonVPN.Client.UI.Countries.CountriesFeatureTabs;

public partial class SecureCoreCountriesPageViewModel : CountriesTabViewModelBase
{
    public override IconElement Icon => new LockLayers();

    public override string Title => Localizer.Get("Countries_SecureCore");

    protected override CountryFeature CountryFeature => CountryFeature.SecureCore;

    public SecureCoreCountriesPageViewModel(
        IMainViewNavigator mainViewNavigator,
        IOverlayActivator overlayActivator,
        ICountriesFeatureTabsViewNavigator countriesFeatureTabsViewNavigator,
        ILocalizationProvider localizationProvider,
        IServersLoader serversLoader,
        NoSearchResultsViewModel noSearchResultsViewModel,
        CountryViewModelsFactory countryViewModelsFactory) : base(mainViewNavigator, overlayActivator, serversLoader,
        countriesFeatureTabsViewNavigator, localizationProvider, noSearchResultsViewModel, countryViewModelsFactory)
    {
    }

    [RelayCommand]
    public void ShowInfoOverlay()
    {
        OverlayActivator.ShowOverlayAsync<SecureCoreOverlayViewModel>();
    }

    protected override IEnumerable<string> GetCountryCodes()
    {
        return ServersLoader.GetCountryCodesByFeatures(ServerFeatures.SecureCore);
    }

    protected override IEnumerable<City> GetCities()
    {
        return new List<City>();
    }

    protected override IEnumerable<Server> GetServers()
    {
        return ServersLoader.GetServersByFeatures(ServerFeatures.SecureCore);
    }

    protected override int GetCountryItemsCount(string countryCode)
    {
        return ServersLoader.GetServersByFeaturesAndCountryCode(ServerFeatures.SecureCore, countryCode).Count();
    }
}