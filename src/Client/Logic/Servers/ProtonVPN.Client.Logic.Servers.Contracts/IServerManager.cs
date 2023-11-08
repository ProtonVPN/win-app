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

public interface IServerManager
{
    Task FetchServersAsync();

    List<string> GetCountryCodes();

    List<string> GetCitiesByCountry(string countryCode);

    List<string> GetP2PCitiesByCountry(string countryCode);

    List<Server> GetServersByCity(string city);

    List<string> GetSecureCoreCountryCodes();

    List<string> GetP2PCountryCodes();

    List<string> GetTorCountryCodes();

    List<Server> GetSecureCoreServersByExitCountry(string countryCode);

    List<Server> GetP2PServersByExitCountry(string countryCode);

    List<Server> GetTorServersByExitCountry(string countryCode);

    List<Server> GetP2PServersByCity(string city);

    string? GetHostCountryCode(string countryCode);
}