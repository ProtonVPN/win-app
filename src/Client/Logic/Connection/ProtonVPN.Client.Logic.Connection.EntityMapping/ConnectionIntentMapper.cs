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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Connection.EntityMapping;

public class ConnectionIntentMapper : IMapper<IConnectionIntent, SerializableConnectionIntent>
{
    private readonly IEntityMapper _entityMapper;
    private readonly ILogger _logger;

    public ConnectionIntentMapper(
        IEntityMapper entityMapper,
        ILogger logger)
    {
        _entityMapper = entityMapper;
        _logger = logger;
    }

    public SerializableConnectionIntent Map(IConnectionIntent leftEntity)
    {
        return leftEntity is null
            ? null
            : new SerializableConnectionIntent()
              {
                  Location = _entityMapper.Map<ILocationIntent, SerializableLocationIntent>(leftEntity.Location),
                  Feature = _entityMapper.Map<IFeatureIntent, SerializableFeatureIntent>(leftEntity.Feature),
              };
    }

    public IConnectionIntent Map(SerializableConnectionIntent rightEntity)
    {
        try
        {
            return rightEntity is null
                ? null
                : new ConnectionIntent(
                    _entityMapper.Map<SerializableLocationIntent, ILocationIntent>(rightEntity.Location),
                    _entityMapper.Map<SerializableFeatureIntent, IFeatureIntent>(rightEntity.Feature));
        }
        catch (Exception e)
        {
            _logger.Warn<AppLog>("Failed to map SerializableConnectionIntent to IConnectionIntent", e);

            return null;
        }
    }
}