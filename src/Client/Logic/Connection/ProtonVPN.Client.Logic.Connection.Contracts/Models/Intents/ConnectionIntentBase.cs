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
using ProtonVPN.Common.Core.Geographical;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;

public abstract class ConnectionIntentBase : IntentBase, IConnectionIntent
{
    public ILocationIntent Location { get; protected set; }

    public IFeatureIntent? Feature { get; protected set; }

    public override bool IsForPaidUsersOnly => Location.IsForPaidUsersOnly || (Feature != null && Feature.IsForPaidUsersOnly);

    protected ConnectionIntentBase(ILocationIntent location, IFeatureIntent? feature = null)
    {
        Location = location;
        Feature = feature;
    }

    public abstract bool IsSameAs(IConnectionIntent? intent);

    public bool HasNoServers(IEnumerable<Server> servers, DeviceLocation? deviceLocation)
    {
        return !servers.Any(s => IsSupported(s, deviceLocation));
    }

    public bool AreAllServersUnderMaintenance(IEnumerable<Server> servers, DeviceLocation? deviceLocation)
    {
        return servers.Where(s => IsSupported(s, deviceLocation)).All(server => server.IsUnderMaintenance());
    }

    public override string ToString()
    {
        return $"{Location}{(Feature is null ? string.Empty : $" [{Feature}]")}";
    }

    public bool IsSupported(Server server, DeviceLocation? deviceLocation)
    {
        return Location.IsSupported(server, deviceLocation) && (Feature == null || Feature.IsSupported(server));
    }

    public bool IsPortForwardingSupported()
    {
        return Feature is not SecureCoreFeatureIntent and not TorFeatureIntent;
    }
}