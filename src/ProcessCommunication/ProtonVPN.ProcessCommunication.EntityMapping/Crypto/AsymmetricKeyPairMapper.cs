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

public class AsymmetricKeyPairMapper : IMapper<AsymmetricKeyPair, AsymmetricKeyPairIpcEntity>
{
    private readonly IEntityMapper _entityMapper;

    public AsymmetricKeyPairMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public AsymmetricKeyPairIpcEntity Map(AsymmetricKeyPair leftEntity)
    {
        if (leftEntity is null || leftEntity.SecretKey is null || leftEntity.PublicKey is null)
        {
            return null;
        }
        return new AsymmetricKeyPairIpcEntity()
        {
            SecretKey = _entityMapper.Map<SecretKey, SecretKeyIpcEntity>(leftEntity.SecretKey),
            PublicKey = _entityMapper.Map<PublicKey, PublicKeyIpcEntity>(leftEntity.PublicKey),
        };
    }

    public AsymmetricKeyPair Map(AsymmetricKeyPairIpcEntity rightEntity)
    {
        if (rightEntity is null || rightEntity.SecretKey is null || rightEntity.PublicKey is null)
        {
            return null;
        }
        return new AsymmetricKeyPair(
            _entityMapper.Map<SecretKeyIpcEntity, SecretKey>(rightEntity.SecretKey),
            _entityMapper.Map<PublicKeyIpcEntity, PublicKey>(rightEntity.PublicKey));
    }
}
