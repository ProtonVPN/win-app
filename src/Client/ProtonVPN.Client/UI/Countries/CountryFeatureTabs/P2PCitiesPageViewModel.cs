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
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Dialogs.Overlays;

namespace ProtonVPN.Client.UI.Countries.CountryFeatureTabs;

public partial class P2PCitiesPageViewModel : CitiesPageViewModelBase
{
    public override IconElement Icon => new ArrowRightArrowLeft();

    public override string Title => Localizer.GetFormat("Countries_P2P");

    protected override CountryFeature CountryFeature => CountryFeature.P2P;

    public P2PCitiesPageViewModel(
        IServersLoader serversLoader,
        ICountryFeatureTabsViewNavigator viewNavigator,
        CountryViewModelsFactory countryViewModelsFactory,
        ILocalizationProvider localizationProvider,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IOverlayActivator overlayActivator) :
        base(connectionManager, mainViewNavigator, overlayActivator, serversLoader, viewNavigator, countryViewModelsFactory,
            localizationProvider)
    {
    }

    [RelayCommand]
    public async Task ShowInfoOverlayAsync()
    {
        await OverlayActivator.ShowOverlayAsync<P2POverlayViewModel>();
    }

    protected override IEnumerable<City> GetCities()
    {
        return ServersLoader.GetCitiesByFeaturesAndCountryCode(ServerFeatures.P2P, CurrentCountryCode);
    }

    protected override IEnumerable<Server> GetServers(City city)
    {
        return ServersLoader.GetServersByFeaturesAndCity(ServerFeatures.P2P, city);
    }
}