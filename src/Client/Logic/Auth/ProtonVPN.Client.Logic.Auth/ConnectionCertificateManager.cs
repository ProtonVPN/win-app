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

using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Certificates;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.UserCertificateLogs;

namespace ProtonVPN.Client.Logic.Auth;

public class ConnectionCertificateManager : IConnectionCertificateManager
{
    private readonly ISettings _settings;
    private readonly IConnectionKeyManager _connectionKeyManager;
    private readonly IApiClient _apiClient;
    private readonly ILogger _logger;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private IList<string> _features = new List<string>();

    public ConnectionCertificateManager(
        ISettings settings,
        IConnectionKeyManager connectionKeyManager,
        IApiClient apiClient,
        ILogger logger,
        IEventMessageSender eventMessageSender)
    {
        _settings = settings;
        _connectionKeyManager = connectionKeyManager;
        _apiClient = apiClient;
        _logger = logger;
        _eventMessageSender = eventMessageSender;
    }

    public void SetFeatures(IList<string> features)
    {
        _features = features.ToList();
    }

    public void DeleteKeyPairAndCertificate()
    {
        _connectionKeyManager.DeleteKeyPair();
        _settings.ConnectionCertificate = null;
        SendUpdateMessage(null);
        _logger.Info<UserCertificateRevokedLog>("Connection certificate deleted.");
    }

    public void DeleteKeyPairAndCertificateIfMatches(string expiredCertificatePem)
    {
        if (expiredCertificatePem == _settings.ConnectionCertificate?.Pem)
        {
            DeleteKeyPairAndCertificate();
        }
    }

    private enum NewCertificateRequestParameter
    {
        NewCertificateIfCurrentIsOld = 0,
        ForceNewCertificate = 1,
        ForceNewKeyPairAndCertificate = 2
    }

    public async Task RequestNewCertificateAsync(CancellationToken cancellationToken = default, string? expiredCertificatePem = null)
    {
        await EnqueueRequestAsync(NewCertificateRequestParameter.NewCertificateIfCurrentIsOld,
            cancellationToken,
            expiredCertificatePem);
    }

    public async Task ForceRequestNewCertificateAsync(CancellationToken cancellationToken = default)
    {
        await EnqueueRequestAsync(NewCertificateRequestParameter.ForceNewCertificate, cancellationToken);
    }

    public async Task ForceRequestNewKeyPairAndCertificateAsync()
    {
        await EnqueueRequestAsync(NewCertificateRequestParameter.ForceNewKeyPairAndCertificate);
    }

    private async Task EnqueueRequestAsync(
        NewCertificateRequestParameter parameter,
        CancellationToken cancellationToken = default,
        string? expiredCertificatePem = null)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (parameter != NewCertificateRequestParameter.NewCertificateIfCurrentIsOld ||
                IsToRequest(expiredCertificatePem))
            {
                LogNewCertificateRequest(parameter);
                RegenerateKeyPairIfRequested(parameter);
                IList<string> features = _features.ToList();
                ApiResponseResult<CertificateResponse> response = await RequestAsync(features, cancellationToken);
                if (response.Failure)
                {
                    _logger.Error<UserCertificateRefreshErrorLog>("Connection certificate request failed with " +
                        $"Status Code {response.ResponseMessage.StatusCode}, " +
                        $"Internal Code {response.Value.Code}, " +
                        $"Error '{response.Value.Error}'.");
                }
            }
            else
            {
                SendMessageWithCurrentCertificate();
            }
        }
        catch (Exception e)
        {
            _logger.Error<UserCertificateRefreshErrorLog>("Connection certificate request failed.", e);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void LogNewCertificateRequest(NewCertificateRequestParameter parameter)
    {
        switch (parameter)
        {
            case NewCertificateRequestParameter.NewCertificateIfCurrentIsOld:
                _logger.Info<UserCertificateRefreshLog>("Requesting a new connection certificate since the current one is considered old.");
                break;
            case NewCertificateRequestParameter.ForceNewCertificate:
                _logger.Info<UserCertificateRefreshLog>("Forcing a new connection certificate request.");
                break;
            case NewCertificateRequestParameter.ForceNewKeyPairAndCertificate:
                _logger.Info<UserCertificateRefreshLog>("Generating new connection key pair and forcing a new connection certificate request.");
                break;
        }
    }

    private bool IsToRequest(string? expiredCertificatePem)
    {
        ConnectionCertificate? connectionCertificate = _settings.ConnectionCertificate;
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        return connectionCertificate is null ||
               string.IsNullOrWhiteSpace(connectionCertificate.Value.Pem) ||
               utcNow >= connectionCertificate.Value.RefreshUtcDate ||
               utcNow >= connectionCertificate.Value.ExpirationUtcDate ||
               expiredCertificatePem == connectionCertificate.Value.Pem;
    }

    private void RegenerateKeyPairIfRequested(NewCertificateRequestParameter parameter)
    {
        if (parameter == NewCertificateRequestParameter.ForceNewKeyPairAndCertificate)
        {
            _connectionKeyManager.RegenerateKeyPair();
        }
    }

    private async Task<ApiResponseResult<CertificateResponse>> RequestAsync(IList<string> features, CancellationToken cancellationToken)
    {
        ApiResponseResult<CertificateResponse> certificateResponseData =
            await RequestConnectionCertificateAsync(features, cancellationToken);

        if (certificateResponseData.Failure && certificateResponseData.Value.Code == ResponseCodes.ClientPublicKeyConflict)
        {
            _logger.Warn<UserCertificateRefreshErrorLog>("New connection certificate failed because the " +
                                                         "client public key is already in use. Generating a new key pair and retrying.");
            _connectionKeyManager.RegenerateKeyPair();
            certificateResponseData = await RequestConnectionCertificateAsync(features, cancellationToken);
        }

        return certificateResponseData;
    }

    private async Task<ApiResponseResult<CertificateResponse>> RequestConnectionCertificateAsync(IList<string> features, CancellationToken cancellationToken)
    {
        CertificateRequest certificateRequest = CreateCertificateRequestData(features);
        ApiResponseResult<CertificateResponse> certificateResponseData =
            await _apiClient.RequestConnectionCertificateAsync(certificateRequest, cancellationToken);

        if (certificateResponseData.Success)
        {
            ConnectionCertificate connectionCertificate = new()
            {
                Pem = certificateResponseData.Value.Certificate,
                RequestUtcDate = DateTimeOffset.UtcNow,
                RefreshUtcDate = DateTimeOffset.FromUnixTimeSeconds(certificateResponseData.Value.RefreshTime),
                ExpirationUtcDate = DateTimeOffset.FromUnixTimeSeconds(certificateResponseData.Value.ExpirationTime),
            };
            _settings.ConnectionCertificate = connectionCertificate;

            _logger.Info<UserCertificateNewLog>("New connection certificate successfully saved. " +
                                                $"Expires at {connectionCertificate.ExpirationUtcDate}.");

            SendUpdateMessage(connectionCertificate);
        }

        return certificateResponseData;
    }

    private CertificateRequest CreateCertificateRequestData(IList<string> features)
    {
        return new()
        {
            ClientPublicKey = GetOrCreateClientPublicKeyPem(),
            Features = features,
        };
    }

    private string GetOrCreateClientPublicKeyPem()
    {
        string? clientPublicKey = _connectionKeyManager.GetPublicKey()?.Pem;
        if (string.IsNullOrEmpty(clientPublicKey))
        {
            _connectionKeyManager.RegenerateKeyPair();
            clientPublicKey = _connectionKeyManager.GetPublicKey()?.Pem;
        }

        return clientPublicKey ?? string.Empty;
    }

    private void SendUpdateMessage(ConnectionCertificate? connectionCertificate)
    {
        ConnectionCertificateUpdatedMessage message = new()
        {
            Certificate = connectionCertificate
        };

        _eventMessageSender.Send(message);
    }

    private void SendMessageWithCurrentCertificate()
    {
        SendUpdateMessage(_settings.ConnectionCertificate);
    }
}