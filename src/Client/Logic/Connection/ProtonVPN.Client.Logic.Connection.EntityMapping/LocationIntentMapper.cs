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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Connection.EntityMapping;

public class LocationIntentMapper : IMapper<ILocationIntent, SerializableLocationIntent>
{
    private readonly IEntityMapper _entityMapper;

    public LocationIntentMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public SerializableLocationIntent Map(ILocationIntent leftEntity)
    {
        return leftEntity is null
            ? null
            : leftEntity switch // Case order is important because some types are base classes of the other types
            {
                ServerLocationIntent serverLocationIntent =>
                    _entityMapper.Map<ServerLocationIntent, SerializableLocationIntent>(serverLocationIntent),
                CityLocationIntent cityLocationIntent =>
                    _entityMapper.Map<CityLocationIntent, SerializableLocationIntent>(cityLocationIntent),
                StateLocationIntent stateLocationIntent =>
                    _entityMapper.Map<StateLocationIntent, SerializableLocationIntent>(stateLocationIntent),
                CountryLocationIntent countryLocationIntent =>
                    _entityMapper.Map<CountryLocationIntent, SerializableLocationIntent>(countryLocationIntent),

                GatewayServerLocationIntent gatewayServerLocationIntent =>
                    _entityMapper.Map<GatewayServerLocationIntent, SerializableLocationIntent>(gatewayServerLocationIntent),
                GatewayLocationIntent gatewayLocationIntent =>
                    _entityMapper.Map<GatewayLocationIntent, SerializableLocationIntent>(gatewayLocationIntent),

                FreeServerLocationIntent freeServerLocationIntent =>
                    _entityMapper.Map<FreeServerLocationIntent, SerializableLocationIntent>(freeServerLocationIntent),

                _ => throw new NotImplementedException($"No mapping is implemented for {leftEntity.GetType().FullName}"),
            };
    }

    public ILocationIntent Map(SerializableLocationIntent rightEntity)
    {
        return rightEntity is null
            ? null
            : rightEntity.TypeName switch
            {
                nameof(ServerLocationIntent) => _entityMapper.Map<SerializableLocationIntent, ServerLocationIntent>(rightEntity),
                nameof(CityLocationIntent) => _entityMapper.Map<SerializableLocationIntent, CityLocationIntent>(rightEntity),
                nameof(StateLocationIntent) => _entityMapper.Map<SerializableLocationIntent, StateLocationIntent>(rightEntity),
                nameof(CountryLocationIntent) => _entityMapper.Map<SerializableLocationIntent, CountryLocationIntent>(rightEntity),
                nameof(GatewayServerLocationIntent) => _entityMapper.Map<SerializableLocationIntent, GatewayServerLocationIntent>(rightEntity),
                nameof(GatewayLocationIntent) => _entityMapper.Map<SerializableLocationIntent, GatewayLocationIntent>(rightEntity),
                nameof(FreeServerLocationIntent) => _entityMapper.Map<SerializableLocationIntent, FreeServerLocationIntent>(rightEntity),
                _ => throw new NotImplementedException($"No mapping is implemented for {rightEntity.TypeName}"),
            };
    }
}