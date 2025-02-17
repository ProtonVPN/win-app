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

using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Profiles.Contracts.SerializableEntities;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Profiles.EntityMapping;

public class ConnectionProfileMapper : IMapper<IConnectionProfile, SerializableProfile>
{
    private readonly IEntityMapper _entityMapper;

    public ConnectionProfileMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public SerializableProfile Map(IConnectionProfile leftEntity)
    {
        return leftEntity is null
            ? null
            : new SerializableProfile()
            {
                Id = leftEntity.Id,
                Name = leftEntity.Name,
                CreationDateTimeUtc = leftEntity.CreationDateTimeUtc,
                UpdateDateTimeUtc = leftEntity.UpdateDateTimeUtc,
                Location = _entityMapper.Map<ILocationIntent, SerializableLocationIntent>(leftEntity.Location),
                Feature = _entityMapper.Map<IFeatureIntent, SerializableFeatureIntent>(leftEntity.Feature),
                Icon = _entityMapper.Map<IProfileIcon, SerializableProfileIcon>(leftEntity.Icon),
                Settings = _entityMapper.Map<IProfileSettings, SerializableProfileSettings>(leftEntity.Settings),
                Options = _entityMapper.Map<IProfileOptions, SerializableProfileOptions>(leftEntity.Options),
            };
    }

    public IConnectionProfile Map(SerializableProfile rightEntity)
    {
        return rightEntity is null
            ? null
            : new ConnectionProfile(
                id: rightEntity.Id,
                creationDateTimeUtc: rightEntity.CreationDateTimeUtc,
                icon: _entityMapper.Map<SerializableProfileIcon, IProfileIcon>(rightEntity.Icon),
                settings: _entityMapper.Map<SerializableProfileSettings, IProfileSettings>(rightEntity.Settings),
                options: _entityMapper.Map<SerializableProfileOptions, IProfileOptions>(rightEntity.Options),
                location: _entityMapper.Map<SerializableLocationIntent, ILocationIntent>(rightEntity.Location),
                feature: _entityMapper.Map<SerializableFeatureIntent, IFeatureIntent>(rightEntity.Feature))
            {
                Name = rightEntity.Name,
                UpdateDateTimeUtc = rightEntity.UpdateDateTimeUtc
            };
    }
}