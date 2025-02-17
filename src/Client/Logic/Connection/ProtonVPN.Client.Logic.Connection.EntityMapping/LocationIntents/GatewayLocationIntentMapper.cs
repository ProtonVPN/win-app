
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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Connection.EntityMapping.LocationIntents;

public class GatewayLocationIntentMapper : IMapper<GatewayLocationIntent, SerializableLocationIntent>
{
    public SerializableLocationIntent Map(GatewayLocationIntent leftEntity)
    {
        return leftEntity is null
            ? null
            : new SerializableLocationIntent()
            {
                TypeName = nameof(GatewayLocationIntent),
                GatewayName = leftEntity.GatewayName,
                Kind = leftEntity.Kind.ToString(),
            };
    }

    public GatewayLocationIntent Map(SerializableLocationIntent rightEntity)
    {
        if (rightEntity is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(rightEntity.Kind) || !Enum.TryParse(rightEntity.Kind, out ConnectionIntentKind kind))
        {
            kind = ConnectionIntentKind.Fastest;
        }

        return new GatewayLocationIntent(rightEntity.GatewayName, kind);
    }
}