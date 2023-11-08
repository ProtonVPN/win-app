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

namespace ProtonVPN.Client.Logic.Servers;

public class ServerManager : IServerManager
{
    private readonly IApiClient _apiClient;

    private List<string> _countries = new();

    private List<LogicalServerResponse> _servers = new();

    public ServerManager(IApiClient apiClient)
    {
        _apiClient = apiClient;
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

    public List<string> GetCitiesByCountry(string countryCode)
    {
        return GetCitiesByFilter(server => server.ExitCountry == countryCode &&
                                        !string.IsNullOrWhiteSpace(server.City));
    }

    public List<string> GetP2PCitiesByCountry(string countryCode)
    {
        return GetCitiesByFilter(server => server.ExitCountry == countryCode &&
                                        ServerFeatures.SupportsP2P(server.Features) &&
                                        !string.IsNullOrWhiteSpace(server.City));
    }

    private List<string> GetCitiesByFilter(Func<LogicalServerResponse, bool> filterFunc)
    {
        return _servers
            .Where(filterFunc)
            .Select(s => s.City)
            .Distinct()
            .ToList();
    }

    public List<Server> GetServersByCity(string city)
    {
        return (from server in _servers where server.City == city select Map(server)).ToList();
    }

    public List<string> GetSecureCoreCountryCodes()
    {
        return GetCountryCodesByFilter(server => ServerFeatures.IsSecureCore(server.Features));
    }

    public List<string> GetP2PCountryCodes()
    {
        return GetCountryCodesByFilter(server => ServerFeatures.SupportsP2P(server.Features));
    }

    public List<string> GetTorCountryCodes()
    {
        return GetCountryCodesByFilter(server => ServerFeatures.SupportsTor(server.Features));
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

    public List<Server> GetSecureCoreServersByExitCountry(string countryCode)
    {
        return GetServers(server => ServerFeatures.IsSecureCore(server.Features) && server.ExitCountry == countryCode);
    }

    public List<Server> GetP2PServersByExitCountry(string countryCode)
    {
        return GetServers(server => ServerFeatures.SupportsP2P(server.Features) && server.ExitCountry == countryCode);
    }

    public List<Server> GetTorServersByExitCountry(string countryCode)
    {
        return GetServers(server => ServerFeatures.SupportsTor(server.Features) && server.ExitCountry == countryCode);
    }

    public List<Server> GetP2PServersByCity(string city)
    {
        return GetServers(server => ServerFeatures.SupportsP2P(server.Features) && server.City == city);
    }

    private List<Server> GetServers(Func<LogicalServerResponse, bool> filterFunc)
    {
        return _servers
            .Where(filterFunc)
            .Select(Map)
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

    private static Server Map(LogicalServerResponse item)
    {
        if (item == null)
        {
            return null;
        }

        List<PhysicalServer> physicalServers = item.Servers.Select(Map).ToList();

        return new Server
        {
            Id = item.Id,
            Name = item.Name,
            City = item.City,
            EntryCountry = item.EntryCountry,
            ExitCountry = item.ExitCountry,
            Domain = item.Domain,
            ExitIp = ExitIp(physicalServers),
            Status = item.Status,
            Tier = item.Tier,
            Features = item.Features,
            Load = item.Load,
            Score = item.Score,
            Servers = physicalServers,
            IsVirtual = !string.IsNullOrEmpty(item.HostCountry),
            IsSecureCore = ServerFeatures.IsSecureCore(item.Features),
            SupportsP2P = ServerFeatures.SupportsP2P(item.Features),
            SupportsTor = ServerFeatures.SupportsTor(item.Features),
            IsUnderMaintenance = item.Status == 0,
        };
    }

    private static PhysicalServer Map(PhysicalServerResponse server)
    {
        return new(
            id: server.Id,
            entryIp: server.EntryIp,
            exitIp: server.ExitIp,
            domain: server.Domain,
            label: server.Label,
            status: server.Status,
            x25519PublicKey: server.X25519PublicKey,
            signature: server.Signature);
    }

    /// <summary>
    /// If ExitIp is same on all physical servers, it is returned.
    /// </summary>
    private static string ExitIp(IEnumerable<PhysicalServer> servers)
    {
        return servers.Aggregate(
            (string)null,
            (ip, p) => ip == null || ip == p.ExitIp ? p.ExitIp : "",
            ip => !string.IsNullOrEmpty(ip) ? ip : null);
    }
}