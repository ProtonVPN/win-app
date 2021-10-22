/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Api.Data;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Api
{
    public class TokenClient : BaseApiClient, ITokenClient
    {
        private readonly HttpClient _client;

        public TokenClient(
            ILogger logger,
            HttpClient client,
            IApiAppVersion appVersion,
            ITokenStorage tokenStorage,
            IAppLanguageCache appLanguageCache,
            string apiVersion) : base(logger, appVersion, tokenStorage, appLanguageCache, apiVersion)
        {
            _client = client;
        }

        public async Task<ApiResponseResult<RefreshTokenResponse>> RefreshTokenAsync(CancellationToken token)
        {
            RefreshTokenData data = new RefreshTokenData
            {
                ResponseType = "token",
                RefreshToken = TokenStorage.RefreshToken,
                GrantType = "refresh_token",
                RedirectUri = "http://api.protonvpn.ch"
            };
            ValidateRefreshTokenData(data);

            try
            {
                HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "auth/refresh");
                this.ValidateRequestHeaders(request);
                request.Content = GetJsonContent(data);

                using HttpResponseMessage response = await _client.SendAsync(request, token);
                string body = await response.Content.ReadAsStringAsync();
                return ApiResponseResult<RefreshTokenResponse>(body, response.StatusCode);
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message);
            }
        }

        private void ValidateRefreshTokenData(RefreshTokenData data)
        {
            if (data.RefreshToken == null)
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
