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
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Gateways.Items;

namespace ProtonVPN.Client.UI.Countries;

public class CountryViewModelsFactory
{
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly IOverlayActivator _overlayActivator;
    private readonly ILocalizationProvider _localizer;
    private readonly IConnectionManager _connectionManager;

    public CountryViewModelsFactory(
        ILocalizationProvider localizer,
        IMainViewNavigator mainViewNavigator,
        IOverlayActivator overlayActivator,
        IConnectionManager connectionManager)
    {
        _mainViewNavigator = mainViewNavigator;
        _overlayActivator = overlayActivator;
        _localizer = localizer;
        _connectionManager = connectionManager;
    }

    public CountryViewModel GetCountryViewModel(string exitCountryCode, CountryFeature countryFeature, int itemCount)
    {
        return new CountryViewModel(_localizer, _mainViewNavigator, _connectionManager)
        {
            EntryCountryCode = string.Empty,
            ExitCountryCode = exitCountryCode,
            ExitCountryName = _localizer.GetCountryName(exitCountryCode),
            IsUnderMaintenance = false,
            SecondaryActionLabel = _localizer.GetPluralFormat(GetCountrySecondaryActionLabel(countryFeature), itemCount),
            CountryFeature = countryFeature
        };
    }

    public CountryViewModel GetFastestCountryViewModel(CountryFeature countryFeature)
    {
        string fastestCountryLabel = _localizer.Get("Countries_Fastest");

        return new CountryViewModel(_localizer, _mainViewNavigator, _connectionManager)
        {
            EntryCountryCode = string.Empty,
            ExitCountryCode = string.Empty,
            ExitCountryName = fastestCountryLabel,
            CountryFeature = countryFeature,
            SecondaryActionLabel = string.Empty,
        };
    }

    public CityViewModel GetCityViewModel(City city, List<ServerViewModel> servers, CountryFeature countryFeature)
    {
        return new(_localizer, _mainViewNavigator, _overlayActivator, _connectionManager)
        {
            City = city,
            Servers = servers,
            CountryFeature = countryFeature
        };
    }

    public ServerViewModel GetServerViewModel(Server server)
    {
        ServerViewModel serverViewModel = new(_localizer, _mainViewNavigator, _connectionManager);

        serverViewModel.CopyPropertiesFromServer(server);

        return serverViewModel;
    }

    private string GetCountrySecondaryActionLabel(CountryFeature countryFeature)
    {
        return countryFeature switch
        {
            CountryFeature.None or CountryFeature.P2P => "Countries_City",
            _ => "Countries_Server",
        };
    }
}