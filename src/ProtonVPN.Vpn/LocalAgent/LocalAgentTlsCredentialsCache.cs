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

using System;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.LocalAgentLogs;

namespace ProtonVPN.Vpn.LocalAgent;

public class LocalAgentTlsCredentialsCache : ILocalAgentTlsCredentialsCache
{
    public event EventHandler<EventArgs<LocalAgentTlsCredentials>> Changed;

    private readonly ILogger _logger;
    private readonly object _lock = new();

    private LocalAgentTlsCredentials _credentials;

    public LocalAgentTlsCredentialsCache(ILogger logger)
    {
        _logger = logger;
    }

    public LocalAgentTlsCredentials Get()
    {
        lock(_lock)
        {
            return _credentials;
        }
    }

    public void Set(LocalAgentTlsCredentials credentials)
    {
        lock (_lock)
        {
            SetCredentialsIfChanged(credentials);
        }
    }

    private void SetCredentialsIfChanged(LocalAgentTlsCredentials credentials)
    {
        ConnectionCertificate certificate = credentials.ConnectionCertificate;

        if (credentials is null ||
            string.IsNullOrEmpty(certificate.Pem) ||
            certificate.ExpirationDateUtc is null ||
            string.IsNullOrEmpty(credentials.ClientKeyPair?.SecretKey.Pem))
        {
            _logger.Warn<LocalAgentTlsCredentialsLog>($"Ignoring new credentials because it is null or has no data.");
            return;
        }

        if (_credentials is not null)
        {
            if (_credentials.ClientKeyPair.SecretKey.Pem != credentials.ClientKeyPair.SecretKey.Pem &&
                _credentials.ConnectionCertificate.Pem == certificate.Pem)
            {
                _logger.Warn<LocalAgentTlsCredentialsLog>($"Ignoring new credentials, because the private key has changed, but the certificate is the same.");
                return;
            }
            else if (certificate.Pem == _credentials.ConnectionCertificate.Pem)
            {
                _logger.Debug<LocalAgentTlsCredentialsLog>($"Ignoring new credentials because the new certificate is equal.");
                return;
            }
        }

        if (_credentials is null)
        {
            SetCredentials(credentials, $"Credentials set. The certificate expires in '{certificate.ExpirationDateUtc}'.");
        }
        else if (certificate.ExpirationDateUtc > _credentials.ConnectionCertificate.ExpirationDateUtc)
        {
            SetCredentials(credentials, $"Credentials updated. " +
                $"New certificate expires in '{certificate.ExpirationDateUtc}'. " +
                $"Old certificate expired in '{_credentials.ConnectionCertificate.ExpirationDateUtc}'.");
        }
        else
        {
            _logger.Warn<LocalAgentTlsCredentialsLog>($"Ignoring new credentials because the certificate expiration date " +
                $"'{certificate.ExpirationDateUtc}' is equal or older than the current one " +
                $"'{_credentials.ConnectionCertificate.ExpirationDateUtc}'.");
        }
    }

    private void SetCredentials(LocalAgentTlsCredentials credentials, string logMessage)
    {
        _logger.Info<LocalAgentTlsCredentialsLog>(logMessage);
        _credentials = credentials;
        Changed?.Invoke(this, new EventArgs<LocalAgentTlsCredentials>(credentials));
    }
}