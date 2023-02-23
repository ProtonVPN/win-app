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
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ApiLogs;
using ProtonVPN.Common.Networking;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.Contracts.Exceptions;

namespace ProtonVPN.Api.Handlers
{
    public class DnsHandler : DelegatingHandler
    {
        private readonly ILogger _logger;
        private readonly IDnsManager _dnsManager;

        public DnsHandler(ILogger logger, IDnsManager dnsManager)
        {
            _logger = logger;
            _dnsManager = dnsManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri.HostNameType == UriHostNameType.Dns)
            {
                return await SendRequestToDomainAsync(request, cancellationToken);
            }

            return await SendRequestAsync(request, cancellationToken);
        }

        private async Task<HttpResponseMessage> SendRequestToDomainAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            IList<IpAddress> ipAddresses = await _dnsManager.GetAsync(request.RequestUri.IdnHost, cancellationToken);
            if (!ipAddresses.IsNullOrEmpty())
            {
                for (int i = 0; i < ipAddresses.Count; i++)
                {
                    try
                    {
                        HttpResponseMessage httpResponseMessage = await SendRequestToIpAddressAsync(
                            ipAddresses[i], request, cancellationToken);
                        return httpResponseMessage;
                    }
                    catch
                    {
                        if (i + 1 == ipAddresses.Count)
                        {
                            throw;
                        }
                    }
                }
            }
            throw new DnsException($"No IP addresses to make the API request to '{request.RequestUri}'.");
        }

        private async Task<HttpResponseMessage> SendRequestToIpAddressAsync(IpAddress ipAddress,
            HttpRequestMessage request, CancellationToken token)
        {
            Uri oldRequestUri = request.RequestUri;
            SetRequestHost(request, ipAddress.ToString(), oldRequestUri);
            HttpResponseMessage httpResponseMessage;
            try
            {
                httpResponseMessage = await SendRequestAsync(request, token);
            }
            catch (Exception ex)
            {
                _logger.Error<ApiErrorLog>($"API request '{request.RequestUri}' failed.", ex);
                ResetRequestUri(request, oldRequestUri);
                throw;
            }
            return httpResponseMessage;
        }

        private void SetRequestHost(HttpRequestMessage request, string uriHost, Uri oldRequestUri)
        {
            UriBuilder uriBuilder = new(request.RequestUri) { Host = uriHost };
            request.Headers.Host = oldRequestUri.Host;
            request.RequestUri = uriBuilder.Uri;
        }

        private void ResetRequestUri(HttpRequestMessage request, Uri uri)
        {
            UriBuilder uriBuilder = new(uri) { Host = uri.Host };
            request.Headers.Host = uriBuilder.Host;
            request.RequestUri = uriBuilder.Uri;
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, CancellationToken token)
        {
            return await base.SendAsync(request, token);
        }
    }
}