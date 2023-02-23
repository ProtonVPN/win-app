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
using ProtonVPN.Api.Handlers.Retries;
using ProtonVPN.Api.Handlers.StackBuilders;
using ProtonVPN.Api.Handlers.TlsPinning;
using ProtonVPN.Common.Configuration;

namespace ProtonVPN.Api
{
    public class TokenHttpClientFactory : ITokenHttpClientFactory
    {
        private readonly IConfiguration _configuration;
        private readonly HttpMessageHandler _innerHandler;

        public TokenHttpClientFactory(IConfiguration configuration, 
            AlternativeHostHandler alternativeHostHandler,
            RetryingHandler retryingHandler,
            DnsHandler dnsHandler,
            LoggingHandlerBase loggingHandlerBase,
            CertificateHandler certificateHandler)
        {
            _configuration = configuration;

            _innerHandler = new HttpMessageHandlerStackBuilder()
                            .AddDelegatingHandler(alternativeHostHandler)
                            .AddDelegatingHandler(retryingHandler)
                            .AddDelegatingHandler(dnsHandler)
                            .AddDelegatingHandler(loggingHandlerBase)
                            .AddLastHandler(certificateHandler)
                            .Build();
        }

        public HttpClient GetTokenHttpClient()
        {
            return new HttpClient(_innerHandler) { BaseAddress = new Uri(_configuration.Urls.ApiUrl) };
        }
    }
}