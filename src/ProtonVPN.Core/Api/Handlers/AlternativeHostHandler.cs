/*
 * Copyright (c) 2020 Proton Technologies AG
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
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.OS.Net.DoH;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Api.Handlers
{
    public class AlternativeHostHandler : DelegatingHandler
    {
        private readonly DohClients _dohClients;
        private readonly MainHostname _mainHostname;
        private readonly IAppSettings _appSettings;
        private readonly SingleAction _fetchProxies;
        private readonly GuestHoleState _guestHoleState;

        private const int HoursToUseProxy = 24;
        private string _activeBackendHost;
        private readonly string _apiHost;

        public AlternativeHostHandler(
            DohClients dohClients,
            MainHostname mainHostname,
            IAppSettings appSettings,
            GuestHoleState guestHoleState,
            string apiHost)
        {
            _guestHoleState = guestHoleState;
            _mainHostname = mainHostname;
            _dohClients = dohClients;
            _appSettings = appSettings;
            _apiHost = apiHost;
            _activeBackendHost = apiHost;
            _fetchProxies = new SingleAction(FetchProxies);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (!_appSettings.DoHEnabled || _guestHoleState.Active)
            {
                ResetBackendHost();
                return await SendInternalAsync(request, token);
            }

            if (ProxyActivated())
            {
                try
                {
                    _activeBackendHost = _appSettings.ActiveAlternativeApiBaseUrl;
                    return await SendInternalAsync(request, token);
                }
                catch (Exception e) when (e.IsPotentialBlocking())
                {
                    ResetBackendHost();
                    return await SendAsync(request, token);
                }
            }

            try
            {
                return await SendInternalAsync(request, token);
            }
            catch (Exception e) when (e.IsPotentialBlocking())
            {
                await _fetchProxies.Run();

                if (_appSettings.AlternativeApiBaseUrls == null)
                {
                    throw;
                }

                if (await IsApiReachable(request, token))
                {
                    try
                    {
                        return await SendInternalAsync(request, token);
                    }
                    catch (Exception ex) when (ex.IsPotentialBlocking())
                    {
                        throw;
                    }
                }

                var alternativeResult = await TryAlternativeHosts(request, token);
                if (alternativeResult.Success)
                {
                    return alternativeResult.Value;
                }

                throw;
            }
        }

        private async Task<bool> IsApiReachable(HttpRequestMessage request, CancellationToken token)
        {
            try
            {
                var result = await base.SendAsync(GetPingRequest(request), token);
                return result.IsSuccessStatusCode;
            }
            catch (Exception e) when (e.IsPotentialBlocking())
            {
                return false;
            }
        }

        private HttpRequestMessage GetPingRequest(HttpRequestMessage request)
        {
            var pingRequest = new HttpRequestMessage();
            var uriBuilder = new UriBuilder(request.RequestUri)
            {
                Host = _activeBackendHost,
                Path = "tests/ping"
            };
            pingRequest.Headers.Host = uriBuilder.Host;
            pingRequest.RequestUri = uriBuilder.Uri;
            pingRequest.Method = HttpMethod.Get;

            return pingRequest;
        }

        private void ResetBackendHost()
        {
            _appSettings.ActiveAlternativeApiBaseUrl = string.Empty;
            _activeBackendHost = _apiHost;
        }

        private async Task<Result<HttpResponseMessage>> TryAlternativeHosts(HttpRequestMessage request, CancellationToken token)
        {
            foreach (var host in _appSettings.AlternativeApiBaseUrls)
            {
                try
                {
                    _activeBackendHost = host;
                    var result = await SendInternalAsync(request, token);
                    if (result.IsSuccessStatusCode)
                    {
                        _appSettings.ActiveAlternativeApiBaseUrl = host;
                    }

                    return Result.Ok(result);
                }
                catch (Exception ex) when (ex.IsApiCommunicationException())
                {
                    //Ignore
                }
            }

            ResetBackendHost();

            return Result.Fail<HttpResponseMessage>();
        }

        private async Task<HttpResponseMessage> SendInternalAsync(HttpRequestMessage request, CancellationToken token)
        {
            return await base.SendAsync(GetRequest(request), token);
        }

        private bool ProxyActivated()
        {
            return _appSettings.DoHEnabled &&
                   DateTime.Now.Subtract(_appSettings.LastPrimaryApiFail).TotalHours < HoursToUseProxy &&
                   !string.IsNullOrEmpty(_appSettings.ActiveAlternativeApiBaseUrl);
        }

        private HttpRequestMessage GetRequest(HttpRequestMessage request)
        {
            var uriBuilder = new UriBuilder(request.RequestUri) { Host = _activeBackendHost };
            request.Headers.Host = uriBuilder.Host;
            request.RequestUri = uriBuilder.Uri;

            return request;
        }

        private async Task FetchProxies()
        {
            _appSettings.LastPrimaryApiFail = DateTime.Now;

            var clients = _dohClients.Get();
            foreach (var dohClient in clients)
            {
                try
                {
                    var alternativeHosts = await dohClient.ResolveTxtAsync(_mainHostname.Value());
                    if (alternativeHosts.Count > 0)
                    {
                        _appSettings.AlternativeApiBaseUrls = GetAlternativeApiBaseUrls(alternativeHosts);
                        return;
                    }
                }
                catch (Exception e) when (e.IsPotentialBlocking() || e.IsApiCommunicationException())
                {
                    //Ignore
                }
            }
        }

        private StringCollection GetAlternativeApiBaseUrls(List<string> list)
        {
            var collection = new StringCollection();
            foreach (var element in list)
            {
                collection.Add(element);
            }

            return collection;
        }
    }
}
