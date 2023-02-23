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
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ApiLogs;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Api
{
    public class TokenClient : BaseApiClient, ITokenClient
    {
        private const int REFRESH_TOKEN_LOG_LENGTH = 5;
        private readonly HttpClient _client;

        public TokenClient(
            ILogger logger,
            ITokenHttpClientFactory tokenHttpClientFactory,
            IApiAppVersion appVersion,
            IAppSettings appSettings,
            IAppLanguageCache appLanguageCache,
            IConfiguration config) : base(logger, appVersion, appSettings, appLanguageCache, config)
        {
            _client = tokenHttpClientFactory.GetTokenHttpClient();
        }

        public async Task<ApiResponseResult<RefreshTokenResponse>> RefreshTokenAsync(CancellationToken token)
        {
            RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest
            {
                ResponseType = "token",
                RefreshToken = AppSettings.RefreshToken,
                GrantType = "refresh_token",
                RedirectUri = "http://api.protonvpn.ch"
            };
            ValidateRefreshTokenData(refreshTokenRequest);

            try
            {
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "auth/refresh");
                ValidateRequestHeaders(request);
                request.Content = GetJsonContent(refreshTokenRequest);
                LogRefreshToken();

                using HttpResponseMessage response = await _client.SendAsync(request, token);
                return await GetApiResponseResult<RefreshTokenResponse>(response);
            }
            catch (Exception e)
            {
                if (!e.IsApiCommunicationException())
                {
                    Logger.Error<ApiErrorLog>("An exception occurred in an API request " +
                        "that is not related with its communication.", e);
                }
                throw new HttpRequestException(e.Message);
            }
        }

        private void LogRefreshToken()
        {
            Logger.Info<ApiLog>($"Using refresh token value (showing only first {REFRESH_TOKEN_LOG_LENGTH} chars) " +
                                $"{AppSettings.RefreshToken.GetFirstChars(REFRESH_TOKEN_LOG_LENGTH)}");
        }

        private void ValidateRefreshTokenData(RefreshTokenRequest request)
        {
            if (request.RefreshToken == null)
            {
                throw new ArgumentNullException("The RefreshToken in RefreshTokenData can't be null.");
            }
        }

        private void ValidateRequestHeaders(HttpRequestMessage request)
        {
            string uid = request?.Headers?.GetValues("x-pm-uid")?.FirstOrDefault();

            if (uid == null)
            {
                throw new ArgumentNullException("The UID header in the HttpRequest can't be null.");
            }
        }
    }
}