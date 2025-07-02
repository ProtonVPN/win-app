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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.ServerListGenerators;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Logic.Connection.ServerListGenerators;

public class SmartSecureCoreServerListGenerator : ServerListGeneratorBase, ISmartSecureCoreServerListGenerator
{
    private const int MAX_GENERATED_INTENT_LOGICAL_SERVERS = 3;

    protected override int MaxPhysicalServersPerLogical => 1;

    private readonly ISettings _settings;
    private readonly IServersLoader _serversLoader;
    private readonly IIntentServerListGenerator _intentServerListGenerator;

    public SmartSecureCoreServerListGenerator(ISettings settings,
        IServersLoader serversLoader,
        IIntentServerListGenerator intentServerListGenerator)
    {
        _settings = settings;
        _serversLoader = serversLoader;
        _intentServerListGenerator = intentServerListGenerator;
    }

    public IEnumerable<PhysicalServer> Generate(SecureCoreFeatureIntent secureCoreFeatureIntent,
        CountryLocationIntent countryLocationIntent,
        IList<VpnProtocol> preferredProtocols)
    {
        List<Server> pickedServers = GenerateIntentServers(secureCoreFeatureIntent, countryLocationIntent, preferredProtocols).ToList();
        if (pickedServers.Count == 0)
        {
            return _intentServerListGenerator.Generate(new ConnectionIntent(countryLocationIntent, secureCoreFeatureIntent), preferredProtocols);
        }

        string? entryCountry = GetEntryCountry(pickedServers, secureCoreFeatureIntent);
        string? exitCountry = GetExitCountry(pickedServers, countryLocationIntent);
        string? excludedCountry = countryLocationIntent.IsToExcludeMyCountry
           ? _settings.DeviceLocation?.CountryCode
           : null;
        IEnumerable<Server> unfilteredServers = GetSecureCoreServers(countryLocationIntent.Kind, preferredProtocols);

        if (exitCountry is not null)
        {
            AddServerIfNotAlreadyListed(pickedServers, unfilteredServers,
                s => IsSameExitDifferentEntry(s, exitCountry: exitCountry, entryCountry: entryCountry, excludedCountry: excludedCountry));
        }
        if (entryCountry is not null)
        {
            AddServerIfNotAlreadyListed(pickedServers, unfilteredServers,
                s => IsDifferentExitSameEntry(s, exitCountry: exitCountry, entryCountry: entryCountry, excludedCountry: excludedCountry));
        }
        AddServerIfNotAlreadyListed(pickedServers, unfilteredServers,
            s => IsDifferentExitAndEntry(s, exitCountry: exitCountry, entryCountry: entryCountry, excludedCountry: excludedCountry));

        return SelectDistinctPhysicalServers(pickedServers, preferredProtocols);
    }

    private IEnumerable<Server> GenerateIntentServers(SecureCoreFeatureIntent secureCoreFeatureIntent,
        CountryLocationIntent countryLocationIntent,
        IList<VpnProtocol> preferredProtocols)
    {
        IEnumerable<Server> servers = _serversLoader.GetServers();
        servers = countryLocationIntent.FilterServers(servers, _settings.DeviceLocation);
        servers = secureCoreFeatureIntent.FilterServers(servers);

        return SortServers(countryLocationIntent.Kind, servers)
            .Where(s => s.IsAvailable(preferredProtocols))
            .Take(MAX_GENERATED_INTENT_LOGICAL_SERVERS);
    }

    private IEnumerable<Server> SortServers(ConnectionIntentKind kind, IEnumerable<Server> source)
    {
        return kind == ConnectionIntentKind.Random
            ? source.OrderBy(_ => Random.Next())
            : source.OrderBy(s => s.Score);
    }

    private IEnumerable<Server> GetSecureCoreServers(ConnectionIntentKind kind, IList<VpnProtocol> preferredProtocols)
    {
        return SortServers(kind, _serversLoader.GetServers().Where(s => s.IsAvailable(preferredProtocols) && s.Features.IsSupported(ServerFeatures.SecureCore)));
    }

    private bool IsSameExitDifferentEntry(Server server, string exitCountry, string? entryCountry, string? excludedCountry)
    {
        return server.ExitCountry == exitCountry && server.ExitCountry != excludedCountry
            && server.EntryCountry != entryCountry && server.EntryCountry != excludedCountry;
    }

    private bool IsDifferentExitSameEntry(Server server, string? exitCountry, string entryCountry, string? excludedCountry)
    {
        return server.ExitCountry != exitCountry && server.ExitCountry != excludedCountry
            && server.EntryCountry == entryCountry && server.EntryCountry != excludedCountry;
    }

    private bool IsDifferentExitAndEntry(Server server, string? exitCountry, string? entryCountry, string? excludedCountry)
    {
        return server.ExitCountry != exitCountry && server.ExitCountry != excludedCountry
            && server.EntryCountry != entryCountry && server.EntryCountry != excludedCountry;
    }
}