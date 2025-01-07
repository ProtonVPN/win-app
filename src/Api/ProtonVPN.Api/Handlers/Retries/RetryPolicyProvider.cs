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
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using ProtonVPN.Api.Extensions;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ApiLogs;

namespace ProtonVPN.Api.Handlers.Retries
{
    public class RetryPolicyProvider : IRetryPolicyProvider
    {
        private readonly ILogger _logger;
        private readonly SleepDurationProvider _sleepDurationProvider;
        private readonly IRetryCountProvider _retryCountProvider;
        private readonly IRequestTimeoutProvider _requestTimeoutProvider;

        public RetryPolicyProvider(
            ILogger logger,
            SleepDurationProvider sleepDurationProvider,
            IRetryCountProvider retryCountProvider,
            IRequestTimeoutProvider requestTimeoutProvider)
        {
            _logger = logger;
            _sleepDurationProvider = sleepDurationProvider;
            _retryCountProvider = retryCountProvider;
            _requestTimeoutProvider = requestTimeoutProvider;
        }

        public AsyncPolicy<HttpResponseMessage> GetRetryPolicy(HttpRequestMessage request)
        {
            return GetResponseMessageRetryPolicy(request)
                .WrapAsync(GetResponseMessageRetryOncePolicy(request))
                .WrapAsync(GetExceptionRetryPolicy(request))
                .WrapAsync(GetTimeoutPolicy(request));
        }

        private AsyncPolicy GetTimeoutPolicy(HttpRequestMessage request)
        {
            return Policy.TimeoutAsync(_requestTimeoutProvider.GetTimeout(request));
        }

        private AsyncPolicy<HttpResponseMessage> GetResponseMessageRetryOncePolicy(HttpRequestMessage request)
        {
            return Policy
                .HandleResult<HttpResponseMessage>(response => response.IsToRetryOnce())
                .WaitAndRetryAsync(1, _ => TimeSpan.Zero);
        }

        private Func<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context, Task> OnRetryAsync(HttpRequestMessage request)
        {
            Task RetryAsyncFunction(DelegateResult<HttpResponseMessage> response, TimeSpan timeSpan, int retryCount, Context _)
            {
                _logger.Info<ApiLog>(GetRetryLogMessage(request, timeSpan, retryCount,
                    GetReasonByHttpStatusCode(response.Result.StatusCode)));

                return Task.CompletedTask;
            }

            return RetryAsyncFunction;
        }

        private AsyncPolicy<HttpResponseMessage> GetResponseMessageRetryPolicy(HttpRequestMessage request)
        {
            return Policy
                .HandleResult<HttpResponseMessage>(response => response.IsToRetry() && !response.IsToRetryOnce())
                .WaitAndRetryAsync(_retryCountProvider.GetRetryCount(request),
                    _sleepDurationProvider.ResponseMessageDurationFunction, OnRetryAsync(request));
        }

        private string GetRetryLogMessage(HttpRequestMessage request, TimeSpan timeSpan, int retryCount, string reason)
        {
            return $"Waiting for {timeSpan} before retrying {request.Method} {request.RequestUri} due to {reason}. " +
                   $"Retry count: {retryCount}.";
        }

        private AsyncRetryPolicy GetExceptionRetryPolicy(HttpRequestMessage request)
        {
            return Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(_retryCountProvider.GetRetryCount(request),
                    _sleepDurationProvider.RequestExceptionDurationFunction,
                    (exception, timeSpan, retryCount, _) =>
                    {
                        _logger.Info<ApiLog>(GetRetryLogMessage(request, timeSpan, retryCount,
                            GetReasonByException(exception)));
                    });
        }

        private string GetReasonByHttpStatusCode(HttpStatusCode statusCode)
        {
            return $"{statusCode} status code";
        }

        private string GetReasonByException(Exception e)
        {
            return $"exception {e.CombinedMessage()}";
        }
    }
}