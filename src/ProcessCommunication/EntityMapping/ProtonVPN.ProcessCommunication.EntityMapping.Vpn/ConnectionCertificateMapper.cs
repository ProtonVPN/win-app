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
using ProtonVPN.Common.Core.LocalAgent;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.LocalAgent;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

public class ConnectionCertificateMapper : IMapper<ConnectionCertificate, ConnectionCertificateIpcEntity>
{
    private readonly ILogger _logger;

    public ConnectionCertificateMapper(ILogger logger)
    {
        _logger = logger;
    }

    public ConnectionCertificateIpcEntity Map(ConnectionCertificate leftEntity)
    {
        return leftEntity is null
            ? null
            : new ConnectionCertificateIpcEntity()
            {
                Pem = leftEntity.Pem,
                ExpirationDateUtc = leftEntity.ExpirationDateUtc,
            };
    }

    public ConnectionCertificate Map(ConnectionCertificateIpcEntity rightEntity)
    {
        if (rightEntity is null)
        {
            return null;
        }

        string pem = rightEntity.Pem;
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

        return new ConnectionCertificate(pem, rightEntity.ExpirationDateUtc);
    }
}