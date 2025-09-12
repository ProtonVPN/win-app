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

using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Connection.Contracts.ServerListGenerators;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Connection.ServerListGenerators;

public class SmartServerListGenerator : ServerListGeneratorBase, ISmartServerListGenerator
{
    private const int MAX_GENERATED_INTENT_LOGICAL_SERVERS = 3;
    private const int MAX_GENERATED_BASE_INTENT_LOGICAL_SERVERS = 1;

    protected override int MaxPhysicalServersPerLogical => 1;

    protected override int MaxPhysicalServersInTotal => 64;

    public SmartServerListGenerator(
        ISettings settings,
        IServersLoader serversLoader,
        ILogger logger)
        : base(settings, serversLoader, logger)
    { }

    public IEnumerable<PhysicalServer> Generate(IConnectionIntent connectionIntent, IList<VpnProtocol> preferredProtocols)
    {
        Logger.Debug<AppLog>($"Generating smart servers list for intent: {connectionIntent}");

        List<Server> availableServers = [.. ServersLoader.GetServers()];
        List<Server> servers = [];

        IEnumerable<ILocationIntent> locationIntents = connectionIntent.Location.GetIntentHierarchy();
        IEnumerable<IFeatureIntent> featureIntents = connectionIntent.Feature.GetIntentHierarchy();

        // Phase 1: iterate over all (location, feature) combinations
        foreach (ILocationIntent locationIntent in locationIntents)
        {
            int numberOfServersToPick = GetNumberOfServersToPick(servers.Count);

            foreach (IFeatureIntent featureIntent in featureIntents)
            {
                ConnectionIntent intent = new(locationIntent, featureIntent);
                List<Server> matchingServers = PickThenRemoveServers(availableServers, intent, preferredProtocols, numberOfServersToPick);

                servers.AddRange(matchingServers);
            }
        }

        // Phase 2: location-only fallback
        foreach (ILocationIntent locationIntent in locationIntents)
        {
            int numberOfServersToPick = GetNumberOfServersToPick(servers.Count);

            ConnectionIntent intent = new(locationIntent);
            List<Server> matchingServers = PickThenRemoveServers(availableServers, intent, preferredProtocols, numberOfServersToPick);

            servers.AddRange(matchingServers);
        }

        Logger.Debug<AppLog>($"Generated smart servers list: {string.Join(", ", servers.Select(s => s.Name))}");

        return SelectDistinctPhysicalServers(servers, preferredProtocols);
    }

    /// <summary>
    /// Pick # servers from the available servers list that match the connection intent.
    /// Then remove all the servers that match the connection intent from the available servers list.
    /// </summary>
    /// <param name="availableServers"></param>
    /// <param name="connectionIntent"></param>
    /// <param name="numberOfServersToPick"></param>
    /// <returns></returns>
    private List<Server> PickThenRemoveServers(
        List<Server> availableServers,
        IConnectionIntent connectionIntent,
        IList<VpnProtocol> preferredProtocols,
        int numberOfServersToPick)
    {
        // Get all the servers that match the current connection intent and pick the first one(s).
        List<Server> supportedServers = SelectLogicalServers(availableServers, connectionIntent, preferredProtocols).ToList();
        List<Server> pickedServers = [.. supportedServers.Take(numberOfServersToPick)];

        if (connectionIntent.Location is MultiCountryLocationIntent countryintent && countryintent.IsSelectionEmpty)
        {
            // Get all the servers that are located in the same countries as the ones picked.
            List<string> pickedCountries = pickedServers.Select(s => s.ExitCountry).Distinct().ToList();
            List<Server> serversWithSameLocation = supportedServers.Where(s => pickedCountries.Contains(s.ExitCountry, StringComparer.OrdinalIgnoreCase)).ToList();
            // Remove the servers from the same countries as the ones picked from the available servers.
            foreach (Server server in serversWithSameLocation)
            {
                availableServers.Remove(server);
            }
        }
        else
        {
            // Remove the servers that were supported at this step from the available servers.
            foreach (Server server in supportedServers)
            {
                availableServers.Remove(server);
            }
        }

        // Return the first # servers from the supported servers list.
        return pickedServers;
    }

    /// <summary>
    /// Get how many logical servers should be picked based on the current number of servers already picked.
    /// </summary>
    private int GetNumberOfServersToPick(int currentServersCount)
    {
        return currentServersCount > 0
            ? MAX_GENERATED_BASE_INTENT_LOGICAL_SERVERS
            : MAX_GENERATED_INTENT_LOGICAL_SERVERS;
    }
}