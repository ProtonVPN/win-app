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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.ServerListGenerators;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.Logic.Connection.ServerListGenerators;

public class SmartStandardServerListGenerator : ServerListGeneratorBase, ISmartStandardServerListGenerator
{
    private const int MAX_GENERATED_INTENT_PHYSICAL_SERVERS = 3;

    protected override int MaxPhysicalServersPerLogical => 1;

    private readonly ISettings _settings;
    private readonly IServersLoader _serversLoader;

    public SmartStandardServerListGenerator(ISettings settings,
        IServersLoader serversLoader)
    {
        _settings = settings;
        _serversLoader = serversLoader;
    }

    public IEnumerable<PhysicalServer> Generate(IConnectionIntent connectionIntent)
    {
        List<Server> pickedServers = GenerateIntentServers(connectionIntent).ToList();

        string? city = GetCity(pickedServers, connectionIntent.Location as CityStateLocationIntent);
        string? exitCountry = GetExitCountry(pickedServers, connectionIntent.Location as CountryLocationIntent);
        IEnumerable<Server> unfilteredServers = GetUnfilteredServers();
        
        if (exitCountry is not null)
        {
            if (city is not null)
            {
                AddServerIfNotAlreadyListed(pickedServers, unfilteredServers,
                    s => IsSameFeatureAndCountryAndCity(s, connectionIntent.Feature, exitCountry: exitCountry, city: city));
            }
            AddServerIfNotAlreadyListed(pickedServers, unfilteredServers,
                s => IsSameFeatureAndCountryDifferentCity(s, connectionIntent.Feature, exitCountry: exitCountry, city: city));
        }
        if (connectionIntent.Feature is not null)
        {
            AddServerIfNotAlreadyListed(pickedServers, unfilteredServers,
                s => IsSameFeatureDifferentCountry(s, connectionIntent.Feature, exitCountry: exitCountry));
        }
        if (exitCountry is not null)
        {
            if (city is not null)
            {
                AddServerIfNotAlreadyListed(pickedServers, unfilteredServers,
                    s => IsStandardServerSameCity(s, exitCountry: exitCountry, city: city));
            }
            AddServerIfNotAlreadyListed(pickedServers, unfilteredServers,
                s => IsStandardServerSameCountryDifferentCity(s, exitCountry: exitCountry, city: city));
        }
        AddServerIfNotAlreadyListed(pickedServers, unfilteredServers,
            s => IsStandardServerDifferentCountry(s, exitCountry: exitCountry));

        return SelectDistinctPhysicalServers(pickedServers);
    }

    private IEnumerable<Server> GenerateIntentServers(IConnectionIntent connectionIntent)
    {
        IEnumerable<Server> servers = _serversLoader.GetServers();

        if (connectionIntent.Location is not null)
        {
            servers = connectionIntent.Location.FilterServers(servers);
        }

        servers = connectionIntent.Feature is null
            ? servers.Where(IsStandardServer)
            : connectionIntent.Feature.FilterServers(servers);

        return SortServers(servers)
            .Where(s => !s.IsUnderMaintenance())
            .Take(MAX_GENERATED_INTENT_PHYSICAL_SERVERS);
    }

    private IEnumerable<Server> SortServers(IEnumerable<Server> source)
    {
        return _settings.IsPortForwardingEnabled
            ? source.OrderByDescending(s => s.Features.IsSupported(ServerFeatures.P2P)).ThenBy(s => s.Score)
            : source.OrderBy(s => s.Score);
    }

    private IEnumerable<Server> GetUnfilteredServers()
    {
        return SortServers(_serversLoader.GetServers().Where(s => !s.IsUnderMaintenance()));
    }

    private bool IsSameFeatureAndCountryAndCity(Server server,
        IFeatureIntent? featureIntent, string exitCountry, string city)
    {
        return (featureIntent is null ? IsStandardServer(server) : featureIntent.IsSupported(server))
            && server.ExitCountry == exitCountry
            && server.City == city;
    }

    private bool IsSameFeatureAndCountryDifferentCity(Server server,
        IFeatureIntent? featureIntent, string exitCountry, string? city)
    {
        return (featureIntent is null ? IsStandardServer(server) : featureIntent.IsSupported(server))
            && server.ExitCountry == exitCountry
            && server.City != city;
    }

    private bool IsSameFeatureDifferentCountry(Server server, IFeatureIntent featureIntent, string? exitCountry)
    {
        return featureIntent.IsSupported(server) && server.ExitCountry != exitCountry;
    }

    private bool IsStandardServerSameCity(Server server, string exitCountry, string city)
    {
        return IsStandardServer(server) && server.ExitCountry == exitCountry && server.City == city;
    }

    private bool IsStandardServerSameCountryDifferentCity(Server server,  string exitCountry, string? city)
    {
        return IsStandardServer(server) && server.ExitCountry == exitCountry && server.City != city;
    }

    private bool IsStandardServerDifferentCountry(Server server, string? exitCountry)
    {
        return IsStandardServer(server) && server.ExitCountry != exitCountry;
    }
}