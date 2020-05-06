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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Api.Extensions;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Api.Handlers
{
    /// <summary>
    /// Transparently refreshes access token in case Http request is not authorized and
    /// retries Http request with new access token.
    /// </summary>
    public class UnauthorizedResponseHandler : DelegatingHandler
    {
        private readonly ITokenClient _tokenClient;
        private readonly ITokenStorage _tokenStorage;
        private readonly IUserStorage _userStorage;

        private volatile Task<bool> _refreshTask = Task.FromResult(true);

        public event EventHandler SessionExpired;

        public UnauthorizedResponseHandler(
            ITokenClient tokenClient,
            ITokenStorage tokenStorage,
            IUserStorage userStorage)
        {
            _tokenClient = tokenClient;
            _tokenStorage = tokenStorage;
            _userStorage = userStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.AuthHeadersInvalid())
            {
                SessionExpired?.Invoke(this, EventArgs.Empty);
                return new UnauthorizedResponse();
            }

            var refreshTask = _refreshTask;
            if (!refreshTask.IsCompleted)
            {
                var refreshSucceeded = await refreshTask;
                return await ResendAsync(request, cancellationToken, refreshSucceeded);
            }

            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized && !_userStorage.User().Empty())
            {
                try
                {
                    var refreshSucceeded = await Refresh(refreshTask, cancellationToken);
                    return await ResendAsync(request, cancellationToken, refreshSucceeded);
                }
                finally
                {
                    response.Dispose();
                }
            }

            return response;
        }

        private async Task<HttpResponseMessage> ResendAsync(HttpRequestMessage request, CancellationToken cancellationToken,
            bool refreshSucceeded)
        {
            if (refreshSucceeded)
            {
                PrepareRequest(request);
                return await base.SendAsync(request, cancellationToken);
            }

            SessionExpired?.Invoke(this, EventArgs.Empty);
            return new UnauthorizedResponse();
        }

        private async Task<bool> Refresh(Task<bool> refreshTask, CancellationToken cancellationToken)
        {
            var taskCompletion = new TaskCompletionSource<bool>();
            var newTask = taskCompletion.Task;

            var prevTask = Interlocked.CompareExchange(ref _refreshTask, newTask, refreshTask);

            if (prevTask != refreshTask)
            {
                // ReSharper disable once PossibleNullReferenceException
                return await prevTask;
            }

            await taskCompletion.Wrap(() => RefreshTokens(cancellationToken));
            return await newTask;
        }

        private async Task<bool> RefreshTokens(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_tokenStorage.RefreshToken))
            {
                return false;
            }

            try
            {
                var response = await _tokenClient.RefreshTokenAsync(cancellationToken);

                if (response.Success)
                {
                    _tokenStorage.AccessToken = response.Value.AccessToken;
                    _tokenStorage.RefreshToken = response.Value.RefreshToken;

                    return true;
                }
            }
            catch (HttpRequestException)
            { }

            return false;
        }

        private void PrepareRequest(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenStorage.AccessToken);
        }
    }
}
