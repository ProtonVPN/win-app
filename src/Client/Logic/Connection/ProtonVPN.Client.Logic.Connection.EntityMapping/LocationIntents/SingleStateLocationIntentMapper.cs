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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.States;
using ProtonVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Connection.EntityMapping.LocationIntents;

public class SingleStateLocationIntentMapper : IMapper<SingleStateLocationIntent, SerializableLocationIntent>
{
    public SerializableLocationIntent Map(SingleStateLocationIntent leftEntity)
    {
        return leftEntity is null
            ? null
            : new SerializableLocationIntent()
            {
                TypeName = nameof(SingleStateLocationIntent),
                CountryCode = leftEntity.Country.CountryCode,
                State = leftEntity.StateName,
            };
    }

    public SingleStateLocationIntent Map(SerializableLocationIntent rightEntity)
    {
        return rightEntity is null
            ? null
            : SingleStateLocationIntent.From(
                countryCode: rightEntity.CountryCode,
                stateName: rightEntity.State);
    }
}