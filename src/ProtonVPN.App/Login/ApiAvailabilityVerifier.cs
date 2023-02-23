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
using System.Threading.Tasks;
using ProtonVPN.Common.OS.Net.Http;

namespace ProtonVPN.Login
{
    public class ApiAvailabilityVerifier : IApiAvailabilityVerifier
    {
        private const string PROTON_WEBSITE_URL = "https://protonvpn.com";
        private const string PROTON_API_PING_URL = "https://account.protonvpn.com/api/tests/ping";
        private readonly IHttpClient _httpClient;

        public ApiAvailabilityVerifier(IHttpClients httpClients)
        {
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
                IHttpResponseMessage response = await _httpClient.GetAsync(PROTON_API_PING_URL);
                return response.IsSuccessStatusCode;
            }
            catch
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
            catch
            {
                return false;
            }
        }
    }
}