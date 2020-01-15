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
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Servers
{
    public class ServerCandidates
    {
        private readonly ServerManager _serverManager;
        private readonly IUserStorage _userStorage;

        public ServerCandidates(ServerManager serverManager, IUserStorage userStorage, IReadOnlyCollection<Server> items)
        {
            _serverManager = serverManager;
            _userStorage = userStorage;
            Items = items;
        }

        public IReadOnlyCollection<Server> Items { get; }

        public Server ServerByEntryIp(string ip)
        {
            if (string.IsNullOrEmpty(ip))
                return Server.Empty();

            var maxTier = _userStorage.User().MaxTier;
            var servers = Items.Where(l => l.Servers.Any(p => p.EntryIp == ip));
            var server = FirstByUserTier(servers, maxTier);
            if (server != null)
                return WithExitIp(server, ip);

            servers = _serverManager.GetServers(new ServerByEntryIp(ip));
            server = FirstByUserTier(servers, maxTier);
            return WithExitIp(server ?? Server.Empty(), ip);
        }

        private Server FirstByUserTier(IEnumerable<Server> servers, sbyte maxTier)
        {
            return servers
                .OrderByDescending(s => s.Tier <= maxTier ? s.Tier : -1)
                .FirstOrDefault();
        }

        private Server WithExitIp(Server server, string entryIp)
        {
            var result = server.Clone();

            var physical = server.Servers.FirstOrDefault(p => p.EntryIp == entryIp);
            if (physical != null)
            {
                result.ExitIp = physical.ExitIp;
            }

            return result;
        }
    }
}
