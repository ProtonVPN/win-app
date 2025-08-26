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
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Common.Core.Geographical;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;

public abstract class LocationIntentBase : IntentBase, ILocationIntent
{
    public ConnectionIntentKind Kind { get; }

    protected LocationIntentBase(ConnectionIntentKind kind = ConnectionIntentKind.Fastest)
    {
        Kind = kind;
    }

    public virtual bool IsSameAs(ILocationIntent? intent)
    {
        if (intent == null)
        {
            return false;
        }

        return GetType() == intent.GetType();
    }

    public IEnumerable<Server> FilterServers(IEnumerable<Server> servers, DeviceLocation? deviceLocation)
    {
        return servers.Where(s => IsSupported(s, deviceLocation));
    }

    public abstract bool IsSupported(Server server, DeviceLocation? deviceLocation);

    public abstract bool IsGenericRandomIntent();

    public ILocationIntent Copy()
    {
        return (ILocationIntent)MemberwiseClone();
    }
}