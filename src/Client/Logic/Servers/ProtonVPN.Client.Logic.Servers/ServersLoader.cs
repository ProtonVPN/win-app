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

using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Servers;

public class ServersLoader : IServersLoader
{
    private readonly IServersCache _serversCache;

    public ServersLoader(IServersCache serversCache)
    {
        _serversCache = serversCache;
    }

    public Server? GetById(string serverId)
    {
        return GetServers().FirstOrDefault(s => s.Id == serverId);
    }

    public IEnumerable<string> GetCountryCodes()
    {
        return _serversCache.CountryCodes;
    }

    public IEnumerable<string> GetCountryCodesByFeatures(ServerFeatures serverFeatures)
    {
        return GetServersByFilter(s => !string.IsNullOrWhiteSpace(s.ExitCountry)
                                    && s.Features.IsSupported(serverFeatures))
            .Select(s => s.ExitCountry)
            .Distinct();
    }

    public IEnumerable<string> GetFreeCountryCodes()
    {
        return GetServersByFilter(s => !string.IsNullOrWhiteSpace(s.ExitCountry)
                                    && (ServerTiers)s.Tier == ServerTiers.Free)
            .Select(s => s.ExitCountry)
            .Distinct();
    }

    public IEnumerable<City> GetCities()
    {
        return GetCitiesByFilter();
    }

    private IEnumerable<City> GetCitiesByFilter(Func<Server, bool>? filterFunc = null)
    {
        return GetServersByFilter(s => !string.IsNullOrWhiteSpace(s.City)
                                    && (filterFunc is null || filterFunc(s)))
            .GroupBy(s => new { Name = s.City, CountryCode = s.ExitCountry })
            .Select(g => new City { Name = g.Key.Name, CountryCode = g.Key.CountryCode });
    }

    public IEnumerable<City> GetCitiesByCountryCode(string countryCode)
    {
        return GetCitiesByFilter(s => s.ExitCountry == countryCode);
    }

    public IEnumerable<City> GetCitiesByFeatures(ServerFeatures serverFeatures)
    {
        return GetCitiesByFilter(s => s.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<City> GetCitiesByFeaturesAndCountryCode(ServerFeatures serverFeatures, string countryCode)
    {
        return GetCitiesByFilter(s => s.ExitCountry == countryCode
                                   && s.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<Server> GetServers()
    {
        return _serversCache.Servers;
    }

    private IEnumerable<Server> GetServersByFilter(Func<Server, bool> filterFunc, bool excludeB2BServers = true)
    {
        return GetServers()
            .Where(s => (filterFunc is null || filterFunc(s))
                     && !(excludeB2BServers && s.Features.IsSupported(ServerFeatures.B2B)));
    }

    public IEnumerable<Server> GetServersByCity(City city)
    {
        return GetServersByFilter(s => s.ExitCountry == city.CountryCode
                                    && s.City == city.Name);
    }

    public IEnumerable<Server> GetServersByFeatures(ServerFeatures serverFeatures)
    {
        return GetServersByFilter(s => s.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<Server> GetServersByFeaturesAndCountryCode(ServerFeatures serverFeatures, string countryCode)
    {
        return GetServersByFilter(s => s.ExitCountry == countryCode
                                    && s.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<Server> GetServersByFeaturesAndCity(ServerFeatures serverFeatures, City city)
    {
        return GetServersByFilter(s => s.ExitCountry == city.CountryCode
                                    && s.City == city.Name
                                    && s.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<string> GetGateways()
    {
        return _serversCache.Gateways;
    }

    public IEnumerable<Server> GetServersByGateway(string gatewayName)
    {
        return GetServersByFilter(s => s.GatewayName == gatewayName, false);
    }

    public string? GetHostCountryCode(string countryCode)
    {
        return GetServersByFilter(s => s.ExitCountry == countryCode
                                    && !string.IsNullOrEmpty(s.HostCountry))
            .FirstOrDefault()?
            .HostCountry;
    }
}