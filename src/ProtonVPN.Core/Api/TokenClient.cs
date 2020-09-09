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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Api.Data;

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
            string apiVersion,
            string locale) : base(logger, appVersion, tokenStorage, apiVersion, locale)
        {
            _client = client;
        }

        public async Task<ApiResponseResult<RefreshTokenResponse>> RefreshTokenAsync(CancellationToken token)
        {
            var data = new RefreshTokenData
            {
                ResponseType = "token",
                RefreshToken = TokenStorage.RefreshToken,
                GrantType = "refresh_token",
                RedirectUri = "http://api.protonvpn.ch"
            };

            try
            {
                var request = GetAuthorizedRequest(HttpMethod.Post, "auth/refresh");
                request.Content = GetJsonContent(data);

                using var response = await _client.SendAsync(request, token);
                var body = await response.Content.ReadAsStringAsync();
                return ApiResponseResult<RefreshTokenResponse>(body, response.StatusCode);
            }
            catch (Exception e) when (e.IsApiCommunicationException())
            {
                throw new HttpRequestException(e.Message);
            }
        }
    }
}
