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

namespace ProtonVPN.Client.Logic.Connection.ServerListGenerators;

public class SmartSecureCoreServerListGenerator : ServerListGeneratorBase, ISmartSecureCoreServerListGenerator
{
    private const int MAX_GENERATED_INTENT_LOGICAL_SERVERS = 3;

    protected override int MaxPhysicalServersPerLogical => 1;

    private readonly IServersLoader _serversLoader;
    private readonly IIntentServerListGenerator _intentServerListGenerator;

    public SmartSecureCoreServerListGenerator(IServersLoader serversLoader,
        IIntentServerListGenerator intentServerListGenerator)
    {
        _serversLoader = serversLoader;
        _intentServerListGenerator = intentServerListGenerator;
    }

    public IEnumerable<PhysicalServer> Generate(SecureCoreFeatureIntent secureCoreFeatureIntent,
        CountryLocationIntent countryLocationIntent)
    {
        List<Server> pickedServers = GenerateIntentServers(secureCoreFeatureIntent, countryLocationIntent).ToList();
        if (pickedServers.Count == 0)
        {
            return _intentServerListGenerator.Generate(new ConnectionIntent(countryLocationIntent, secureCoreFeatureIntent));
        }

        string? entryCountry = GetEntryCountry(pickedServers, secureCoreFeatureIntent);
        string? exitCountry = GetExitCountry(pickedServers, countryLocationIntent);
        IEnumerable<Server> unfilteredServers = GetSecureCoreServers();

        if (exitCountry is not null)
        {
            AddServerIfNotAlreadyListed(pickedServers, unfilteredServers,
                s => IsSameExitDifferentEntry(s, exitCountry: exitCountry, entryCountry: entryCountry));
        }
        if (entryCountry is not null)
        {
            AddServerIfNotAlreadyListed(pickedServers, unfilteredServers,
                s => IsDifferentExitSameEntry(s, exitCountry: exitCountry, entryCountry: entryCountry));
        }
        AddServerIfNotAlreadyListed(pickedServers, unfilteredServers,
            s => IsDifferentExitAndEntry(s, exitCountry: exitCountry, entryCountry: entryCountry));

        return SelectDistinctPhysicalServers(pickedServers);
    }

    private IEnumerable<Server> GenerateIntentServers(SecureCoreFeatureIntent secureCoreFeatureIntent,
        CountryLocationIntent countryLocationIntent)
    {
        IEnumerable<Server> servers = _serversLoader.GetServers();
        servers = countryLocationIntent.FilterServers(servers);
        servers = secureCoreFeatureIntent.FilterServers(servers);

        return SortServers(servers)
            .Where(s => s.IsAvailable())
            .Take(MAX_GENERATED_INTENT_LOGICAL_SERVERS);
    }

    private IEnumerable<Server> SortServers(IEnumerable<Server> source)
    {
        return source.OrderBy(s => s.Score);
    }

    private IEnumerable<Server> GetSecureCoreServers()
    {
        return SortServers(_serversLoader.GetServers().Where(s => s.IsAvailable() && s.Features.IsSupported(ServerFeatures.SecureCore)));
    }

    private bool IsSameExitDifferentEntry(Server server, string exitCountry, string? entryCountry)
    {
        return server.ExitCountry == exitCountry && server.EntryCountry != entryCountry;
    }

    private bool IsDifferentExitSameEntry(Server server, string? exitCountry, string entryCountry)
    {
        return server.ExitCountry != exitCountry && server.EntryCountry == entryCountry;
    }

    private bool IsDifferentExitAndEntry(Server server, string? exitCountry, string? entryCountry)
    {
        return server.ExitCountry != exitCountry && server.EntryCountry != entryCountry;
    }
}