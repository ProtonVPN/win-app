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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProtonVPN.UI.Tests.TestsHelper.BTI
{
    public static class AtlasController
    {
        private static readonly HttpClient _client;

        static AtlasController()
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            _client = new HttpClient(handler)
            {
                BaseAddress = new Uri(Environment.GetEnvironmentVariable("BTI_ATLAS_CONTROLLER_IP"))
            };
        }

        public static async Task<string> ExecuteQuarkAsync(string scenario)
        {
            HttpResponseMessage response = await _client.GetAsync(scenario);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> SeedUserAsync(string username)
        {
            string response = await ExecuteQuarkAsync(Scenarios.SEED_PLUS_USER(username));
            string pattern = @"\(ID (\d+)\)";
            Match match = Regex.Match(response, pattern);
            return match.Groups[1].Value;
        }
    }
}
