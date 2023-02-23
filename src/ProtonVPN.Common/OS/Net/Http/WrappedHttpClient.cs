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

namespace ProtonVPN.Common.OS.Net.Http
{
    public class WrappedHttpClient : IHttpClient
    {
        private readonly HttpClient _httpClient;

        public WrappedHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public TimeSpan Timeout
        {
            get => _httpClient.Timeout;
            set => _httpClient.Timeout = value;
        }

        public async Task<IHttpResponseMessage> GetAsync(string requestUri, CancellationToken ct) =>
            new WrappedHttpResponseMessage(await _httpClient.GetAsync(requestUri, ct));

        public async Task<IHttpResponseMessage> GetAsync(Uri requestUri) =>
            new WrappedHttpResponseMessage(await _httpClient.GetAsync(requestUri));

        public void Dispose() => _httpClient.Dispose();
    }
}