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

public class UpdateStateMapper : IMapper<AppUpdateStateContract, UpdateStateIpcEntity>
{
    private readonly IEntityMapper _entityMapper;

    public UpdateStateMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public UpdateStateIpcEntity Map(AppUpdateStateContract leftEntity)
    {
        return new UpdateStateIpcEntity
        {
            IsAvailable = leftEntity.IsAvailable,
            FileArguments = leftEntity.FileArguments,
            FilePath = leftEntity.FilePath,
            Version = leftEntity.Version.ToString(),
            IsReady = leftEntity.IsReady,
            ReleaseHistory = _entityMapper.Map<ReleaseContract, ReleaseIpcEntity>(leftEntity.ReleaseHistory).ToArray(),
            Status = (UpdateStatusIpcEntity)leftEntity.Status,
        };
    }

    public AppUpdateStateContract Map(UpdateStateIpcEntity rightEntity)
    {
        return new AppUpdateStateContract
        {
            IsAvailable = rightEntity.IsAvailable,
            FileArguments = rightEntity.FileArguments,
            FilePath = rightEntity.FilePath,
            Version = new Version(rightEntity.Version),
            IsReady = rightEntity.IsReady,
            Status = (AppUpdateStatus)rightEntity.Status,
            ReleaseHistory = _entityMapper.Map<ReleaseIpcEntity, ReleaseContract>(rightEntity.ReleaseHistory)
        };
    }
}