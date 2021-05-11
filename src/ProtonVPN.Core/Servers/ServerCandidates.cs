/*
 * Copyright (c) 2021 Proton Technologies AG
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
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;

namespace ProtonVPN.Core.Servers
{
    public class ServerCandidates
    {
        private readonly ServerManager _serverManager;

        public ServerCandidates(ServerManager serverManager, IReadOnlyCollection<Server> items)
        {
            _serverManager = serverManager;
            Items = items;
        }

        public IReadOnlyCollection<Server> Items { get; }

        public Server ServerByEntryIpAndLabel(string entryIp, string label)
        {
            if (string.IsNullOrEmpty(entryIp))
            {
                return Server.Empty();
            }

            IReadOnlyCollection<Server> servers = Items == null || Items.Count == 0
                ? _serverManager.GetServers(new ServerByEntryIp(entryIp))
                : Items;

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

            return Server.Empty();
        }
    }
}