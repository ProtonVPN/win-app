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
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;

namespace ProtonVPN.Login
{
    public class SignUpAvailabilityProvider : ISignUpAvailabilityProvider
    {
        private const string PROTON_WEBSITE_URL = "https://protonvpn.com";
        private readonly IApiClient _apiClient;
        private readonly IHttpClient _httpClient;

        public SignUpAvailabilityProvider(IApiClient apiClient, IHttpClients httpClients)
        {
            _apiClient = apiClient;
            _httpClient = httpClients.Client();
            _httpClient.Timeout = TimeSpan.FromSeconds(3);
        }

        public async Task<bool> IsSignUpPageAccessibleAsync()
        {
            return await IsApiReachable() && await IsProtonWebSiteReachableAsync();
        }

        private async Task<bool> IsApiReachable()
        {
            try
            {
                ApiResponseResult<BaseResponse> response = await _apiClient.GetPingResponseAsync();
                return response.Success;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        private async Task<bool> IsProtonWebSiteReachableAsync()
        {
            try
            {
                IHttpResponseMessage response = await _httpClient.GetAsync(PROTON_WEBSITE_URL);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e) when (e is TimeoutException or TaskCanceledException)
            {
                return false;
            }
        }
    }
}