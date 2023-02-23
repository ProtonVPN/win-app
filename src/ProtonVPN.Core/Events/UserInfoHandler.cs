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

using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Api.Contracts.Events;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Events
{
    public class UserInfoHandler : IApiDataChangeAware
    {
        private readonly IApiClient _apiClient;
        private readonly IUserStorage _userStorage;

        public UserInfoHandler(IApiClient apiClient, IUserStorage userStorage)
        {
            _userStorage = userStorage;
            _apiClient = apiClient;
        }

        public async Task OnApiDataChanged(EventResponse eventResponse)
        {
            if (eventResponse.VpnSettings == null)
            {
                return;
            }

            try
            {
                ApiResponseResult<VpnInfoWrapperResponse> response = await _apiClient.GetVpnInfoResponse();
                if (response.Success)
                {
                    _userStorage.StoreVpnInfo(response.Value);
                }
            }
            catch
            {
            }
        }
    }
}