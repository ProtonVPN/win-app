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

using ProtonVPN.Common.Vpn;
using ProtonVPN.Crypto;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Auth;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

public class VpnCredentialsMapper : IMapper<VpnCredentials, VpnCredentialsIpcEntity>
{
    private readonly IEntityMapper _entityMapper;

    public VpnCredentialsMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public VpnCredentialsIpcEntity Map(VpnCredentials leftEntity)
    {
        return new()
        {
            Certificate = CreateConnectionCertificateIpcEntity(leftEntity),
            ClientKeyPair = _entityMapper.Map<AsymmetricKeyPair, AsymmetricKeyPairIpcEntity>(leftEntity.ClientKeyPair)
        };
    }

    private ConnectionCertificateIpcEntity CreateConnectionCertificateIpcEntity(VpnCredentials leftEntity)
    {
        return string.IsNullOrWhiteSpace(leftEntity.ClientCertPem)
            ? null
            : new()
        {
            Pem = leftEntity.ClientCertPem ?? string.Empty,
            ExpirationDateUtc = DateTime.MinValue,
        };
    }

    public VpnCredentials Map(VpnCredentialsIpcEntity rightEntity)
    {
        return IsCertificateCredential(rightEntity)
            ? CreateCertificateVpnCredentials(rightEntity)
            : throw new ArgumentNullException($"The {nameof(VpnCredentialsIpcEntity)} to be mapped is null.");
    }

    private bool IsCertificateCredential(VpnCredentialsIpcEntity rightEntity)
    {
        return rightEntity is not null &&
               rightEntity.ClientKeyPair is not null &&
               rightEntity.Certificate is not null &&
               !string.IsNullOrWhiteSpace(rightEntity.Certificate.Pem);
    }

    private VpnCredentials CreateCertificateVpnCredentials(VpnCredentialsIpcEntity rightEntity)
    {
        return new(rightEntity.Certificate.Pem,
                   _entityMapper.Map<AsymmetricKeyPairIpcEntity, AsymmetricKeyPair>(rightEntity.ClientKeyPair));
    }
}