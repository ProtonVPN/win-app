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

using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.OS.Net.Http;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProtonVPN.P2PDetection.Blocked
{
    /// <summary>
    /// Detects blocked traffic on free VPN servers.
    /// </summary>
    /// <remarks>
    /// P2P traffic is not allowed on free servers and some not free servers.
    /// All traffic gets blocked when P2P activity is detected.
    /// </remarks>
    public class BlockedTraffic : IBlockedTraffic
    {
        private readonly Uri _p2PStatusUri;

        private readonly IHttpClient _httpClient;

        public BlockedTraffic(IHttpClients httpClients, Uri p2PStatusUri, TimeSpan timeout)
        {
            Ensure.NotNull(httpClients, nameof(httpClients));
            Ensure.NotNull(p2PStatusUri, nameof(p2PStatusUri));

            _p2PStatusUri = p2PStatusUri;
            _httpClient = httpClients.Client(new HttpClientHandler
            {
                UseCookies = false
            });
            _httpClient.Timeout = timeout;
        }

        public async Task<bool> Detected()
        {
            string response = await GetResponse();
            return response.Contains("<!--P2P_WARNING-->");
        }

        private async Task<string> GetResponse()
        {
            using (IHttpResponseMessage response = await _httpClient.GetAsync(_p2PStatusUri))
            {
                if (!response.IsSuccessStatusCode)
                {
                    return string.Empty;
                }

                string content = await response.Content.ReadAsStringAsync();
                return content;
            }
        }
    }
}
