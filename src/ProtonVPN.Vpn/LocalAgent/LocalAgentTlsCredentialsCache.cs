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

using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.LocalAgent;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.LocalAgentLogs;

namespace ProtonVPN.Vpn.LocalAgent;

public class LocalAgentTlsCredentialsCache : ILocalAgentTlsCredentialsCache
{
    public Channel<LocalAgentTlsCredentialsUpdate> LocalAgentTlsCredentialsChannel { get; } = Channel.CreateUnbounded<LocalAgentTlsCredentialsUpdate>();
    public long CurrentVersion => Interlocked.Read(ref _currentVersion);

    private readonly ILogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private long _currentVersion;

    private LocalAgentTlsCredentials? _credentials;

    public LocalAgentTlsCredentialsCache(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<LocalAgentTlsCredentials?> GetAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            return _credentials;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SetAsync(LocalAgentTlsCredentials credentials, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            await SetCredentialsIfChangedAsync(credentials, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task SetCredentialsIfChangedAsync(LocalAgentTlsCredentials credentials, CancellationToken cancellationToken)
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
            await SetCredentialsAsync(credentials, $"Credentials set. The certificate expires in '{certificate.ExpirationDateUtc}'.", cancellationToken);
        }
        else if (certificate.ExpirationDateUtc > _credentials.ConnectionCertificate.ExpirationDateUtc)
        {
            await SetCredentialsAsync(credentials, $"Credentials updated. " +
                $"New certificate expires in '{certificate.ExpirationDateUtc}'. " +
                $"Old certificate expired in '{_credentials.ConnectionCertificate.ExpirationDateUtc}'.",
                cancellationToken);
        }
        else
        {
            _logger.Warn<LocalAgentTlsCredentialsLog>($"Ignoring new credentials because the certificate expiration date " +
                $"'{certificate.ExpirationDateUtc}' is equal or older than the current one " +
                $"'{_credentials.ConnectionCertificate.ExpirationDateUtc}'.");
        }
    }

    private async Task SetCredentialsAsync(LocalAgentTlsCredentials credentials, string logMessage, CancellationToken cancellationToken)
    {
        _logger.Info<LocalAgentTlsCredentialsLog>(logMessage);
        _credentials = credentials;

        long version = Interlocked.Increment(ref _currentVersion);
        await LocalAgentTlsCredentialsChannel.Writer.WriteAsync(new LocalAgentTlsCredentialsUpdate(credentials, version), cancellationToken);
    }
}