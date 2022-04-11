/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Polly.Timeout;

namespace ProtonVPN.Core.Api.Handlers
{
    /// <summary>
    /// Applies timeout to Http request and retries in case of timeout or failure.
    /// </summary>
    public class RetryingHandler : DelegatingHandler
    {
        private const int ServersTimeoutInSeconds = 30;

        private static readonly HttpStatusCode[] HttpStatusCodesWorthRetrying =
        {
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout,
            HttpStatusCode.BadGateway,
            (HttpStatusCode) 429
        };

        private readonly AsyncPolicy<HttpResponseMessage> _basePolicy;
        private readonly TimeSpan _timeout;
        private readonly TimeSpan _uploadTimeout;

        public RetryingHandler(TimeSpan timeout, TimeSpan uploadTimeout, int maxRetries, Func<int, DelegateResult<HttpResponseMessage>, Context, TimeSpan> sleepDurationProvider)
        {
            _timeout = timeout;
            _uploadTimeout = uploadTimeout;

            AsyncRetryPolicy<HttpResponseMessage> retryAfterPolicy = Policy
                .HandleResult<HttpResponseMessage>(ContainsRetryAfterHeader)
                .WaitAndRetryAsync(maxRetries, sleepDurationProvider, (outcome, timespan, retryCount, context) => Task.CompletedTask);

            AsyncRetryPolicy<HttpResponseMessage> httpRetryPolicy =
                Policy
                    .Handle<HttpRequestException>()
                    .Or<TaskCanceledException>()
                    .Or<TimeoutRejectedException>()
                    .OrResult<HttpResponseMessage>(RetryRequired)
                    .WaitAndRetryAsync(GetBackOff(maxRetries));

            _basePolicy = retryAfterPolicy.WrapAsync(httpRetryPolicy);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return GetRetryPolicy(request)
                .ExecuteAsync(async ct => await base.SendAsync(request, ct), cancellationToken);
        }

        private AsyncPolicy<HttpResponseMessage> GetRetryPolicy(HttpRequestMessage request)
        {
            AsyncPolicy<HttpResponseMessage> policy = _basePolicy;
            if (request.RequestUri.AbsolutePath == "/vpn/logicals")
            {
                policy = policy.WrapAsync(Policy.TimeoutAsync(ServersTimeoutInSeconds));
            }
            else
            {
                policy = request.Content is MultipartFormDataContent
                    ? policy.WrapAsync(Policy.TimeoutAsync(_uploadTimeout))
                    : policy.WrapAsync(Policy.TimeoutAsync(_timeout));
            }

            return policy;
        }

        private bool RetryRequired(HttpResponseMessage response)
        {
            return HttpStatusCodesWorthRetrying.Contains(response.StatusCode) && !ContainsRetryAfterHeader(response);
        }

        private bool ContainsRetryAfterHeader(HttpResponseMessage response)
        {
            return response?.Headers?.RetryAfter != null;
        }

        private IEnumerable<TimeSpan> GetBackOff(int maxRetries)
        {
            return Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(2), maxRetries);
        }
    }
}