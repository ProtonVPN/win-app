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

using System;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.UserCertificateLogs;

namespace ProtonVPN.Vpn.ConnectionCertificates;

public class ConnectionCertificateCache : IConnectionCertificateCache
{
    public event EventHandler<EventArgs<ConnectionCertificate>> Changed;

    private readonly ILogger _logger;
    private readonly object _lock = new();

    private ConnectionCertificate _certificate;

    public ConnectionCertificateCache(ILogger logger)
    {
        _logger = logger;
    }

    public ConnectionCertificate Get()
    {
        lock(_lock)
        {
            return _certificate;
        }
    }

    public void Set(ConnectionCertificate certificate)
    {
        lock (_lock)
        {
            SetCertificateIfMoreRecent(certificate);
        }
    }

    private void SetCertificateIfMoreRecent(ConnectionCertificate certificate)
    {
        if (certificate is null || string.IsNullOrEmpty(certificate.Pem) || certificate.ExpirationDateUtc is null)
        {
            _logger.Warn<UserCertificateLog>($"Ignoring new certificate because the new certificate is null or has no data.");
            return;
        }

        if (_certificate is not null && certificate.Pem == _certificate.Pem)
        {
            _logger.Debug<UserCertificateLog>($"Ignoring new certificate because the new certificate is equal.");
            return;
        }

        if (_certificate is null)
        {
            SetCertificate(certificate, $"Certificate set. The certificate expires in '{certificate.ExpirationDateUtc}'.");
        }
        else if (certificate.ExpirationDateUtc > _certificate.ExpirationDateUtc)
        {
            SetCertificate(certificate, $"Certificate updated. " +
                $"New certificate expires in '{certificate.ExpirationDateUtc}'. " +
                $"Old certificate expired in '{_certificate.ExpirationDateUtc}'.");
        }
        else
        {
            _logger.Warn<UserCertificateLog>($"Ignoring new certificate because the expiration date " +
                $"'{certificate.ExpirationDateUtc}' is equal or older than the current one " +
                $"'{_certificate.ExpirationDateUtc}'.");
        }
    }

    private void SetCertificate(ConnectionCertificate certificate, string logMessage)
    {
        _logger.Info<UserCertificateLog>(logMessage);
        _certificate = certificate;
        Changed?.Invoke(this, new EventArgs<ConnectionCertificate>(certificate));
    }
}