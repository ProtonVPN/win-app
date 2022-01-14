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

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ApiLogs;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.User;

namespace ProtonVPN.Core.Servers
{
    public class ApiServers : IApiServers
    {
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
                ApiResponseResult<ServerList> response = await _apiClient.GetServersAsync(_location.Ip());
                if (response.Success)
                {
                    return response.Value.Servers;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.Error<ApiErrorLog>("API: Get servers failed", ex);
            }

            return new LogicalServerContract[0];
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

            return new LogicalServerContract[0];
        }
    }
}
