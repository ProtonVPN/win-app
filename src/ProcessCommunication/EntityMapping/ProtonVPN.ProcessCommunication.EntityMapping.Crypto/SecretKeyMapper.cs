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

using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Crypto;

public class SecretKeyMapper : KeyMapperBase<SecretKeyIpcEntity>, IMapper<SecretKey, SecretKeyIpcEntity>
{
    public SecretKeyIpcEntity Map(SecretKey leftEntity)
    {
        return leftEntity is null
            ? null
            : MapKeyToIpcEntity(leftEntity);
    }

    public SecretKey Map(SecretKeyIpcEntity rightEntity)
    {
        return rightEntity is null
            ? null
            : rightEntity.Base64 is null
                ? new(rightEntity.Bytes, (KeyAlgorithm)rightEntity.Algorithm)
                : new(rightEntity.Base64, (KeyAlgorithm)rightEntity.Algorithm);
    }
}
