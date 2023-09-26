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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Users;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Account
{
    public class UserCreationDateUpdater : IUserCreationDateUpdater, ILoggedInAware
    {
        private readonly IApiClient _api;
        private readonly ILogger _logger;
        private readonly IUserStorage _userStorage;

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public UserCreationDateUpdater(ILogger logger,
            IApiClient api,
            IUserStorage userStorage)
        {
            _api = api;
            _logger = logger;
            _userStorage = userStorage;
        }

        public void OnUserLoggedIn()
        {
            UpdateAsync();
        }

        private async Task UpdateAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                ApiResponseResult<UsersResponse> response = await _api.GetUserAsync();
                if (response.Success)
                {
                    _userStorage.StoreCreationDateUtc(DateTimeOffset.FromUnixTimeSeconds(response.Value.User.CreateTime).UtcDateTime);
                }
            }
            catch (Exception e)
            {
                if (e is not HttpRequestException)
                {
                    _logger.Error<AppLog>("An unexpected exception was thrown when updating the VPN info.", e);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}