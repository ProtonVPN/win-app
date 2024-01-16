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
using ProtonVPN.Client.Logic.Servers.Contracts;

namespace ProtonVPN.Client.Logic.Connection.ServerListGenerators;

public abstract class ServerListGeneratorBase
{
    protected abstract int MaxPhysicalServersPerLogical { get; }

    private readonly Random _random = new();

    protected IEnumerable<PhysicalServer> SelectDistinctPhysicalServers(List<Server> pickedServers)
    {
        return pickedServers
            .SelectMany(SelectPhysicalServers)
            .DistinctBy(s => new { s.EntryIp, s.Label });
    }

    protected IEnumerable<PhysicalServer> SelectPhysicalServers(Server server)
    {
        return server.Servers.Where(s => !s.IsUnderMaintenance).OrderBy(_ => _random.Next()).Take(MaxPhysicalServersPerLogical);
    }

    protected bool IsStandardServer(Server server)
    {
        return !server.Features.IsSupported(ServerFeatures.SecureCore | ServerFeatures.B2B | ServerFeatures.Tor);
    }

    protected string GetCity(List<Server> pickedServers, CityStateLocationIntent? cityStateLocationIntent)
    {
        return string.IsNullOrWhiteSpace(cityStateLocationIntent?.CityState)
            ? pickedServers.First().City
            : cityStateLocationIntent.CityState;
    }

    protected string GetExitCountry(List<Server> pickedServers, CountryLocationIntent? countryLocationIntent)
    {
        return string.IsNullOrWhiteSpace(countryLocationIntent?.CountryCode)
            ? pickedServers.First().ExitCountry
            : countryLocationIntent.CountryCode;
    }

    protected string GetEntryCountry(List<Server> pickedServers, SecureCoreFeatureIntent secureCoreFeatureIntent)
    {
        return string.IsNullOrWhiteSpace(secureCoreFeatureIntent.EntryCountryCode)
            ? pickedServers.First().EntryCountry
            : secureCoreFeatureIntent.EntryCountryCode;
    }

    protected void AddServerIfNotAlreadyListed(List<Server> pickedServers, IEnumerable<Server> unfilteredServers, Func<Server, bool> serverFilter)
    {
        if (!pickedServers.Any(serverFilter))
        {
            Server? server = unfilteredServers.Where(s => !pickedServers.Any(pickedServer => s.Id == pickedServer.Id)).FirstOrDefault(serverFilter);
            if (server is not null)
            {
                pickedServers.Add(server);
            }
        }
    }
}