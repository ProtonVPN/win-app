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

using System.Security.Cryptography.X509Certificates;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Common.Legacy.Vpn;

public class VpnCredentialsMapper : IMapper<VpnCredentials, VpnCredentialsIpcEntity>
{
    private readonly ILogger _logger;
    private readonly IEntityMapper _entityMapper;

    public VpnCredentialsMapper(
        ILogger logger,
        IEntityMapper entityMapper)
    {
        _logger = logger;
        _entityMapper = entityMapper;
    }

    public VpnCredentialsIpcEntity Map(VpnCredentials leftEntity)
    {
        return new()
        {
            Certificate = new()
            {
                Pem = leftEntity.ClientCertPem ?? string.Empty,
                ExpirationDateUtc = leftEntity.ClientCertificateExpirationDateUtc ?? DateTime.MinValue,
            },
            ClientKeyPair = _entityMapper.Map<AsymmetricKeyPair, AsymmetricKeyPairIpcEntity>(leftEntity.ClientKeyPair),
            Username = leftEntity.Username,
            Password = leftEntity.Password,
        };
    }

    public VpnCredentials Map(VpnCredentialsIpcEntity rightEntity)
    {
        string pem = rightEntity.Certificate.Pem;
        if (!string.IsNullOrEmpty(pem))
        {
            try
            {
                using X509Certificate2 cert = X509Certificate2.CreateFromPem(pem);
                pem = cert.ExportCertificatePem();
            }
            catch (Exception e)
            {
                pem = string.Empty;
                _logger.Error<ConnectionLog>($"Failed to parse connection certificate.", e);
            }
        }

        return new(pem,
            rightEntity.Certificate.ExpirationDateUtc,
            _entityMapper.Map<AsymmetricKeyPairIpcEntity, AsymmetricKeyPair>(rightEntity.ClientKeyPair),
            rightEntity.Username,
            rightEntity.Password);
    }
}