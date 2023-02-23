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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.UserLogs;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Api.Handlers
{
    /// <summary>
    /// Transparently refreshes access token in case Http request is not authorized and
    /// retries Http request with new access token.
    /// </summary>
    public class UnauthorizedResponseHandler : DelegatingHandler
    {
        private readonly ITokenClient _tokenClient;
        private readonly IAppSettings _appSettings;
        private readonly IUserStorage _userStorage;

        private readonly ILogger _logger;
        private volatile Task<RefreshTokenStatus> _refreshTask = Task.FromResult(RefreshTokenStatus.Success);

        public event EventHandler SessionExpired;

        public UnauthorizedResponseHandler(
            ITokenClient tokenClient,
            IAppSettings appSettings,
            IUserStorage userStorage,
            ILogger logger)
        {
            _tokenClient = tokenClient;
            _appSettings = appSettings;
            _userStorage = userStorage;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.AuthHeadersInvalid())
            {
                SessionExpired?.Invoke(this, EventArgs.Empty);
                return FailResponse.UnauthorizedResponse();
            }

            Task<RefreshTokenStatus> refreshTask = _refreshTask;
            if (!refreshTask.IsCompleted)
            {
                RefreshTokenStatus refreshSucceeded = await refreshTask;
                return await ResendAsync(request, cancellationToken, refreshSucceeded);
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized && !_userStorage.GetUser().Empty())
            {
                try
                {
                    RefreshTokenStatus refreshSucceeded = await Refresh(refreshTask, cancellationToken);
                    return await ResendAsync(request, cancellationToken, refreshSucceeded);
                }
                finally
                {
                    response.Dispose();
                }
            }

            return response;
        }

        private async Task<HttpResponseMessage> ResendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken,
            RefreshTokenStatus refreshTokenStatus)
        {
            switch (refreshTokenStatus)
            {
                case RefreshTokenStatus.Success:
                    PrepareRequest(request);
                    return await base.SendAsync(request, cancellationToken);
                case RefreshTokenStatus.Unauthorized:
                    SessionExpired?.Invoke(this, EventArgs.Empty);
                    return FailResponse.UnauthorizedResponse();
                default:
                    return FailResponse.UnauthorizedResponse();
            }
        }

        private async Task<RefreshTokenStatus> Refresh(Task<RefreshTokenStatus> refreshTask,
            CancellationToken cancellationToken)
        {
            TaskCompletionSource<RefreshTokenStatus> taskCompletion = new TaskCompletionSource<RefreshTokenStatus>();
            Task<RefreshTokenStatus> newTask = taskCompletion.Task;

            Task<RefreshTokenStatus> prevTask = Interlocked.CompareExchange(ref _refreshTask, newTask, refreshTask);

            if (prevTask != refreshTask)
            {
                // ReSharper disable once PossibleNullReferenceException
                return await prevTask;
            }

            await taskCompletion.Wrap(() => RefreshTokens(cancellationToken));
            return await newTask;
        }

        private async Task<RefreshTokenStatus> RefreshTokens(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_appSettings.RefreshToken) || string.IsNullOrEmpty(_appSettings.Uid))
            {
                return RefreshTokenStatus.Unauthorized;
            }

            try
            {
                ApiResponseResult<RefreshTokenResponse> response =
                    await _tokenClient.RefreshTokenAsync(cancellationToken);

                if (response.Success)
                {
                    _appSettings.AccessToken = response.Value.AccessToken;
                    _appSettings.RefreshToken = response.Value.RefreshToken;

                    return RefreshTokenStatus.Success;
                }
            }
            catch (ArgumentNullException e)
            {
                _logger.Error<UserLog>($"An error occurred when refreshing the auth token: {e.ParamName}");
            }
            catch (Exception)
            {
                return RefreshTokenStatus.Fail;
            }

            return RefreshTokenStatus.Unauthorized;
        }

        private void PrepareRequest(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _appSettings.AccessToken);
        }
    }
}