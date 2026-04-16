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
using ProtonVPN.Client.Logic.Connection.Contracts.Preferences;
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
        IExclusionChecker exclusionChecker,
        ILogger logger)
        : base(settings, serversLoader, exclusionChecker, logger)
    { }

    public ServerListResult Generate(IConnectionIntent connectionIntent, IList<VpnProtocol> preferredProtocols)
    {
        Logger.Debug<AppLog>($"Generating servers list for intent: {connectionIntent}");

        List<Server> servers = SelectLogicalServers(connectionIntent, preferredProtocols, applyExclusions: true)
            .Take(MAX_LOGICAL_SERVERS_IN_TOTAL)
            .ToList();

        ServerListDiagnostic diagnostic = DetermineExclusionDiagnostic(servers.Count,
            () => SelectLogicalServers(connectionIntent, preferredProtocols, applyExclusions: false).Any());

        Logger.Debug<AppLog>($"Generated servers list: {string.Join(", ", servers.Select(s => s.Name))}");

        IReadOnlyList<PhysicalServer> physicalServers = SelectDistinctPhysicalServers(servers, preferredProtocols).ToList();
        
        return new ServerListResult(physicalServers, diagnostic);
    }
}