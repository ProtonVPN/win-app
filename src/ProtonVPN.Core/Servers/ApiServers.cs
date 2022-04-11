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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ApiLogs;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.User;

namespace ProtonVPN.Core.Servers
{
    public class ApiServers : IApiServers
    {
        private const int RetryCount = 3;
        private const int RetryDelayInSeconds = 2;

        private readonly ILogger _logger;
        private readonly IApiClient _apiClient;
        private readonly TruncatedLocation _location;

        public ApiServers(
            ILogger logger,
            IApiClient apiClient,
            TruncatedLocation location)
        {
            _logger = logger;
            _apiClient = apiClient;
            _location = location;
        }

        public async Task<IReadOnlyCollection<LogicalServerContract>> GetServersAsync()
        {
            try
            {
                return await GetServersWithRetryAsync();
            }
            catch (HttpRequestException ex)
            {
                _logger.Error<ApiErrorLog>("API: Get servers failed", ex);
            }

            return Array.Empty<LogicalServerContract>();
        }

        private async Task<IReadOnlyCollection<LogicalServerContract>> GetServersWithRetryAsync()
        {
            AsyncRetryPolicy policy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(GetRetryPolicy());

            return await policy.ExecuteAsync<IReadOnlyCollection<LogicalServerContract>>(async _ =>
            {
                ApiResponseResult<ServerList> response = await _apiClient.GetServersAsync(_location.Ip());
                return response.Success ? response.Value.Servers : Array.Empty<LogicalServerContract>();
            }, CancellationToken.None);
        }

        private IEnumerable<TimeSpan> GetRetryPolicy()
        {
            return Backoff.ExponentialBackoff(TimeSpan.FromSeconds(RetryDelayInSeconds), RetryCount);
        }

        public async Task<IReadOnlyCollection<LogicalServerContract>> GetLoadsAsync()
        {
            try
            {
                ApiResponseResult<ServerList> response = await _apiClient.GetServerLoadsAsync(_location.Ip());
                if (response.Success)
                {
                    return response.Value.Servers;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.Error<ApiErrorLog>("API: Get servers failed", ex);
            }

            return Array.Empty<LogicalServerContract>();
        }
    }
}