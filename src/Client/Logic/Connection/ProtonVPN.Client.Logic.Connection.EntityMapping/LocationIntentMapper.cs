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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Cities;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.FreeServers;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Gateways;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.GatewayServers;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Servers;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.States;
using ProtonVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Connection.EntityMapping;

public class LocationIntentMapper : IMapper<ILocationIntent, SerializableLocationIntent>
{
    private readonly IEntityMapper _entityMapper;

    private const string LEGACY_SERVER_LOCATION_INTENT = "ServerLocationIntent";
    private const string LEGACY_CITY_LOCATION_INTENT = "CityLocationIntent";
    private const string LEGACY_STATE_LOCATION_INTENT = "StateLocationIntent";
    private const string LEGACY_COUNTRY_LOCATION_INTENT = "CountryLocationIntent";
    private const string LEGACY_GATEWAY_SERVER_LOCATION_INTENT = "GatewayServerLocationIntent";
    private const string LEGACY_GATEWAY_LOCATION_INTENT = "GatewayLocationIntent";


    public LocationIntentMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public SerializableLocationIntent Map(ILocationIntent leftEntity)
    {
        return leftEntity is null
            ? null
            : leftEntity switch
            {
                SingleServerLocationIntent intent => _entityMapper.Map<SingleServerLocationIntent, SerializableLocationIntent>(intent),
                MultiServerLocationIntent intent => _entityMapper.Map<MultiServerLocationIntent, SerializableLocationIntent>(intent),
                SingleCityLocationIntent intent => _entityMapper.Map<SingleCityLocationIntent, SerializableLocationIntent>(intent),
                MultiCityLocationIntent intent => _entityMapper.Map<MultiCityLocationIntent, SerializableLocationIntent>(intent),
                SingleStateLocationIntent intent => _entityMapper.Map<SingleStateLocationIntent, SerializableLocationIntent>(intent),
                MultiStateLocationIntent intent => _entityMapper.Map<MultiStateLocationIntent, SerializableLocationIntent>(intent),
                SingleCountryLocationIntent intent => _entityMapper.Map<SingleCountryLocationIntent, SerializableLocationIntent>(intent),
                MultiCountryLocationIntent intent => _entityMapper.Map<MultiCountryLocationIntent, SerializableLocationIntent>(intent),
                SingleGatewayServerLocationIntent intent => _entityMapper.Map<SingleGatewayServerLocationIntent, SerializableLocationIntent>(intent),
                MultiGatewayServerLocationIntent intent => _entityMapper.Map<MultiGatewayServerLocationIntent, SerializableLocationIntent>(intent),
                SingleGatewayLocationIntent intent => _entityMapper.Map<SingleGatewayLocationIntent, SerializableLocationIntent>(intent),
                MultiGatewayLocationIntent intent => _entityMapper.Map<MultiGatewayLocationIntent, SerializableLocationIntent>(intent),
                FreeServerLocationIntent intent => _entityMapper.Map<FreeServerLocationIntent, SerializableLocationIntent>(intent),

                _ => throw new NotImplementedException($"No mapping is implemented for {leftEntity.GetType().FullName}"),
            };
    }

    public ILocationIntent Map(SerializableLocationIntent rightEntity)
    {
        return rightEntity is null
            ? null
            : rightEntity.TypeName switch
            {
                // Legacy types mapping
                LEGACY_SERVER_LOCATION_INTENT => string.IsNullOrEmpty(rightEntity.Id)
                    ? _entityMapper.Map<SerializableLocationIntent, MultiServerLocationIntent>(rightEntity)
                    : _entityMapper.Map<SerializableLocationIntent, SingleServerLocationIntent>(rightEntity),
                LEGACY_CITY_LOCATION_INTENT => string.IsNullOrEmpty(rightEntity.City)
                    ? _entityMapper.Map<SerializableLocationIntent, MultiCityLocationIntent>(rightEntity)
                    : _entityMapper.Map<SerializableLocationIntent, SingleCityLocationIntent>(rightEntity),
                LEGACY_STATE_LOCATION_INTENT => string.IsNullOrEmpty(rightEntity.State)
                    ? _entityMapper.Map<SerializableLocationIntent, MultiStateLocationIntent>(rightEntity)
                    : _entityMapper.Map<SerializableLocationIntent, SingleStateLocationIntent>(rightEntity),
                LEGACY_COUNTRY_LOCATION_INTENT => string.IsNullOrEmpty(rightEntity.CountryCode)
                    ? _entityMapper.Map<SerializableLocationIntent, MultiCountryLocationIntent>(rightEntity)
                    : _entityMapper.Map<SerializableLocationIntent, SingleCountryLocationIntent>(rightEntity),
                LEGACY_GATEWAY_SERVER_LOCATION_INTENT => string.IsNullOrEmpty(rightEntity.Id)
                    ? _entityMapper.Map<SerializableLocationIntent, MultiGatewayServerLocationIntent>(rightEntity)
                    : _entityMapper.Map<SerializableLocationIntent, SingleGatewayServerLocationIntent>(rightEntity),
                LEGACY_GATEWAY_LOCATION_INTENT => string.IsNullOrEmpty(rightEntity.GatewayName)
                    ? _entityMapper.Map<SerializableLocationIntent, MultiGatewayLocationIntent>(rightEntity)
                    : _entityMapper.Map<SerializableLocationIntent, SingleGatewayLocationIntent>(rightEntity),

                nameof(SingleServerLocationIntent) => _entityMapper.Map<SerializableLocationIntent, SingleServerLocationIntent>(rightEntity),
                nameof(MultiServerLocationIntent) => _entityMapper.Map<SerializableLocationIntent, MultiServerLocationIntent>(rightEntity),
                nameof(SingleCityLocationIntent) => _entityMapper.Map<SerializableLocationIntent, SingleCityLocationIntent>(rightEntity),
                nameof(MultiCityLocationIntent) => _entityMapper.Map<SerializableLocationIntent, MultiCityLocationIntent>(rightEntity),
                nameof(SingleStateLocationIntent) => _entityMapper.Map<SerializableLocationIntent, SingleStateLocationIntent>(rightEntity),
                nameof(MultiStateLocationIntent) => _entityMapper.Map<SerializableLocationIntent, MultiStateLocationIntent>(rightEntity),
                nameof(SingleCountryLocationIntent) => _entityMapper.Map<SerializableLocationIntent, SingleCountryLocationIntent>(rightEntity),
                nameof(MultiCountryLocationIntent) => _entityMapper.Map<SerializableLocationIntent, MultiCountryLocationIntent>(rightEntity),
                nameof(SingleGatewayServerLocationIntent) => _entityMapper.Map<SerializableLocationIntent, SingleGatewayServerLocationIntent>(rightEntity),
                nameof(MultiGatewayServerLocationIntent) => _entityMapper.Map<SerializableLocationIntent, MultiGatewayServerLocationIntent>(rightEntity),
                nameof(SingleGatewayLocationIntent) => _entityMapper.Map<SerializableLocationIntent, SingleGatewayLocationIntent>(rightEntity),
                nameof(MultiGatewayLocationIntent) => _entityMapper.Map<SerializableLocationIntent, MultiGatewayLocationIntent>(rightEntity),
                nameof(FreeServerLocationIntent) => _entityMapper.Map<SerializableLocationIntent, FreeServerLocationIntent>(rightEntity),

                _ => throw new NotImplementedException($"No mapping is implemented for {rightEntity.TypeName}"),
            };
    }
}