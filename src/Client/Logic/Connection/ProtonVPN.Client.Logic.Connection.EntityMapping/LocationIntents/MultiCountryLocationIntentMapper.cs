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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;
using ProtonVPN.Client.Logic.Connection.EntityMapping.Extensions;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Connection.EntityMapping.LocationIntents;

public class MultiCountryLocationIntentMapper : IMapper<MultiCountryLocationIntent, SerializableLocationIntent>
{
    public SerializableLocationIntent Map(MultiCountryLocationIntent leftEntity)
    {
        return leftEntity is null
            ? null
            : new SerializableLocationIntent()
            {
                TypeName = nameof(MultiCountryLocationIntent),
                Strategy = leftEntity.Strategy,
                CountryCodes = leftEntity.CountryCodes.ToList(),
                IsToExcludeMyCountry = leftEntity.IsToExcludeMyCountry,
            };
    }

    public MultiCountryLocationIntent Map(SerializableLocationIntent rightEntity)
    {
        bool isToExcludeMyCountry = rightEntity.IsToExcludeMyCountry ?? false;
        SelectionStrategy strategy = rightEntity.GetSelectionStrategy();

        return rightEntity is null
            ? null
            : isToExcludeMyCountry
                ? MultiCountryLocationIntent.ExcludingMyCountry(
                    strategy: strategy)
                : MultiCountryLocationIntent.From(
                    countryCodes: rightEntity.CountryCodes ?? [],
                    strategy: strategy);
    }
}