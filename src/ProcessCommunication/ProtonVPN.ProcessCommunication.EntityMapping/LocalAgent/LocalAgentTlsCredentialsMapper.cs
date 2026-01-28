/*
 * Copyright (c) 2026 Proton AG
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
using ProtonVPN.ProcessCommunication.Contracts.Entities.LocalAgent;
using ProtonVPN.Vpn.LocalAgent;

namespace ProtonVPN.ProcessCommunication.EntityMapping.LocalAgent;

public class LocalAgentTlsCredentialsMapper : IMapper<LocalAgentTlsCredentials, LocalAgentTlsCredentialsIpcEntity>
{
    private readonly IEntityMapper _entityMapper;

    public LocalAgentTlsCredentialsMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public LocalAgentTlsCredentialsIpcEntity Map(LocalAgentTlsCredentials leftEntity)
    {
        return leftEntity is null
            ? null
            : new LocalAgentTlsCredentialsIpcEntity()
            {
                ConnectionCertificate = _entityMapper.Map<ConnectionCertificate, ConnectionCertificateIpcEntity>(leftEntity.ConnectionCertificate),
                ClientKeyPair = _entityMapper.Map<AsymmetricKeyPair, AsymmetricKeyPairIpcEntity>(leftEntity.ClientKeyPair),
            };
    }

    public LocalAgentTlsCredentials Map(LocalAgentTlsCredentialsIpcEntity rightEntity)
    {
        return rightEntity is null
            ? null
            : new LocalAgentTlsCredentials(
                _entityMapper.Map<ConnectionCertificateIpcEntity, ConnectionCertificate>(rightEntity.ConnectionCertificate),
                _entityMapper.Map<AsymmetricKeyPairIpcEntity, AsymmetricKeyPair>(rightEntity.ClientKeyPair));
    }
}