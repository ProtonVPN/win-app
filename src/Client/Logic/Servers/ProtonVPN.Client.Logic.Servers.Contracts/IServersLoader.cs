/*
 * Copyright (c) 2025 Proton AG
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

using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Servers.Contracts;

public interface IServersLoader
{
    int GetServerCount();
    int GetCountryCount();

    Server? GetById(string serverId);

    IEnumerable<FreeCountry> GetFreeCountries();

    IEnumerable<Country> GetCountries();
    IEnumerable<Country> GetCountriesByFeatures(ServerFeatures serverFeatures);
    bool HasAnyCountries();

    IEnumerable<State> GetStates();
    IEnumerable<State> GetStatesByCountryCode(string countryCode);
    IEnumerable<State> GetStatesByFeatures(ServerFeatures serverFeatures);
    IEnumerable<State> GetStatesByFeaturesAndCountryCode(ServerFeatures serverFeatures, string countryCode);

    IEnumerable<City> GetCities();
    IEnumerable<City> GetCitiesByCountryCode(string countryCode);
    IEnumerable<City> GetCitiesByState(State state);
    IEnumerable<City> GetCitiesByFeatures(ServerFeatures serverFeatures);
    IEnumerable<City> GetCitiesByFeaturesAndCountryCode(ServerFeatures serverFeatures, string countryCode);
    IEnumerable<City> GetCitiesByFeaturesAndState(ServerFeatures serverFeatures, State state);

    IEnumerable<Server> GetServers();
    IEnumerable<Server> GetFreeServers();
    IEnumerable<Server> GetFreeServersByFeatures(ServerFeatures serverFeatures);
    IEnumerable<Server> GetServersByState(State state);
    IEnumerable<Server> GetServersByCity(City city);
    IEnumerable<Server> GetServersByFeatures(ServerFeatures serverFeatures);
    IEnumerable<Server> GetServersByFeaturesAndCountryCode(ServerFeatures serverFeatures, string countryCode);
    IEnumerable<Server> GetServersByFeaturesAndState(ServerFeatures serverFeatures, State state);
    IEnumerable<Server> GetServersByFeaturesAndCity(ServerFeatures serverFeatures, City city);

    IEnumerable<SecureCoreCountryPair> GetSecureCoreCountryPairs();
    IEnumerable<SecureCoreCountryPair> GetSecureCoreCountryPairsByExitCountryCode(string exitCountryCode);
    IEnumerable<Server> GetServersBySecureCoreCountryPair(SecureCoreCountryPair countryPair);

    IEnumerable<Gateway> GetGateways();
    IEnumerable<Server> GetServersByGatewayName(string gatewayName);
    bool HasAnyGateways();
}