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
using Newtonsoft.Json.Linq;

namespace ProtonVPN.UI.Tests.ApiClient
{
    public class ProtonTestApiClient
    {
        private readonly HttpClient _client;

        public ProtonTestApiClient()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://api.protonvpn.ch")
            };
        }

        public async Task<JArray> GetLogicalServers()
        {
            HttpResponseMessage response = await _client.GetAsync("/vpn/logicals");
            response.EnsureSuccessStatusCode();
            string responseBody = response.Content.ReadAsStringAsync().Result;
            JObject json = JObject.Parse(responseBody);
            return (JArray)json["LogicalServers"];
        }
    }
}
