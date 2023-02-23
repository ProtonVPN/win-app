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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.DnsLogs;
using ProtonVPN.Common.Networking;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.Contracts.Exceptions;
using ProtonVPN.Dns.HttpClients;

namespace ProtonVPN.Dns.Resolvers
{
    public abstract class DnsOverHttpsResolverBase : DnsResolverBase
    {
        private readonly TimeSpan _dnsOverHttpsPerProviderTimeout;
        private readonly IDnsOverHttpsProvidersManager _dnsOverHttpsProvidersManager;
        private readonly RecordType _recordType;
        private readonly IList<string> _providersUrl;
        private readonly HttpClient _httpClient;

        protected DnsOverHttpsResolverBase(IConfiguration configuration, ILogger logger,
            IHttpClientFactory httpClientFactory, IDnsOverHttpsProvidersManager dnsOverHttpsProvidersManager,
            RecordType recordType) : base(configuration, logger)
        {
            _dnsOverHttpsPerProviderTimeout = configuration.DnsOverHttpsPerProviderTimeout;
            _dnsOverHttpsProvidersManager = dnsOverHttpsProvidersManager;
            _recordType = recordType;
            _providersUrl = configuration.DoHProviders?.ToList() ?? new List<string>();
            _httpClient = httpClientFactory.Create();
            _httpClient.Timeout = configuration.DohClientTimeout;
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/dns-message"));
        }

        protected override async Task<DnsResponse> StartTasksAndWaitAnySuccessAsync(string host,
            CancellationToken cancellationToken)
        {
            DnsResponse dnsResponse = null;
            try
            {
                string message = GenerateBase64DnsMessage(host);
                List<Func<CancellationToken, Task<DnsResponse>>> resolveFuncs = new();
                foreach (string providerUrl in _providersUrl)
                {
                    DnsOverHttpsParallelHttpRequestConfiguration requestConfig = new()
                    {
                        Host = host,
                        Message = message,
                        ProviderUrl = providerUrl,
                    };
                    resolveFuncs.Add(ct => TryRequestAsync(requestConfig, ct));
                }
                dnsResponse = await WaitAnySuccessfulResolveAsync(resolveFuncs, DnsResolveTimeout, cancellationToken);
            }
            catch (Exception e)
            {
                Logger.Error<DnsErrorLog>("DNS over HTTPS failed during preparation.", e);
            }

            return dnsResponse;
        }

        private string GenerateBase64DnsMessage(string host)
        {
            byte[] bytes = GenerateDnsQueryMessageBytes(host);
            byte[] cleanBytes = bytes.TrimTrailingZeroBytes();

            return Convert.ToBase64String(cleanBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private byte[] GenerateDnsQueryMessageBytes(string host)
        {
            DnsMessage dnsMessage = new();
            DnsQuestion question = new(DomainName.Parse(host), _recordType, RecordClass.INet);
            dnsMessage.Questions.Add(question);
            dnsMessage.IsRecursionDesired = true;

            return EncodeDnsQueryMessageToBytes(dnsMessage);
        }

        private byte[] EncodeDnsQueryMessageToBytes(DnsMessage message)
        {
            MethodInfo m = message.GetType().GetMethod(
                "Encode",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(bool), typeof(byte[]).MakeByRefType() },
                null);

            object[] args = { false, null };
            m.Invoke(message, args);

            return args[1] as byte[];
        }

        private async Task<DnsResponse> TryRequestAsync(DnsOverHttpsParallelHttpRequestConfiguration config,
            CancellationToken cancellationToken)
        {
            Logger.Info<DnsResolveLog>($"Attempting to resolve host '{config.Host}' " +
                $"with DNS over HTTPS provider '{config.ProviderUrl}'.");
            DnsResponse dnsResponse = null;
            try
            {
                dnsResponse = await RequestAsync(config, cancellationToken);
            }
            catch (Exception e)
            {
                if (e.IsOrAnyInnerIsOfExceptionType<OperationCanceledException>())
                {
                    LogOperationCancelled($"The DNS over HTTPS provider '{config.ProviderUrl}' " +
                        $"was canceled when resolving host '{config.Host}'.");
                }
                else if (e is DnsException)
                {
                    Logger.Error<DnsErrorLog>($"The DNS over HTTPS provider '{config.ProviderUrl}' " +
                        $"failed when resolving host '{config.Host}'. Reason: {e.Message}");
                }
                else
                {
                    Logger.Error<DnsErrorLog>($"The DNS over HTTPS provider '{config.ProviderUrl}' " +
                        $"failed when resolving host '{config.Host}'.", e);
                }
            }

            return dnsResponse;
        }

        private async Task<DnsResponse> RequestAsync(DnsOverHttpsParallelHttpRequestConfiguration config,
            CancellationToken cancellationToken)
        {
            IList<IpAddress> ipAddresses = await GetDoHProviderIpAddressesAsync(config, cancellationToken);
            List<Func<CancellationToken, Task<DnsResponse>>> resolveFuncs = new();
            foreach (IpAddress ipAddress in ipAddresses)
            {
                resolveFuncs.Add(ct => ResolveForHostAsync(ipAddress, config, cancellationToken));
            }
            return await WaitAnySuccessfulResolveAsync(resolveFuncs, _dnsOverHttpsPerProviderTimeout, cancellationToken);
        }

        private async Task<IList<IpAddress>> GetDoHProviderIpAddressesAsync(
            DnsOverHttpsParallelHttpRequestConfiguration config, CancellationToken cancellationToken)
        {
            UriBuilder providerUriBuilder = new(config.ProviderUrl);
            IList<IpAddress> ipAddresses = await _dnsOverHttpsProvidersManager.GetAsync(providerUriBuilder.Host, cancellationToken);
            if (ipAddresses.IsNullOrEmpty())
            {
                throw new DnsException($"No IP addresses were found for host '{config.ProviderUrl}'.");
            }

            return ipAddresses;
        }

        private async Task<DnsResponse> ResolveForHostAsync(IpAddress ipAddress,
            DnsOverHttpsParallelHttpRequestConfiguration config, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = CreateRequestMessage(ipAddress, config);
            HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            ThrowIfHttpResponseMessageIsNotSuccess(response, config);
            byte[] dnsResponseMessageBytes = await response.Content.ReadAsByteArrayAsync();
            Logger.Info<DnsResponseLog>($"Successfully resolved host '{config.Host}' " +
                $"with DNS over HTTPS provider '{config.ProviderUrl}'.");
            DnsMessage dnsMessage = DnsMessage.Parse(dnsResponseMessageBytes);
            return ParseDnsResponseMessage(config, dnsMessage);
        }

        private void ThrowIfHttpResponseMessageIsNotSuccess(HttpResponseMessage response,
            DnsOverHttpsParallelHttpRequestConfiguration config)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpException((int)response.StatusCode, $"Unexpected HTTP response '{response.StatusCode}' " +
                    $"when resolving for host '{config.Host}' with DNS over HTTPS provider '{config.ProviderUrl}'.");
            }
        }

        private HttpRequestMessage CreateRequestMessage(IpAddress ipAddress,
            DnsOverHttpsParallelHttpRequestConfiguration config)
        {
            UriBuilder dohProviderUriBuilder = new UriBuilder(config.ProviderUrl);
            HttpRequestMessage requestMessage = new();
            UriBuilder uriBuilder = new(config.ProviderUrl)
            {
                Host = ipAddress.ToString(),
                Path = $"{dohProviderUriBuilder.Path}",
                Query = $"dns={config.Message}",
            };
            requestMessage.Headers.Host = dohProviderUriBuilder.Host;
            requestMessage.RequestUri = uriBuilder.Uri;
            requestMessage.Method = HttpMethod.Get;

            return requestMessage;
        }

        protected abstract DnsResponse ParseDnsResponseMessage(DnsOverHttpsParallelHttpRequestConfiguration config,
            DnsMessage dnsResponseMessage);
    }
}
