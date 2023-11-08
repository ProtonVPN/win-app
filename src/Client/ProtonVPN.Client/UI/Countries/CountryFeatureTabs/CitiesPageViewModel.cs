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

using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Navigation;

namespace ProtonVPN.Client.UI.Countries.CountryFeatureTabs;

public class CitiesPageViewModel : CitiesPageViewModelBase
{
    public override string Title => Localizer.GetFormat("Countries_Cities");

    public CitiesPageViewModel(
        IServerManager serverManager,
        ICountryFeatureTabsViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator) : base(connectionManager, mainViewNavigator, serverManager, viewNavigator,
        localizationProvider)
    {
    }

    protected override List<string> GetCities()
    {
        return ServerManager.GetCitiesByCountry(CurrentCountryCode);
    }

    protected override List<Server> GetServers(string city)
    {
        return ServerManager.GetServersByCity(city);
    }
}