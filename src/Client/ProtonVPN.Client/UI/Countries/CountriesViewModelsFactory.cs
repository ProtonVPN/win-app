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
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Navigation;
using WinUI3Localizer;

namespace ProtonVPN.Client.UI.Countries;

public class CountriesViewModelsFactory
{
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly ILocalizationProvider _localizer;
    
    public CountriesViewModelsFactory(ILocalizationProvider localizer, IMainViewNavigator mainViewNavigator)
    {
        _mainViewNavigator = mainViewNavigator;
        _localizer = localizer;
    }

    public CountryViewModel GetCountryViewModel(string exitCountryCode, CountryFeature countryFeature, int itemCount)
    {
        return new CountryViewModel (_localizer, _mainViewNavigator)
        {
            ExitCountryCode = exitCountryCode,
            ExitCountryName = _localizer.GetCountryName(exitCountryCode),
            IsUnderMaintenance = false,
            SecondaryActionLabel = _localizer.GetPluralFormat(GetCountrySecondaryActionLabel(countryFeature), itemCount),
            CountryFeature = countryFeature,
            IsSecureCore = countryFeature == CountryFeature.SecureCore,
        };
    }

    public CountryViewModel GetFastestCountryViewModel(CountryFeature countryFeature)
    {
        string fastestCountryLabel = _localizer.Get("Countries_Fastest");

        return new CountryViewModel(_localizer, _mainViewNavigator)
        {
            ExitCountryName = fastestCountryLabel,
            CountryFeature = countryFeature,
            IsSecureCore = countryFeature == CountryFeature.SecureCore,
        };
    }

    private string GetCountrySecondaryActionLabel(CountryFeature countryFeature)
    {
        return countryFeature == CountryFeature.Cities ? "Countries_City" : "Countries_Server";
    }

    public CityViewModel GetCityViewModel(string city, List<ServerViewModel> servers)
    {
        return new(_localizer, _mainViewNavigator)
        {
            Name = city,
            Servers = servers,
        };
    }

    public ServerViewModel GetServerViewModel(Server server)
    {
        if (server.IsSecureCore)
        {
            return new SecureCoreServerViewModel(_localizer, _mainViewNavigator)
            {
                Name = server.Name,
                Load = server.Load / 100d,
                LoadPercent = $"{(double)server.Load / 100:P0}",
                IsVirtual = server.IsVirtual,
                SupportsP2P = server.SupportsP2P,
                IsUnderMaintenance = server.IsUnderMaintenance,
                EntryCountryCode = server.EntryCountry,
                EntryCountryName = _localizer.GetFormat("Countries_ViaCountry", _localizer.GetCountryName(server.EntryCountry)),
                ExitCountryCode = server.ExitCountry,
                ExitCountryName = _localizer.GetCountryName(server.ExitCountry),
            };
        }

        return new ServerViewModel(_localizer, _mainViewNavigator)
        {
            Name = server.Name,
            Load = server.Load / 100d,
            LoadPercent = $"{(double)server.Load / 100:P0}",
            IsVirtual = server.IsVirtual,
            SupportsP2P = server.SupportsP2P,
            IsUnderMaintenance = server.IsUnderMaintenance,
        };
    }
}