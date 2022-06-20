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
using ProtonVPN.Api.Handlers;
using ProtonVPN.Common.Configuration;

namespace ProtonVPN.Api
{
    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly Config _config;
        private readonly AlternativeHostHandler _alternativeHostHandler;

        public HttpClientFactory(Config config, AlternativeHostHandler alternativeHostHandler)
        {
            _config = config;
            _alternativeHostHandler = alternativeHostHandler;
        }

        public HttpClient GetApiHttpClientWithoutCache()
        {
            HttpClient client = GetApiHttpClientWithCache();
            client.DefaultRequestHeaders.ConnectionClose = true;
            return client;
        }

        public HttpClient GetApiHttpClientWithCache()
        {
            return new(_alternativeHostHandler) { BaseAddress = new Uri(_config.Urls.ApiUrl) };
        }
    }
}