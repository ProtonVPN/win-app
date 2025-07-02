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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Logic.Connection.ServerListGenerators;

public abstract class ServerListGeneratorBase
{
    protected readonly Random Random = new();

    protected abstract int MaxPhysicalServersPerLogical { get; }

    protected IEnumerable<PhysicalServer> SelectDistinctPhysicalServers(List<Server> pickedServers, IList<VpnProtocol> preferredProtocols)
    {
        return pickedServers
            .SelectMany(s => SelectPhysicalServers(s, preferredProtocols))
            .DistinctBy(s => new { s.EntryIp, s.Label });
    }

    protected IEnumerable<PhysicalServer> SelectPhysicalServers(Server server, IList<VpnProtocol> preferredProtocols)
    {
        return server.Servers?.Where(s => s.IsAvailable(preferredProtocols)).OrderBy(_ => Random.Next()).Take(MaxPhysicalServersPerLogical) ?? [];
    }

    protected string? GetCity(List<Server> pickedServers, CityLocationIntent? cityLocationIntent)
    {
        string? intentCity = cityLocationIntent?.City;
        string? firstPickedServerCity = pickedServers.FirstOrDefault()?.City;

        return string.IsNullOrWhiteSpace(intentCity)
            ? string.IsNullOrWhiteSpace(firstPickedServerCity)
                ? null
                : firstPickedServerCity
            : intentCity;
    }

    protected string? GetState(List<Server> pickedServers, StateLocationIntent? stateLocationIntent)
    {
        string? intentState = stateLocationIntent?.State;
        string? firstPickedServerState = pickedServers.FirstOrDefault()?.State;

        return string.IsNullOrWhiteSpace(intentState)
            ? string.IsNullOrWhiteSpace(firstPickedServerState)
                ? null
                : firstPickedServerState
            : intentState;
    }

    protected string? GetExitCountry(List<Server> pickedServers, CountryLocationIntent? countryLocationIntent)
    {
        string? intentExitCountry = countryLocationIntent?.CountryCode;
        string? firstPickedServerExitCountry = pickedServers.FirstOrDefault()?.ExitCountry;

        return string.IsNullOrWhiteSpace(intentExitCountry)
            ? string.IsNullOrWhiteSpace(firstPickedServerExitCountry)
                ? null
                : firstPickedServerExitCountry
            : intentExitCountry;
    }

    protected string? GetEntryCountry(List<Server> pickedServers, SecureCoreFeatureIntent secureCoreFeatureIntent)
    {
        string? intentEntryCountry = secureCoreFeatureIntent?.EntryCountryCode;
        string? firstPickedServerEntryCountry = pickedServers.FirstOrDefault()?.EntryCountry;

        return string.IsNullOrWhiteSpace(intentEntryCountry)
            ? string.IsNullOrWhiteSpace(firstPickedServerEntryCountry)
                ? null
                : firstPickedServerEntryCountry
            : intentEntryCountry;
    }

    protected void AddServerIfNotAlreadyListed(List<Server> pickedServers, IEnumerable<Server> unfilteredServers, Func<Server, bool> serverFilter)
    {
        Server? server = unfilteredServers.Where(s => !pickedServers.Any(pickedServer => s.Id == pickedServer.Id)).FirstOrDefault(serverFilter);
        if (server is not null)
        {
            pickedServers.Add(server);
        }
    }
}