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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.ServerListGenerators;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Connection.ServerListGenerators;

public class SmartStandardServerListGenerator : ServerListGeneratorBase, ISmartStandardServerListGenerator
{
    private const int MAX_GENERATED_INTENT_LOGICAL_SERVERS = 3;
    private const int MAX_GENERATED_BASE_INTENT_LOGICAL_SERVERS = 1;

    private readonly ISettings _settings;
    private readonly IServersLoader _serversLoader;
    private readonly ILogger _logger;

    protected override int MaxPhysicalServersPerLogical => 1;

    public SmartStandardServerListGenerator(
        ISettings settings,
        IServersLoader serversLoader,
        ILogger logger)
    {
        _settings = settings;
        _serversLoader = serversLoader;
        _logger = logger;
    }

    public IEnumerable<PhysicalServer> Generate(IConnectionIntent connectionIntent, IList<VpnProtocol> preferredProtocols)
    {
        _logger.Debug<AppLog>($"Generating smart servers list for intent: {connectionIntent}");

        IEnumerable<Server> availableServers = GetUnfilteredServers(connectionIntent.Location, preferredProtocols);
        List<Server> pickedServers = [];

        DeviceLocation? deviceLocation = _settings.DeviceLocation;

        // Set how many logical servers matching the exact connection intent (feature + location) should be picked.
        int numberOfServersToPick = MAX_GENERATED_INTENT_LOGICAL_SERVERS;

        if (connectionIntent.Feature is not null)
        {
            // (1.) Select all the servers that match the feature intent only (if defined)
            IEnumerable<Server> featureMatchingServers = connectionIntent.Feature.FilterServers(availableServers);

            // (1.a.) Pick servers from the feature matching servers list that also match the exact location intent.
            // (1.b.) Then pick servers from the feature matching servers list that recursively match the base location intent (eg. server from the same city, server from the same country but different city...)
            pickedServers.AddRange(FilterServersByLayer(connectionIntent.Location, featureMatchingServers, numberOfServersToPick, deviceLocation));

            // If any servers have been picked, reduce the number of servers to pick for the next step.
            numberOfServersToPick = pickedServers.Any()
                ? MAX_GENERATED_BASE_INTENT_LOGICAL_SERVERS
                : MAX_GENERATED_INTENT_LOGICAL_SERVERS;
        }

        // (2.) Select all the standard servers (non B2B/Tor/SecureCore servers). Exclude all the servers that have already been picked.
        IEnumerable<Server> standardServers = availableServers.Where(s => s.IsStandard()).Except(pickedServers);

        // (2.a.) Pick servers from the standard server list that also match the exact location intent.
        // (2.b.) Then pick servers from the standard server list that recursively match the base location intent (eg. server from the same city, server from the same country but different city...)
        pickedServers.AddRange(FilterServersByLayer(connectionIntent.Location, standardServers, numberOfServersToPick, deviceLocation));

        _logger.Debug<AppLog>($"Picked servers: {string.Join(", ", pickedServers.Select(s => s.Name))}");

        return SelectDistinctPhysicalServers(pickedServers, preferredProtocols);
    }

    private IEnumerable<Server> GetUnfilteredServers(ILocationIntent locationIntent, IList<VpnProtocol> preferredProtocols)
    {
        return SortServers(locationIntent, _serversLoader.GetServers().Where(s => s.IsAvailable(preferredProtocols)));
    }

    private IEnumerable<Server> SortServers(ILocationIntent locationIntent, IEnumerable<Server> source)
    {
        IOrderedEnumerable<Server> sortedServers = _settings.IsPortForwardingEnabled
            ? source.OrderByDescending(s => s.Features.IsSupported(ServerFeatures.P2P) && s.IsPaidNonB2B())
            : source.OrderByDescending(s => s.IsPaidNonB2B());

        return locationIntent.Kind == ConnectionIntentKind.Random
            ? sortedServers.ThenBy(_ => Random.Next())
            : sortedServers.ThenBy(s => s.Score);
    }

    /// <summary>
    /// The 'smart' algorithm picks servers that match the location intent then incrementally decreases the strictness of the location intent.
    /// </summary>
    private IEnumerable<Server> FilterServersByLayer(ILocationIntent locationIntent, IEnumerable<Server> servers, int numberOfServersToPick, DeviceLocation? deviceLocation)
    {
        // (1.) Select all the servers that match the location intent and only pick the first # servers from that list.
        IEnumerable<Server> supportedServers = servers.Where(s => locationIntent.IsSupported(s, deviceLocation));
        IEnumerable<Server> pickedServers = supportedServers.Take(numberOfServersToPick);

        // (2.) Create the base location intent from the current location intent.
        ILocationIntent? baseLocationIntent = locationIntent switch
        {
            // Server -> City -> State -> Country -> Fastest country -> Fastest (excluding my country) -> Random country
            ServerLocationIntent serverIntent => string.IsNullOrEmpty(serverIntent.State)
                ? new CityLocationIntent(serverIntent.CountryCode!, serverIntent.City)
                : new CityLocationIntent(serverIntent.CountryCode!, serverIntent.State, serverIntent.City),
            CityLocationIntent cityIntent => string.IsNullOrEmpty(cityIntent.State)
                ? new StateLocationIntent(cityIntent.CountryCode!)
                : new StateLocationIntent(cityIntent.CountryCode!, cityIntent.State),
            StateLocationIntent stateIntent => new CountryLocationIntent(stateIntent.CountryCode!),
            CountryLocationIntent countryIntent when countryIntent.IsSpecificCountry => CountryLocationIntent.Fastest,
            CountryLocationIntent countryIntent when countryIntent.IsFastestCountry => CountryLocationIntent.FastestExcludingMyCountry,
            CountryLocationIntent countryIntent when countryIntent.IsFastestCountryExcludingMine => CountryLocationIntent.Random,

            // Gateway server -> Gateway
            GatewayServerLocationIntent gatewayServerIntent => new GatewayLocationIntent(gatewayServerIntent.Name),

            _ => null,
        };

        // This is the exit condition for the recursive algorithm. Once the top most location intent is reached, its base intent will be null.
        if (baseLocationIntent != null)
        {
            // (3.) Select all the servers that are not supported by the current location intent.
            IEnumerable<Server> remainingServers = servers.Where(s => !locationIntent.IsSupported(s, deviceLocation));

            // (4.) Recursively pick servers from the remaining servers list that match the base location intent. Concatenate the results to the picked servers list. 
            pickedServers = pickedServers.Concat(
                FilterServersByLayer(baseLocationIntent, remainingServers, MAX_GENERATED_BASE_INTENT_LOGICAL_SERVERS, deviceLocation));
        }

        return pickedServers;
    }
}