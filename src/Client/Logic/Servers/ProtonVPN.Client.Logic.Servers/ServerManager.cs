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

using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Servers;

public class ServerManager : IServerManager
{
    private readonly IApiClient _apiClient;
    private readonly IEntityMapper _entityMapper;

    private List<string> _countries = new();
    private List<LogicalServerResponse> _servers = new();

    public ServerManager(IApiClient apiClient, IEntityMapper entityMapper)
    {
        _apiClient = apiClient;
        _entityMapper = entityMapper;
    }

    public async Task FetchServersAsync()
    {
        try
        {
            ApiResponseResult<ServersResponse> response = await _apiClient.GetServersAsync(string.Empty);
            if (response.Success)
            {
                _servers = response.Value.Servers;
                SaveCountries(response.Value.Servers);
            }
        }
        catch
        {
        }
    }

    public List<string> GetCountryCodes()
    {
        return _countries;
    }

    public List<City> GetCities()
    {
        return GetCitiesByFilter(server => !string.IsNullOrWhiteSpace(server.City));
    }

    public List<City> GetCitiesByCountry(string countryCode)
    {
        return GetCitiesByFilter(server => server.ExitCountry == countryCode &&
                                        !string.IsNullOrWhiteSpace(server.City));
    }

    public List<City> GetP2PCities()
    {
        return GetCitiesByFilter(server => server.Features.IsSupported(ServerFeatures.P2P) &&
                                           !string.IsNullOrWhiteSpace(server.City));
    }

    public List<City> GetP2PCitiesByCountry(string countryCode)
    {
        return GetCitiesByFilter(server => server.ExitCountry == countryCode &&
                                        server.Features.IsSupported(ServerFeatures.P2P) &&
                                        !string.IsNullOrWhiteSpace(server.City));
    }

    private List<City> GetCitiesByFilter(Func<LogicalServerResponse, bool> filterFunc)
    {
        return _servers
            .Where(filterFunc)
            .Select(s => new City
            {
                Name = s.City,
                CountryCode = s.ExitCountry
            })
            .DistinctBy(c => c.Name + c.CountryCode)
            .ToList();
    }

    public List<Server> GetServersByCity(City city)
    {
        return (from server in _servers
            where server.City == city.Name && server.ExitCountry == city.CountryCode
            select _entityMapper.Map<LogicalServerResponse, Server>(server)).ToList();
    }

    public List<string> GetSecureCoreCountryCodes()
    {
        return GetCountryCodesByFilter(server => server.Features.IsSupported(ServerFeatures.SecureCore));
    }

    public List<string> GetP2PCountryCodes()
    {
        return GetCountryCodesByFilter(server => server.Features.IsSupported(ServerFeatures.P2P));
    }

    public List<string> GetTorCountryCodes()
    {
        return GetCountryCodesByFilter(server => server.Features.IsSupported(ServerFeatures.Tor));
    }

    private List<string> GetCountryCodesByFilter(Func<LogicalServerResponse, bool> filterFunc)
    {
        return _servers
            .Where(server => !string.IsNullOrWhiteSpace(server.ExitCountry))
            .Where(filterFunc)
            .Select(s => s.ExitCountry)
            .Distinct()
            .ToList();
    }

    public List<Server> GetSecureCoreServers()
    {
        return GetServers(server => server.Features.IsSupported(ServerFeatures.SecureCore));
    }

    public List<Server> GetSecureCoreServersByExitCountry(string countryCode)
    {
        return GetServers(server => server.Features.IsSupported(ServerFeatures.SecureCore) && server.ExitCountry == countryCode);
    }

    public List<Server> GetP2PServersByExitCountry(string countryCode)
    {
        return GetServers(server => server.Features.IsSupported(ServerFeatures.P2P) && server.ExitCountry == countryCode);
    }

    public List<Server> GetTorServers()
    {
        return GetServers(server => server.Features.IsSupported(ServerFeatures.Tor));
    }

    public List<Server> GetTorServersByExitCountry(string countryCode)
    {
        return GetServers(server => server.Features.IsSupported(ServerFeatures.Tor) && server.ExitCountry == countryCode);
    }

    public List<Server> GetP2PServersByCity(City city)
    {
        return GetServers(server => server.Features.IsSupported(ServerFeatures.P2P) &&
                                    server.City == city.Name &&
                                    server.ExitCountry == city.CountryCode);
    }

    public List<Server> GetP2PServers()
    {
        return GetServers(server => server.Features.IsSupported(ServerFeatures.P2P));
    }

    public List<Server> GetServers(Func<LogicalServerResponse, bool>? filterFunc = null)
    {
        return _servers
            .Where(filterFunc ?? (_ => true))
            .Select(_entityMapper.Map<LogicalServerResponse, Server>)
            .ToList();
    }

    public string? GetHostCountryCode(string countryCode)
    {
         return _servers.FirstOrDefault(server => server.ExitCountry == countryCode && !string.IsNullOrEmpty(server.HostCountry))?.HostCountry;
    }

    private void SaveCountries(IEnumerable<LogicalServerResponse> servers)
    {
        List<string> countryCodes = new();

        foreach (LogicalServerResponse server in servers)
        {
            if (!IsCountry(server) || countryCodes.Contains(server.EntryCountry))
            {
                continue;
            }


            countryCodes.Add(server.EntryCountry);
        }

        _countries = countryCodes;
    }

    private static bool IsCountry(LogicalServerResponse server)
    {
        string code = server.EntryCountry;
        if (code.Equals("AA") || code.Equals("ZZ") || code.StartsWith("X"))
        {
            return false;
        }

        string[] letters = { "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        if (code.StartsWith("Q") && letters.Contains(code.Substring(1, 1)))
        {
            return false;
        }

        return true;
    }
}