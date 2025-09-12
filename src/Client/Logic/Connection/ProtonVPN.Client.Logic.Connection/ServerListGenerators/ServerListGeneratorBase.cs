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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Logic.Connection.ServerListGenerators;

public abstract class ServerListGeneratorBase
{
    protected readonly Random Random = new();

    protected readonly ISettings Settings;
    protected readonly IServersLoader ServersLoader;
    protected readonly ILogger Logger;

    protected abstract int MaxPhysicalServersPerLogical { get; }
    protected abstract int MaxPhysicalServersInTotal { get; }

    protected ServerListGeneratorBase(
        ISettings settings,
        IServersLoader serversLoader,
        ILogger logger)
    {
        Settings = settings;
        ServersLoader = serversLoader;
        Logger = logger;
    }

    protected IOrderedEnumerable<Server> SelectLogicalServers(IConnectionIntent connectionIntent, IList<VpnProtocol> preferredProtocols)
    {
        return SelectLogicalServers(ServersLoader.GetServers(), connectionIntent, preferredProtocols);
    }

    protected IOrderedEnumerable<Server> SelectLogicalServers(IEnumerable<Server> servers, IConnectionIntent connectionIntent, IList<VpnProtocol> preferredProtocols)
    {
        bool isPortForwardingEnabled = connectionIntent is IConnectionProfile profile
            ? profile.Settings.IsPortForwardingEnabled
            : Settings.IsPortForwardingEnabled;

        return connectionIntent
            .FilterAndSortServers(servers, Settings.DeviceLocation, preferredProtocols, isPortForwardingEnabled);
    }

    protected IEnumerable<PhysicalServer> SelectDistinctPhysicalServers(List<Server> pickedServers, IList<VpnProtocol> preferredProtocols)
    {
        return pickedServers
            .SelectMany(s => SelectPhysicalServers(s, preferredProtocols))
            .DistinctBy(s => new { s.EntryIp, s.Label })
            .Take(MaxPhysicalServersInTotal);
    }

    protected IEnumerable<PhysicalServer> SelectPhysicalServers(Server server, IList<VpnProtocol> preferredProtocols)
    {
        return server.Servers
            .Where(s => s.IsAvailable(preferredProtocols))
            .OrderBy(_ => Random.Next())
            .Take(MaxPhysicalServersPerLogical) ?? [];
    }
}