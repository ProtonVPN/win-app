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

using ProtonVPN.Api.Contracts.Servers;

namespace ProtonVPN.Client.Logic.Servers.Contracts;

public interface IServerManager
{
    Task FetchServersAsync();
    List<string> GetCountryCodes();
    List<City> GetCities();
    List<City> GetCitiesByCountry(string countryCode);
    List<City> GetP2PCities();
    List<City> GetP2PCitiesByCountry(string countryCode);
    List<Server> GetServersByCity(City city);
    List<Server> GetServers(Func<LogicalServerResponse, bool>? filterFunc = null);
    List<string> GetSecureCoreCountryCodes();
    List<string> GetP2PCountryCodes();
    List<string> GetTorCountryCodes();
    List<Server> GetSecureCoreServers();
    List<Server> GetSecureCoreServersByExitCountry(string countryCode);
    List<Server> GetP2PServersByExitCountry(string countryCode);
    List<Server> GetTorServers();
    List<Server> GetTorServersByExitCountry(string countryCode);
    List<Server> GetP2PServersByCity(City city);
    List<Server> GetP2PServers();
    string? GetHostCountryCode(string countryCode);
}