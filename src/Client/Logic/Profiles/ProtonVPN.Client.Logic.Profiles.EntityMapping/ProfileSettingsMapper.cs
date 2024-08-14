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

using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Profiles.Contracts.SerializableEntities;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Profiles.EntityMapping;

public class ProfileSettingsMapper : IMapper<IProfileSettings, SerializableProfileSettings>
{
    private readonly IEntityMapper _entityMapper;

    public ProfileSettingsMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public SerializableProfileSettings Map(IProfileSettings leftEntity)
    {
        return leftEntity is null
            ? null
            : new SerializableProfileSettings()
            {
                Protocol = (int)leftEntity.Protocol,
            };
    }

    public IProfileSettings Map(SerializableProfileSettings rightEntity)
    {
        return rightEntity is null
            ? null
            : new ProfileSettings()
            {
                Protocol = (VpnProtocol)rightEntity.Protocol,
            };
    }
}