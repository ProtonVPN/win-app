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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Timeout;

namespace ProtonVPN.Core.Api.Handlers
{
    /// <summary>
    /// Applies timeout to Http request and retries in case of timeout or failure.
    /// </summary>
    public class RetryingHandler : DelegatingHandler
    {
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

        private readonly AsyncPolicy<HttpResponseMessage> _policy;

        public RetryingHandler(TimeSpan timeout, int maxRetries, Func<int, DelegateResult<HttpResponseMessage>, Context, TimeSpan> sleepDurationProvider)
        {
            var httpRetryPolicy =
                Policy
                    .Handle<HttpRequestException>()
                    .Or<TaskCanceledException>()
                    .Or<TimeoutRejectedException>()
                    .OrResult<HttpResponseMessage>(r => HttpStatusCodesWorthRetrying.Contains(r.StatusCode))
                    .WaitAndRetryAsync(maxRetries, sleepDurationProvider, (outcome, timespan, retryCount, context) => Task.CompletedTask);

            var timeoutPolicy = Policy.TimeoutAsync(timeout);

            _policy = httpRetryPolicy.WrapAsync(timeoutPolicy);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return _policy.ExecuteAsync(async ct =>
                    await base.SendAsync(request, ct),
                cancellationToken);
        }
    }
}
