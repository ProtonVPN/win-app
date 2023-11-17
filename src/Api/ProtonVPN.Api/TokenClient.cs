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
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ApiLogs;

namespace ProtonVPN.Api;

public class TokenClient : BaseApiClient, ITokenClient
{
    private const int REFRESH_TOKEN_LOG_LENGTH = 5;
    private readonly HttpClient _client;

    public event EventHandler RefreshTokenExpired;

    public TokenClient(
        ILogger logger,
        ITokenHttpClientFactory tokenHttpClientFactory,
        IApiAppVersion appVersion,
        ISettings settings,
        IConfiguration config) : base(logger, appVersion, settings, config)
    {
        _client = tokenHttpClientFactory.GetTokenHttpClient();
    }

    public async Task<ApiResponseResult<RefreshTokenResponse>> RefreshTokenAsync(CancellationToken token)
    {
        return await RefreshTokenAsync(Settings.RefreshToken, token);
    }

    public async Task<ApiResponseResult<RefreshTokenResponse>> RefreshUnauthTokenAsync(CancellationToken token)
    {
        return await RefreshTokenAsync(Settings.UnauthRefreshToken, token);
    }

    public void TriggerRefreshTokenExpiration()
    {
        RefreshTokenExpired?.Invoke(this, EventArgs.Empty);
    }

    private async Task<ApiResponseResult<RefreshTokenResponse>> RefreshTokenAsync(string refreshToken, CancellationToken token)
    {
        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest
        {
            ResponseType = "token",
            RefreshToken = refreshToken,
            GrantType = "refresh_token",
            RedirectUri =  Config.Urls.ApiUrl
        };
        ValidateRefreshTokenData(refreshTokenRequest);

        try
        {
            HttpRequestMessage request = GetAuthorizedRequest(HttpMethod.Post, "auth/refresh");
            ValidateRequestHeaders(request);
            request.Content = GetJsonContent(refreshTokenRequest);
            LogRefreshToken(refreshTokenRequest);

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

    private void LogRefreshToken(RefreshTokenRequest request)
    {
        Logger.Info<ApiLog>($"Using refresh token value (showing only first {REFRESH_TOKEN_LOG_LENGTH} chars) " +
                            $"{request?.RefreshToken.GetFirstChars(REFRESH_TOKEN_LOG_LENGTH)}");
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
        string uid = (request?.Headers?.GetValues("x-pm-uid")?.FirstOrDefault()) 
            ?? throw new ArgumentNullException("The UID header in the HttpRequest can't be null.");
    }
}