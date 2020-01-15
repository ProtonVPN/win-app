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
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Servers
{
    public class CachedServersProvider
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;
        private readonly ServersFileStorage _serversFileStorage;
        private readonly IUserStorage _userStorage;

        public CachedServersProvider(
            IApiClient apiClient,
            ILogger logger,
            ServersFileStorage serversFileStorage,
            IUserStorage userStorage)
        {
            _userStorage = userStorage;
            _apiClient = apiClient;
            _serversFileStorage = serversFileStorage;
            _logger = logger;
        }

        public async Task<ICollection<LogicalServerContract>> GetServersAsync()
        {
            try
            {
                var location = new TruncatedLocation(_userStorage.Location().Ip);
                var response = await _apiClient.GetServersAsync(location);
                if (response.Success)
                {
                    await SaveServersToCache(response.Value.Servers);
                    return response.Value.Servers;
                }

                return _serversFileStorage.Get();
            }
            catch (HttpRequestException ex)
            {
                _logger.Error("API: Get servers failed: " + ex.CombinedMessage());
                return _serversFileStorage.Get();
            }
        }

        private async Task SaveServersToCache(ICollection<LogicalServerContract> servers)
        {
            try
            {
                await Task.Run(() => _serversFileStorage.Save(servers));
            }
            catch (IOException e)
            {
                _logger.Error("Error saving servers to cache: " + e);
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.Error("Error saving servers to cache: " + e);
            }
        }
    }
}
