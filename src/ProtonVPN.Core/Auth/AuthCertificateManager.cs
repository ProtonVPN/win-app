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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Certificates;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.UserCertificateLogs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Windows;

namespace ProtonVPN.Core.Auth
{
    public class AuthCertificateManager : IAuthCertificateManager, IHandle<WindowStateMessage>
    {
        private readonly IAuthKeyManager _authKeyManager;
        private readonly IApiClient _apiClient;
        private readonly IAppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private IList<string> _features = new List<string>();

        public AuthCertificateManager(
            IAuthKeyManager authKeyManager,
            IApiClient apiClient,
            IAppSettings appSettings,
            ILogger logger,
            IEventAggregator eventAggregator)
        {
            _authKeyManager = authKeyManager;
            _apiClient = apiClient;
            _appSettings = appSettings;
            _logger = logger;

            eventAggregator.Subscribe(this);
        }

        public async void Handle(WindowStateMessage message)
        {
            if (message.Active)
            {
                await RequestNewCertificateAsync();
            }
        }

        public void SetFeatures(IList<string> features)
        {
            _features = features?.ToList() ?? new List<string>();
        }

        public void DeleteKeyPairAndCertificate()
        {
            _authKeyManager.DeleteKeyPair();

            _appSettings.AuthenticationCertificateRequestUtcDate = null;
            _appSettings.AuthenticationCertificatePem = null;
            _appSettings.AuthenticationCertificateExpirationUtcDate = null;
            _appSettings.AuthenticationCertificateRefreshUtcDate = null;
            _appSettings.CertificationServerPublicKey = null;
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
            DateTimeOffset? refreshDate = _appSettings.AuthenticationCertificateRefreshUtcDate;
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
                _appSettings.AuthenticationCertificateRequestUtcDate = DateTimeOffset.UtcNow;
                _appSettings.AuthenticationCertificatePem = certificateResponseData.Value.Certificate;
                _appSettings.AuthenticationCertificateExpirationUtcDate =
                    DateTimeOffset.FromUnixTimeSeconds(certificateResponseData.Value.ExpirationTime);
                _appSettings.AuthenticationCertificateRefreshUtcDate =
                    DateTimeOffset.FromUnixTimeSeconds(certificateResponseData.Value.RefreshTime);
                _appSettings.CertificationServerPublicKey = certificateResponseData.Value.ServerPublicKey;
                _logger.Info<UserCertificateNewLog>("New auth certificate successfully saved. " +
                    $"Expires at {_appSettings.AuthenticationCertificateExpirationUtcDate}.");
            }

            return certificateResponseData;
        }

        private CertificateRequest CreateCertificateRequestData(IList<string> features)
        {
            return new()
            {
                ClientPublicKey = GetOrCreateClientPublicKeyPem(),
                Features = features ?? new List<string>(),
            };
        }

        private string GetOrCreateClientPublicKeyPem()
        {
            string clientPublicKey = _authKeyManager.GetPublicKey()?.Pem;
            if (clientPublicKey.IsNullOrEmpty())
            {
                _authKeyManager.RegenerateKeyPair();
                clientPublicKey = _authKeyManager.GetPublicKey()?.Pem;
            }

            return clientPublicKey;
        }
    }
}