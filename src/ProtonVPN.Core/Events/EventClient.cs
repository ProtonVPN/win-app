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
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Events;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Events
{
    public class EventClient
    {
        private readonly IApiClient _apiClient;
        private readonly IAppSettings _appSettings;

        public event EventHandler<EventResponse> ApiDataChanged;

        public EventClient(IApiClient apiClient, IAppSettings appSettings)
        {
            _appSettings = appSettings;
            _apiClient = apiClient;
        }

        public async Task GetEvents()
        {
            try
            {
                ApiResponseResult<EventResponse> response = await _apiClient.GetEventResponse(_appSettings.LastEventId);
                if (response.Success && _appSettings.LastEventId != response.Value.EventId)
                {
                    _appSettings.LastEventId = response.Value.EventId;
                    ApiDataChanged?.Invoke(this, response.Value);
                }
            }
            catch
            {
            }
        }

        public async Task StoreLatestEvent()
        {
            try
            {
                ApiResponseResult<EventResponse> response = await _apiClient.GetEventResponse();
                if (response.Success)
                {
                    _appSettings.LastEventId = response.Value.EventId;
                }
            }
            catch
            {
            }
        }
    }
}