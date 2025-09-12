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
using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.FreeServers;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;

public abstract class ConnectionIntentBase : IntentBase, IConnectionIntent
{
    public ILocationIntent Location { get; protected set; }

    public IFeatureIntent? Feature { get; protected set; }

    public override bool IsForPaidUsersOnly => Location.IsForPaidUsersOnly 
                                            || (Feature?.IsForPaidUsersOnly ?? false);

    protected ConnectionIntentBase(ILocationIntent location, IFeatureIntent? feature = null)
    {
        Location = location;
        Feature = feature;
    }

    public abstract bool IsSameAs(IConnectionIntent? intent);

    public IOrderedEnumerable<Server> FilterAndSortServers(IEnumerable<Server> servers, DeviceLocation? deviceLocation, IList<VpnProtocol> preferredProtocols, bool isPortForwardingEnabled)
    {
        IEnumerable<Server> supportedServers = servers
            .Where(s => s.IsAvailable(preferredProtocols) 
                     && IsSupported(s, deviceLocation));

        bool isB2BIntent = Feature is B2BFeatureIntent;
        bool isFreeIntent = Location is FreeServerLocationIntent;

        // If port forwarding is enabled, prioritize servers that support P2P
        // If Free intent, prioritize free servers, otherwise prioritize paid servers
        // If B2B intent, prioritize B2B servers, otherwise prioritize non-B2B servers
        Func<Server, object> firstSortFunction = 
            s => (!isPortForwardingEnabled || s.Features.IsSupported(ServerFeatures.P2P))
              && (isFreeIntent ? s.IsFree() : s.IsPaid())
              && (isB2BIntent ? s.IsB2B() : s.IsNonB2B()); 

        // Then sort servers based on the selection strategy (fastest, random...)
        Func<Server, object> secondSortFunction = Location.GetSelectionStrategy() switch
        { 
            SelectionStrategy.Random => _ => Random.Shared.Next(),
            _ => s => s.Score,
        };

        return supportedServers
            .OrderByDescending(firstSortFunction)
            .ThenBy(secondSortFunction);
    }

    public bool HasNoServers(IEnumerable<Server> servers, DeviceLocation? deviceLocation)
    {
        return !servers.Any(s => IsSupported(s, deviceLocation));
    }

    public bool AreAllServersUnderMaintenance(IEnumerable<Server> servers, DeviceLocation? deviceLocation)
    {
        return !servers.Any(s => IsSupported(s, deviceLocation) && !s.IsUnderMaintenance());
    }

    public bool IsSupported(Server server, DeviceLocation? deviceLocation)
    {
        if (deviceLocation.HasValue &&
            Location is MultiCountryLocationIntent multiCountryIntent && 
            multiCountryIntent.IsToExcludeMyCountry && 
            server.ExitCountry == deviceLocation.Value.CountryCode)
        {
            return false;
        }

        return Location.IsSupported(server) 
            && (Feature?.IsSupported(server) ?? server.IsStandard());
    }

    public bool IsPortForwardingSupported()
    {
        return Feature is not SecureCoreFeatureIntent and not TorFeatureIntent;
    }

    public override string ToString()
    {
        return $"{Location}{(Feature is null ? string.Empty : $" [{Feature}]")}";
    }
}