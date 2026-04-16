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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Connection.Contracts.Preferences;
using ProtonVPN.Client.Logic.Connection.Contracts.ServerListGenerators;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Logic.Connection.ServerListGenerators;

public abstract partial class ServerListGeneratorBase
{
    protected readonly Random Random = new();

    protected readonly ISettings Settings;
    protected readonly IServersLoader ServersLoader;
    protected readonly IExclusionChecker ExclusionChecker;
    protected readonly ILogger Logger;

    protected abstract int MaxPhysicalServersPerLogical { get; }
    protected abstract int MaxPhysicalServersInTotal { get; }

    protected ServerListGeneratorBase(
        ISettings settings,
        IServersLoader serversLoader,  
        IExclusionChecker exclusionChecker,
        ILogger logger)
    {
        Settings = settings;
        ServersLoader = serversLoader;
        ExclusionChecker = exclusionChecker;
        Logger = logger;
    }

    protected IEnumerable<Server> GetAvailableServers(IConnectionIntent connectionIntent, bool applyExclusions = true)
    {
        IEnumerable<Server> servers = ServersLoader.GetServers();

        bool shouldExclude = applyExclusions && ShouldApplyExclusionFilter(connectionIntent);

        return shouldExclude
            ? servers.Where(s => !ExclusionChecker.IsServerExcluded(s))
            : servers;
    }

    protected IOrderedEnumerable<Server> SelectLogicalServers(
        IConnectionIntent connectionIntent,
        IList<VpnProtocol> preferredProtocols,
        bool applyExclusions = true)
    {
        IEnumerable<Server> servers = GetAvailableServers(connectionIntent, applyExclusions);
        return SelectLogicalServers(servers, connectionIntent, preferredProtocols);
    }

    protected IOrderedEnumerable<Server> SelectLogicalServers(
        IEnumerable<Server> servers,
        IConnectionIntent connectionIntent,
        IList<VpnProtocol> preferredProtocols)
    {
        IList<Server> serverList = servers as IList<Server> ?? servers.ToList();

        bool isPortForwardingEnabled = connectionIntent is IConnectionProfile profile
            ? profile.Settings.IsPortForwardingEnabled
            : Settings.IsPortForwardingEnabled;

        return connectionIntent
            .FilterAndSortServers(serverList, Settings.DeviceLocation, preferredProtocols, isPortForwardingEnabled);
    }

    protected ServerListDiagnostic DetermineExclusionDiagnostic(int serverCount, Func<bool> checkUnfilteredHasResults)
    {
        bool areAllCandidatesExcluded = serverCount == 0 && checkUnfilteredHasResults();
        return new ServerListDiagnostic(areAllCandidatesExcluded);
    }

    private bool ShouldApplyExclusionFilter(IConnectionIntent connectionIntent)
    {
        return ExclusionChecker.HasExcludedLocations
            && Settings.VpnPlan.IsPaid
            && connectionIntent.Location is MultiCountryLocationIntent intent
            && intent.IsSelectionEmpty;
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