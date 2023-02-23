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

using System.Collections.Generic;
using System.Linq;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;
using PhysicalServerResponse = ProtonVPN.Api.Contracts.Servers.PhysicalServerResponse;

namespace ProtonVPN.Core.Servers
{
    public class ServerManager
    {
        private readonly IUserStorage _userStorage;
        private readonly IAppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly ServerNameComparer _serverNameComparer;

        private List<LogicalServerResponse> _servers = new();
        private List<string> _countries = new();

        public ServerManager(IUserStorage userStorage, IAppSettings appSettings, ILogger logger)
        {
            _userStorage = userStorage;
            _appSettings = appSettings;
            _logger = logger;
            _serverNameComparer = new();
        }

        public bool IsServerFromSpec(Server server, ISpecification<LogicalServerResponse> spec)
        {
            return _servers.Where(s => s.Id == server.Id).Where(spec.IsSatisfiedBy).Any();
        }

        public void Load(IReadOnlyCollection<LogicalServerResponse> servers)
        {
            Ensure.NotEmpty(servers, nameof(servers));

            int previousNumOfServers = _servers.Count;
            int previousNumOfCountries = _countries.Count;

            SaveServers(servers);
            SaveCountries(servers);

            LogServerListUpdate(previousNumOfServers, previousNumOfCountries);
        }

        private void LogServerListUpdate(int previousNumOfServers, int previousNumOfCountries)
        {
            string numOfServersText = previousNumOfServers == _servers.Count
                ? $"{_servers.Count}"
                : $"{previousNumOfServers} -> {_servers.Count}";

            string numOfCountriesText = previousNumOfCountries == _countries.Count
                ? $"{_countries.Count}"
                : $"{previousNumOfCountries} -> {_countries.Count}";

            _logger.Info<AppLog>($"Servers updated. Num of servers: {numOfServersText} Num of countries: {numOfCountriesText}");
        }

        public virtual void UpdateLoads(IReadOnlyCollection<LogicalServerResponse> servers)
        {
            Dictionary<string, LogicalServerResponse> updatedServers = servers.ToDictionary(server => server.Id);
            foreach (LogicalServerResponse server in _servers.Where(server => updatedServers.ContainsKey(server.Id)))
            {
                LogicalServerResponse updatedServer = updatedServers[server.Id];
                server.Load = updatedServer.Load;
                server.Score = updatedServer.Score;
                if (updatedServer.Status == 0 || server.Servers.Count == 1)
                {
                    foreach (PhysicalServerResponse physicalServer in server.Servers)
                    {
                        physicalServer.Status = updatedServer.Status;
                    }
                    server.Status = updatedServer.Status;
                }
            }
        }

        public IReadOnlyCollection<Server> GetServers(ISpecification<LogicalServerResponse> spec, 
            Features orderBy = Features.None)
        {
            sbyte userTier = _userStorage.GetUser().MaxTier;

            return _servers
                .Where(spec.IsSatisfiedBy)
                .Select(Map)
                .OrderBy(s => s.Name.ContainsIgnoringCase("free") ? 0 : 1)
                .ThenBy(s => ServerFeatures.IsPartner(s.Features) ? 1 : 0)
                .ThenBy(s => userTier < s.Tier)
                .ThenByDescending(s => s.IsFeatureSupported(orderBy))
                .ThenBy(s => s.Name, _serverNameComparer)
                .ToList();
        }

        public PhysicalServer GetPhysicalServerByServer(Server server)
        {
            return (from logical in _servers
                    from physical in logical.Servers
                    where logical.Id == server.Id && physical.ExitIp == server.ExitIp
                    select Map(physical))
                .FirstOrDefault();
        }

        public virtual Server GetServer(ISpecification<LogicalServerResponse> spec)
        {
            return Map(_servers.Find(spec.IsSatisfiedBy));
        }

        public Server GetServerByEntryIpAndLabel(string entryIp, string label)
        {
            IReadOnlyCollection<Server> servers = GetServers(new ServerByEntryIp(entryIp));

            foreach (Server server in servers)
            {
                foreach (PhysicalServer physicalServer in server.Servers)
                {
                    if (entryIp == physicalServer.EntryIp && (string.IsNullOrEmpty(label) || label == physicalServer.Label))
                    {
                        Server clone = server.Clone();
                        clone.ExitIp = physicalServer.ExitIp;
                        return clone;
                    }
                }
            }

            _logger.Error<AppLog>($"Failed to find any server matching EntryIp '{entryIp}' and Label '{label}'. " +
                                  $"There are {servers.Count} server(s) matching EntryIp '{entryIp}'.");
            return Server.Empty();
        }

        public void MarkServerUnderMaintenance(string exitIp)
        {
            foreach (PhysicalServerResponse server in _servers.SelectMany(logical =>
                logical.Servers.Where(server => server.ExitIp == exitIp)))
            {
                server.Status = 0;
            }
        }

        public virtual IReadOnlyCollection<string> GetCountries()
        {
            return _countries;
        }

        public IList<string> GetCountriesByTier(params sbyte[] tiers)
        {
            IList<string> result = new List<string>();

            foreach (LogicalServerResponse server in _servers)
            {
                if (tiers.Contains(server.Tier) &&
                    !ServerFeatures.IsSecureCore(server.Features) &&
                    !result.Contains(server.EntryCountry))
                {
                    result.Add(server.EntryCountry);
                }
            }

            return result;
        }

        public IList<string> GetEntryCountriesBySpec(Specification<LogicalServerResponse> specification)
        {
            IList<string> list = new List<string>();
            IReadOnlyCollection<Server> servers = GetServers(specification);

            foreach (Server server in servers)
            {
                if (!list.Contains(server.EntryCountry))
                {
                    list.Add(server.EntryCountry);
                }
            }

            return list;
        }

        public IList<string> GetSecureCoreCountries()
        {
            IList<string> list = new List<string>();
            IReadOnlyCollection<Server> servers = GetServers(new SecureCoreServer());

            foreach (Server server in servers)
            {
                if (!list.Contains(server.ExitCountry))
                {
                    list.Add(server.ExitCountry);
                }
            }

            return list;
        }

        public bool IsCountryVirtual(string countryCode)
        {
            return _servers.Any(server => server.ExitCountry == countryCode && !string.IsNullOrEmpty(server.HostCountry));
        }

        public bool CountryHasAvailableServers(string country, sbyte userTier)
        {
            IReadOnlyCollection<Server> servers = GetServers(new EntryCountryServer(country) && !new TorServer());
            return servers.FirstOrDefault(s => userTier >= s.Tier) != null;
        }

        public bool CountryHasAvailableSecureCoreServers(string country, sbyte userTier)
        {
            IReadOnlyCollection<Server> servers = GetServers(new SecureCoreServer() && new ExitCountryServer(country));
            return servers.FirstOrDefault(s => userTier >= s.Tier) != null;
        }

        public bool CountryUnderMaintenance(string country)
        {
            IReadOnlyCollection<Server> servers = GetServers(new OnlineServer() && new ExitCountryServer(country));
            return servers.Count == 0;
        }

        public bool Empty() => !_servers.Any();

        private void SaveServers(IEnumerable<LogicalServerResponse> servers)
        {
            if (_appSettings.GetProtocol() == VpnProtocol.WireGuard)
            {
                List<LogicalServerResponse> filteredServers = new();
                foreach (LogicalServerResponse server in servers)
                {
                    if (server == null)
                    {
                        continue;
                    }

                    List<PhysicalServerResponse> physicalServers = server.Servers.Where(ContainsPublicKey).ToList();
                    if (physicalServers.Count == 0)
                    {
                        continue;
                    }

                    server.Servers = physicalServers;
                    filteredServers.Add(server);
                }

                _servers = filteredServers;
            }
            else
            {
                _servers = servers.Where(s => s != null).ToList();
            }
        }

        private bool ContainsPublicKey(PhysicalServerResponse server)
        {
            return !server.X25519PublicKey.IsNullOrEmpty();
        }

        private void SaveCountries(IEnumerable<LogicalServerResponse> servers)
        {
            List<string> countryCodes = new();

            foreach (LogicalServerResponse server in servers)
            {
                if (server == null || !IsCountry(server) || countryCodes.Contains(server.EntryCountry))
                {
                    continue;
                }

                if (_appSettings.GetProtocol() == VpnProtocol.WireGuard &&
                    server.Servers.Count(s => !s.X25519PublicKey.IsNullOrEmpty()) == 0)
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

            return new Server(
                item.Id,
                item.Name,
                item.City,
                item.EntryCountry,
                item.ExitCountry,
                item.Domain,
                item.Status,
                item.Tier,
                item.Features,
                item.Load,
                item.Score,
                item.LocationResponse,
                physicalServers,
                ExitIp(physicalServers)
            );
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
}