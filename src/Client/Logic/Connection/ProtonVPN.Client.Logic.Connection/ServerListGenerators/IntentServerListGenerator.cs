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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.ServerListGenerators;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Logic.Connection.ServerListGenerators;

public class IntentServerListGenerator : ServerListGeneratorBase, IIntentServerListGenerator
{
    private const int MAX_GENERATED_PHYSICAL_SERVERS = 64;

    protected override int MaxPhysicalServersPerLogical => 2;

    private readonly ISettings _settings;
    private readonly IServersLoader _serversLoader;

    public IntentServerListGenerator(ISettings settings,
        IServersLoader serversLoader)
    {
        _settings = settings;
        _serversLoader = serversLoader;
    }

    public IEnumerable<PhysicalServer> Generate(IConnectionIntent connectionIntent, IList<VpnProtocol> preferredProtocols)
    {
        IEnumerable<Server> servers = _serversLoader.GetServers().Where(s => s.IsAvailable(preferredProtocols));
        ILocationIntent locationIntent = connectionIntent.Location;
        IFeatureIntent? featureIntent = connectionIntent.Feature;

        bool isPortForwardingEnabled = connectionIntent is IConnectionProfile profile
            ? profile.Settings.IsPortForwardingEnabled
            : _settings.IsPortForwardingEnabled;

        servers = locationIntent.FilterServers(servers, _settings.DeviceLocation);

        if (locationIntent is FreeServerLocationIntent fsli && fsli.Kind == ConnectionIntentKind.Random)
        {
            servers = servers.Where(s => s.Id != fsli.ExcludedLogicalServerId);
        }

        servers = featureIntent is null
            ? servers.Where(s => s.IsStandard())
            : featureIntent.FilterServers(servers);

        return SortServers(servers, locationIntent, isPortForwardingEnabled)
            .SelectMany(s => SelectPhysicalServers(s, preferredProtocols))
            .DistinctBy(s => new { s.EntryIp, s.Label })
            .Take(MAX_GENERATED_PHYSICAL_SERVERS);
    }

    private IEnumerable<Server> SortServers(IEnumerable<Server> source, ILocationIntent locationIntent, bool isPortForwardingEnabled)
    {
        return locationIntent.Kind == ConnectionIntentKind.Random
            ? source.OrderBy(_ => Random.Next())
            : isPortForwardingEnabled
                ? source.OrderByDescending(s => s.Features.IsSupported(ServerFeatures.P2P)).ThenBy(s => s.Score)
                : source.OrderBy(s => s.Score);
    }
}