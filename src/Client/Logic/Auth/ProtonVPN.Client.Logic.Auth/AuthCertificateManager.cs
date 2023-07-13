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
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.UserCertificateLogs;

namespace ProtonVPN.Client.Logic.Auth;

//TODO: call RequestNewCertificateAsync on window restore
public class AuthCertificateManager : IAuthCertificateManager
{
    private readonly ISettings _settings;
    private readonly IAuthKeyManager _authKeyManager;
    private readonly IApiClient _apiClient;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private IList<string> _features = new List<string>();

    public AuthCertificateManager(
        ISettings settings,
        IAuthKeyManager authKeyManager,
        IApiClient apiClient,
        ILogger logger)
    {
        _settings = settings;
        _authKeyManager = authKeyManager;
        _apiClient = apiClient;
        _logger = logger;
    }

    public void SetFeatures(IList<string> features)
    {
        _features = features.ToList();
    }

    public void DeleteKeyPairAndCertificate()
    {
        _authKeyManager.DeleteKeyPair();

        _settings.CertificationServerPublicKey = null;
        _settings.AuthenticationCertificatePem = null;
        _settings.AuthenticationCertificateRequestUtcDate = null;
        _settings.AuthenticationCertificateExpirationUtcDate = null;
        _settings.AuthenticationCertificateRefreshUtcDate = null;
        
        _logger.Info<UserCertificateRevokedLog>("Auth certificate deleted.");
    }

    private enum NewCertificateRequestParameter
    {
        NewCertificateIfCurrentIsOld = 0,
        ForceNewCertificate = 1,
        ForceNewKeyPairAndCertificate = 2
    }

    public async Task RequestNewCertificateAsync()
    {
        await EnqueueRequestAsync(NewCertificateRequestParameter.NewCertificateIfCurrentIsOld);
    }

    public async Task ForceRequestNewCertificateAsync()
    {
        await EnqueueRequestAsync(NewCertificateRequestParameter.ForceNewCertificate);
    }

    public async Task ForceRequestNewKeyPairAndCertificateAsync()
    {
        await EnqueueRequestAsync(NewCertificateRequestParameter.ForceNewKeyPairAndCertificate);
    }

    private async Task EnqueueRequestAsync(NewCertificateRequestParameter parameter)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (parameter != NewCertificateRequestParameter.NewCertificateIfCurrentIsOld || IsToRequest())
            {
                LogNewCertificateRequest(parameter);
                RegenerateKeyPairIfRequested(parameter);
                IList<string> features = _features.ToList();
                ApiResponseResult<CertificateResponse> response = await RequestAsync(features);
                if (response.Failure)
                {
                    _logger.Error<UserCertificateRefreshErrorLog>("Auth certificate request failed with " +
                                                                  $"Status Code {response.ResponseMessage.StatusCode}, " +
                                                                  $"Internal Code {response.Value.Code}, " +
                                                                  $"Error '{response.Value.Error}'.");
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error<UserCertificateRefreshErrorLog>("Auth certificate request failed.", e);
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
                _logger.Info<UserCertificateRefreshLog>("Requesting a new auth certificate since the current one is considered old.");
                break;
            case NewCertificateRequestParameter.ForceNewCertificate:
                _logger.Info<UserCertificateRefreshLog>("Forcing a new auth certificate request.");
                break;
            case NewCertificateRequestParameter.ForceNewKeyPairAndCertificate:
                _logger.Info<UserCertificateRefreshLog>("Generating new auth key pair and forcing a new auth certificate request.");
                break;
        }
    }

    private bool IsToRequest()
    {
        DateTimeOffset? refreshDate = _settings.AuthenticationCertificateRefreshUtcDate;
        return !refreshDate.HasValue || DateTimeOffset.UtcNow >= refreshDate.Value;
    }

    private void RegenerateKeyPairIfRequested(NewCertificateRequestParameter parameter)
    {
        if (parameter == NewCertificateRequestParameter.ForceNewKeyPairAndCertificate)
        {
            _authKeyManager.RegenerateKeyPair();
        }
    }

    private async Task<ApiResponseResult<CertificateResponse>> RequestAsync(IList<string> features)
    {
        ApiResponseResult<CertificateResponse> certificateResponseData =
            await RequestAuthCertificateAsync(features);

        if (certificateResponseData.Failure && certificateResponseData.Value.Code == ResponseCodes.ClientPublicKeyConflict)
        {
            _logger.Warn<UserCertificateRefreshErrorLog>("New auth certificate failed because the " +
                                                         "client public key is already in use. Generating a new key pair and retrying.");
            _authKeyManager.RegenerateKeyPair();
            certificateResponseData = await RequestAuthCertificateAsync(features);
        }

        return certificateResponseData;
    }

    private async Task<ApiResponseResult<CertificateResponse>> RequestAuthCertificateAsync(IList<string> features)
    {
        CertificateRequest certificateRequest = CreateCertificateRequestData(features);
        ApiResponseResult<CertificateResponse> certificateResponseData =
            await _apiClient.RequestAuthCertificateAsync(certificateRequest);

        if (certificateResponseData.Success)
        {
            _settings.AuthenticationCertificateRequestUtcDate = DateTimeOffset.UtcNow;
            _settings.AuthenticationCertificatePem = certificateResponseData.Value.Certificate;
            _settings.AuthenticationCertificateExpirationUtcDate =
                DateTimeOffset.FromUnixTimeSeconds(certificateResponseData.Value.ExpirationTime);
            _settings.AuthenticationCertificateRefreshUtcDate =
                DateTimeOffset.FromUnixTimeSeconds(certificateResponseData.Value.RefreshTime);
            _settings.CertificationServerPublicKey = certificateResponseData.Value.ServerPublicKey;
            _logger.Info<UserCertificateNewLog>("New auth certificate successfully saved. " +
                                                $"Expires at {_settings.AuthenticationCertificateExpirationUtcDate}.");
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
        string clientPublicKey = _authKeyManager.GetPublicKey()?.Pem;
        if (string.IsNullOrEmpty(clientPublicKey))
        {
            _authKeyManager.RegenerateKeyPair();
            clientPublicKey = _authKeyManager.GetPublicKey()?.Pem;
        }

        return clientPublicKey;
    }
}