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

using ProtonVPN.Client.Logic.Servers.Cache;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Servers;

public class ServersLoader : IServersLoader
{
    private readonly IServersCache _serversCache;
    private readonly IServerCountCache _serverCountCache;

    public ServersLoader(
        IServersCache serversCache,
        IServerCountCache serverCountCache)
    {
        _serversCache = serversCache;
        _serverCountCache = serverCountCache;
    }

    public int GetServerCount()
    {
        return _serverCountCache.GetServerCount();
    }

    public int GetCountryCount()
    {
        return _serverCountCache.GetCountryCount();
    }

    public Server? GetById(string serverId)
    {
        return GetServers().FirstOrDefault(s => s.Id == serverId);
    }

    public IEnumerable<FreeCountry> GetFreeCountries()
    {
        return _serversCache.FreeCountries;
    }

    public IEnumerable<Country> GetCountries()
    {
        return _serversCache.Countries;
    }

    public IEnumerable<Country> GetCountriesByFeatures(ServerFeatures serverFeatures)
    {
        return _serversCache.Countries.Where(c => c.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<State> GetStates()
    {
        return _serversCache.States;
    }

    public IEnumerable<State> GetStatesByCountryCode(string countryCode)
    {
        return _serversCache.States.Where(s => s.CountryCode == countryCode);
    }

    public IEnumerable<State> GetStatesByFeatures(ServerFeatures serverFeatures)
    {
        return _serversCache.States.Where(s => s.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<State> GetStatesByFeaturesAndCountryCode(ServerFeatures serverFeatures, string countryCode)
    {
        return _serversCache.States.Where(s => s.CountryCode == countryCode
                                            && s.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<City> GetCities()
    {
        return _serversCache.Cities;
    }

    public IEnumerable<City> GetCitiesByCountryCode(string countryCode)
    {
        return _serversCache.Cities.Where(c => c.CountryCode == countryCode);
    }

    public IEnumerable<City> GetCitiesByState(State state)
    {
        return _serversCache.Cities.Where(c => c.CountryCode == state.CountryCode
                                            && c.StateName == state.Name);
    }

    public IEnumerable<City> GetCitiesByFeatures(ServerFeatures serverFeatures)
    {
        return _serversCache.Cities.Where(c => c.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<City> GetCitiesByFeaturesAndCountryCode(ServerFeatures serverFeatures, string countryCode)
    {
        return _serversCache.Cities.Where(c => c.CountryCode == countryCode
                                            && c.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<City> GetCitiesByFeaturesAndState(ServerFeatures serverFeatures, State state)
    {
        return _serversCache.Cities.Where(c => c.CountryCode == state.CountryCode
                                            && c.StateName == state.Name
                                            && c.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<Server> GetServers()
    {
        return _serversCache.Servers;
    }

    private IEnumerable<Server> GetPaidServersByFilter(Func<Server, bool>? filterFunc)
    {
        return GetServers()
            .Where(s => s.Tier is ServerTiers.Basic or ServerTiers.Plus
                     && (filterFunc is null || filterFunc(s))
                     && !s.Features.IsB2B());
    }

    private IEnumerable<Server> GetFreeServersByFilter(Func<Server, bool>? filterFunc)
    {
        return GetServers()
            .Where(s => s.Tier == ServerTiers.Free
                     && (filterFunc is null || filterFunc(s))
                     && !s.Features.IsB2B());
    }

    private IEnumerable<Server> GetGatewayServersByFilter(Func<Server, bool>? filterFunc)
    {
        return GetServers()
            .Where(s => (filterFunc is null || filterFunc(s))
                     && s.Features.IsB2B());
    }

    public IEnumerable<Server> GetFreeServers()
    {
        return GetFreeServersByFilter(null);
    }

    public IEnumerable<Server> GetFreeServersByFeatures(ServerFeatures serverFeatures)
    {
        return GetFreeServersByFilter(s => s.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<Server> GetServersByState(State state)
    {
        return GetPaidServersByFilter(s => s.ExitCountry == state.CountryCode
                                        && s.State == state.Name
                                        && s.IsStandard());
    }

    public IEnumerable<Server> GetServersByCity(City city)
    {
        return GetPaidServersByFilter(s => s.ExitCountry == city.CountryCode
                                        && s.State == city.StateName
                                        && s.City == city.Name
                                        && s.IsStandard());
    }

    public IEnumerable<Server> GetServersByFeatures(ServerFeatures serverFeatures)
    {
        return GetPaidServersByFilter(s => s.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<Server> GetServersByFeaturesAndCountryCode(ServerFeatures serverFeatures, string countryCode)
    {
        return GetPaidServersByFilter(s => s.ExitCountry == countryCode
                                        && s.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<Server> GetServersByFeaturesAndState(ServerFeatures serverFeatures, State state)
    {
        return GetPaidServersByFilter(s => s.ExitCountry == state.CountryCode
                                        && s.State == state.Name
                                        && s.Features.IsSupported(serverFeatures));
    }
    public IEnumerable<Server> GetServersByFeaturesAndCity(ServerFeatures serverFeatures, City city)
    {
        return GetPaidServersByFilter(s => s.ExitCountry == city.CountryCode
                                        && s.State == city.StateName
                                        && s.City == city.Name
                                        && s.Features.IsSupported(serverFeatures));
    }

    public IEnumerable<SecureCoreCountryPair> GetSecureCoreCountryPairs()
    {
        return _serversCache.SecureCoreCountryPairs;
    }

    public IEnumerable<SecureCoreCountryPair> GetSecureCoreCountryPairsByExitCountryCode(string exitCountryCode)
    {
        return GetSecureCoreCountryPairs()
            .Where(sccp => sccp.ExitCountry == exitCountryCode);
    }

    public IEnumerable<Server> GetServersBySecureCoreCountryPair(SecureCoreCountryPair countryPair)
    {
        return GetPaidServersByFilter(s => s.ExitCountry == countryPair.ExitCountry
                                    && s.EntryCountry == countryPair.EntryCountry
                                    && s.Features.IsSupported(ServerFeatures.SecureCore));
    }

    public IEnumerable<Gateway> GetGateways()
    {
        return _serversCache.Gateways;
    }

    public IEnumerable<Server> GetServersByGatewayName(string gatewayName)
    {
        return GetGatewayServersByFilter(s => s.GatewayName == gatewayName);
    }

    public bool HasAnyGateways()
    {
        return _serversCache.Gateways.Any();
    }

    public string? GetHostCountryCode(string countryCode)
    {
        return GetPaidServersByFilter(s => s.ExitCountry == countryCode
                                    && !string.IsNullOrEmpty(s.HostCountry))
            .FirstOrDefault()? // WARNING: There can be more than one host country, we are ignoring others
            .HostCountry;
    }
}