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
using ProtonVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.SerializableEntities;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Recents.EntityMapping;

public class RecentConnectionMapper : IMapper<IRecentConnection, SerializableRecentConnection>
{
    private readonly IEntityMapper _entityMapper;

    public RecentConnectionMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public SerializableRecentConnection Map(IRecentConnection leftEntity)
    {
        if (leftEntity is null)
        {
            return null;
        }

        SerializableConnectionIntent connectionIntent =
            _entityMapper.Map<IConnectionIntent, SerializableConnectionIntent>(leftEntity.ConnectionIntent);

        if (connectionIntent is null)
        {
            return null;
        }

        return new SerializableRecentConnection()
        {
            ConnectionIntent = connectionIntent,
            IsPinned = leftEntity.IsPinned,
            PinTime = leftEntity.PinTime,
        };
    }

    public IRecentConnection Map(SerializableRecentConnection rightEntity)
    {
        if (rightEntity is null)
        {
            return null;
        }

        IConnectionIntent connectionIntent =
            _entityMapper.Map<SerializableConnectionIntent, IConnectionIntent>(rightEntity.ConnectionIntent);

        if (connectionIntent is null)
        {
            return null;
        }

        return new RecentConnection(connectionIntent)
        {
            IsPinned = rightEntity.IsPinned,
            PinTime = rightEntity.PinTime,
        };
    }
}