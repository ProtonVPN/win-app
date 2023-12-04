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

namespace ProtonVPN.Client.Logic.Servers.Contracts;

public interface IServersLoader
{
    IEnumerable<string> GetCountryCodes();
    IEnumerable<string> GetCountryCodesByFeatures(ServerFeatures serverFeatures);

    IEnumerable<City> GetCities();
    IEnumerable<City> GetCitiesByCountryCode(string countryCode);
    IEnumerable<City> GetCitiesByFeatures(ServerFeatures serverFeatures);
    IEnumerable<City> GetCitiesByFeaturesAndCountryCode(ServerFeatures serverFeatures, string countryCode);

    IEnumerable<Server> GetServers();
    IEnumerable<Server> GetServersByCity(City city);
    IEnumerable<Server> GetServersByFeatures(ServerFeatures serverFeatures);
    IEnumerable<Server> GetServersByFeaturesAndExitCountry(ServerFeatures serverFeatures, string countryCode);
    IEnumerable<Server> GetServersByFeaturesAndCity(ServerFeatures serverFeatures, City city);

    string? GetHostCountryCode(string countryCode);
}