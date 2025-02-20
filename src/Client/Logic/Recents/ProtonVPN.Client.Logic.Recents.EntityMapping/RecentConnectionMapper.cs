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
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Recents.Contracts.SerializableEntities;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Recents.EntityMapping;

public class RecentConnectionMapper : IMapper<IRecentConnection, SerializableRecentConnection>
{
    private readonly IEntityMapper _entityMapper;
    private readonly ILogger _logger;
    private readonly IProfilesManager _profilesManager;

    public RecentConnectionMapper(IEntityMapper entityMapper, ILogger logger, IProfilesManager profilesManager)
    {
        _entityMapper = entityMapper;
        _logger = logger;
        _profilesManager = profilesManager;
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

        return new SerializableRecentConnection
        {
            RecentId = leftEntity.Id,
            ConnectionIntent = connectionIntent,
            ProfileId = leftEntity.ConnectionIntent is IConnectionProfile cnp ? cnp.Id : null,
            IsPinned = leftEntity.IsPinned,
            PinTime = leftEntity.PinTime,
        };
    }

    public IRecentConnection Map(SerializableRecentConnection rightEntity)
    {
        try
        {
            if (rightEntity is null)
            {
                return null;
            }

            IConnectionIntent connectionIntent = rightEntity.ProfileId.HasValue
                ? _profilesManager.GetById(rightEntity.ProfileId.Value)
                : _entityMapper.Map<SerializableConnectionIntent, IConnectionIntent>(rightEntity.ConnectionIntent);

            return connectionIntent is null
                ? null
                : new RecentConnection(rightEntity.RecentId ?? Guid.NewGuid(), connectionIntent)
                {
                    IsPinned = rightEntity.IsPinned,
                    PinTime = rightEntity.PinTime,
                };
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>($"Failed to map recent connection.", ex);
        }
        return null;
    }
}