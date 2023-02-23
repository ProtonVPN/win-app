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
using ProtonVPN.Api.Handlers;
using ProtonVPN.Api.Handlers.StackBuilders;
using ProtonVPN.Api.Handlers.Retries;
using ProtonVPN.Api.Handlers.TlsPinning;
using ProtonVPN.Common.Configuration;

namespace ProtonVPN.Api
{
    public class ApiHttpClientFactory : IApiHttpClientFactory
    {
        private readonly IConfiguration _config;
        private readonly HttpMessageHandler _innerHandler;

        public ApiHttpClientFactory(IConfiguration config, 
            AlternativeHostHandler alternativeHostHandler,
            CancellingHandlerBase cancellingHandlerBase,
            UnauthorizedResponseHandler unauthorizedResponseHandler,
            HumanVerificationHandlerBase humanVerificationHandlerBase,
            OutdatedAppHandler outdatedAppHandler,
            RetryingHandlerBase retryingHandlerBase,
            DnsHandler dnsHandler,
            LoggingHandlerBase loggingHandlerBase,
            CertificateHandler certificateHandler)
        {
            _config = config;

            _innerHandler = new HttpMessageHandlerStackBuilder()
                .AddDelegatingHandler(alternativeHostHandler)
                .AddDelegatingHandler(cancellingHandlerBase)
                .AddDelegatingHandler(unauthorizedResponseHandler)
                .AddDelegatingHandler(humanVerificationHandlerBase)
                .AddDelegatingHandler(outdatedAppHandler)
                .AddDelegatingHandler(retryingHandlerBase)
                .AddDelegatingHandler(dnsHandler)
                .AddDelegatingHandler(loggingHandlerBase)
                .AddLastHandler(certificateHandler)
                .Build();
        }

        public HttpClient GetApiHttpClientWithoutCache()
        {
            HttpClient client = GetApiHttpClientWithCache();
            client.DefaultRequestHeaders.ConnectionClose = true;
            return client;
        }

        public HttpClient GetApiHttpClientWithCache()
        {
            return new(_innerHandler) { BaseAddress = new Uri(_config.Urls.ApiUrl) };
        }
    }
}