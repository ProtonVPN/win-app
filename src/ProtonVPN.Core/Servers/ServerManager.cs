/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Servers
{
    public class ServerManager
    {
        private readonly ServerNameComparer _serverNameComparer;
        private List<LogicalServerContract> _servers = new List<LogicalServerContract>();
        private List<string> _countries = new List<string>();
        private readonly IUserStorage _userStorage;

        public ServerManager(IUserStorage userStorage)
        {
            _userStorage = userStorage;
            _serverNameComparer = new ServerNameComparer();
        }

        public ServerManager(IUserStorage userStorage, List<LogicalServerContract> servers) : this(userStorage)
        {
            _servers = servers;
        }

        public void Load(ICollection<LogicalServerContract> servers)
        {
            Ensure.NotEmpty(servers, nameof(servers));
            SaveServers(servers);
            SaveCountries(servers);
        }

        public IReadOnlyCollection<Server> GetServers(ISpecification<LogicalServerContract> spec)
        {
            return _servers
                .Where(spec.IsSatisfiedBy)
                .Select(Map)
                .OrderBy(s => s.Name.ContainsIgnoringCase("free") ? 0 : 1)
                .ThenBy(s => _userStorage.User().MaxTier < s.Tier)
                .ThenBy(s => s.Name, _serverNameComparer)
                .ToList();
        }

        public Server GetServer(ISpecification<LogicalServerContract> spec)
        {
            return Map(_servers.Find(spec.IsSatisfiedBy));
        }

        public virtual IReadOnlyCollection<string> GetCountries()
        {
            return _countries;
        }

        public virtual IReadOnlyCollection<string> GetCountriesWithFreeServers()
        {
            var result = new List<string>();
            foreach (var server in _servers)
            {
                if (server.Tier.Equals(ServerTiers.Free) && !result.Contains(server.EntryCountry))
                {
                    result.Add(server.EntryCountry);
                }
            }

            return result;
        }

        public bool CountryHasAvailableServers(string country, sbyte userTier)
        {
            var servers = GetServers(new EntryCountryServer(country) && !new TorServer());
            return servers.FirstOrDefault(s => userTier >= s.Tier) != null;
        }

        public bool CountryHasAvailableSecureCoreServers(string country, sbyte userTier)
        {
            var servers = GetServers(new SecureCoreServer() && new ExitCountryServer(country));
            return servers.FirstOrDefault(s => userTier >= s.Tier) != null;
        }

        private void SaveServers(IEnumerable<LogicalServerContract> servers)
        {
            _servers = servers.Where(s => s != null).ToList();
        }

        private void SaveCountries(IEnumerable<LogicalServerContract> servers)
        {
            var countryCodes = new List<string>();
            foreach (var server in servers)
            {
                if (server == null)
                    continue;
                if (!IsCountry(server))
                    continue;
                if (countryCodes.Contains(server.EntryCountry))
                    continue;

                countryCodes.Add(server.EntryCountry);
            }

            _countries = countryCodes;
        }

        private static bool IsCountry(LogicalServerContract server)
        {
            var code = server.EntryCountry;
            if (code.Equals("AA") || code.Equals("ZZ") || code.StartsWith("X"))
                return false;
            var letters = new[] { "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            if (code.StartsWith("Q") && letters.Contains(code.Substring(1, 1)))
                return false;

            return true;
        }

        private static Server Map(LogicalServerContract item)
        {
            if (item == null)
                return null;

            var physicalServers = item.Servers.Select(Map).ToList();

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
                item.Location,
                physicalServers,
                ExitIp(physicalServers)
                );
        }

        private static PhysicalServer Map(PhysicalServerContract server)
        {
            return new PhysicalServer(server.EntryIp, server.ExitIp, server.Domain, server.Status);
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
