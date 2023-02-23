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

using System.Net.Http;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Handlers.Retries;
using ProtonVPN.Api.Handlers.StackBuilders;
using ProtonVPN.Api.Handlers.TlsPinning;
using ProtonVPN.Api.Handlers;
using ProtonVPN.Common.OS.Net.Http;

namespace ProtonVPN.Api
{
    public class FileDownloadHttpClientFactory : IFileDownloadHttpClientFactory
    {
        private readonly RetryingHandler _retryingHandler;
        private readonly DnsHandler _dnsHandler;
        private readonly LoggingHandlerBase _loggingHandlerBase;
        private readonly CertificateHandler _certificateHandler;
        private readonly IHttpClients _httpClients;
        private readonly IApiAppVersion _apiAppVersion;

        public FileDownloadHttpClientFactory(
            RetryingHandler retryingHandler,
            DnsHandler dnsHandler,
            LoggingHandlerBase loggingHandlerBase,
            CertificateHandler certificateHandler,
            IHttpClients httpClients,
            IApiAppVersion apiAppVersion)
        {
            _retryingHandler = retryingHandler;
            _dnsHandler = dnsHandler;
            _loggingHandlerBase = loggingHandlerBase;
            _certificateHandler = certificateHandler;
            _httpClients = httpClients;
            _apiAppVersion = apiAppVersion;
        }

        public IHttpClient GetFileDownloadHttpClient()
        {
            HttpMessageHandler innerHandler = new HttpMessageHandlerStackBuilder()
                .AddDelegatingHandler(_retryingHandler)
                .AddDelegatingHandler(_dnsHandler)
                .AddDelegatingHandler(_loggingHandlerBase)
                .AddLastHandler(_certificateHandler)
                .Build();

            return _httpClients.Client(innerHandler, _apiAppVersion.UserAgent());
        }
    }
}