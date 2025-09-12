/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Client.Logic.Connection.Contracts.ServerListGenerators;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Connection.ServerListGenerators;

public class ServerListGenerator : ServerListGeneratorBase, IServerListGenerator
{
    private const int MAX_LOGICAL_SERVERS_IN_TOTAL = 64;

    protected override int MaxPhysicalServersPerLogical => 2;

    protected override int MaxPhysicalServersInTotal => 64;

    public ServerListGenerator(
        ISettings settings,
        IServersLoader serversLoader,
        ILogger logger)
        : base(settings, serversLoader, logger)
    { }

    public IEnumerable<PhysicalServer> Generate(IConnectionIntent connectionIntent, IList<VpnProtocol> preferredProtocols)
    {
        Logger.Debug<AppLog>($"Generating servers list for intent: {connectionIntent}");

        List<Server> servers = SelectLogicalServers(connectionIntent, preferredProtocols)
            .Take(MAX_LOGICAL_SERVERS_IN_TOTAL)
            .ToList();

        Logger.Debug<AppLog>($"Generated servers list: {string.Join(", ", servers.Select(s => s.Name))}");

        return SelectDistinctPhysicalServers(servers, preferredProtocols);
    }
}