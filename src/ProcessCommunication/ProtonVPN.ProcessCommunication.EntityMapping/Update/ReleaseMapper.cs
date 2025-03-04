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

using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Update;
using ProtonVPN.Update.Contracts;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Update;

public class ReleaseMapper : IMapper<ReleaseContract, ReleaseIpcEntity>
{
    public ReleaseIpcEntity Map(ReleaseContract leftEntity)
    {
        return new ReleaseIpcEntity
        {
            ChangeLog = leftEntity.ChangeLog.ToArray(),
            EarlyAccess = leftEntity.IsEarlyAccess,
            New = leftEntity.IsNew,
            ReleaseDate = leftEntity.ReleaseDate,
            Version = leftEntity.Version.ToString(),
        };
    }

    public ReleaseContract Map(ReleaseIpcEntity rightEntity)
    {
        bool isVersionValid = Version.TryParse(rightEntity.Version, out Version version);

        return new ReleaseContract
        {
            ChangeLog = rightEntity.ChangeLog,
            IsEarlyAccess = rightEntity.EarlyAccess,
            IsNew = rightEntity.New,
            ReleaseDate = rightEntity.ReleaseDate,
            Version = isVersionValid ? version : new Version(),
        };
    }
}