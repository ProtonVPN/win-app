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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ApiLogs;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.OS.Net.DoH;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Api.Handlers
{
    public class AlternativeHostHandler : DelegatingHandler, ILoggedInAware, ILogoutAware
    {
        private readonly ILogger _logger;
        private readonly DohClients _dohClients;
        private readonly MainHostname _mainHostname;
        private readonly IAppSettings _appSettings;
        private readonly GuestHoleState _guestHoleState;
        private readonly ITokenStorage _tokenStorage;
        private readonly SingleAction _fetchProxies;

        private const int MAX_HOURS_WITH_PROXY = 24;
        private string _activeBackendHost;
        private readonly string _apiHost;
        private bool _isDisconnected;
        private bool _isUserLoggedIn;

        public AlternativeHostHandler(
            CancellingHandlerBase cancellingHandler,
            ILogger logger,
            DohClients dohClients,
            MainHostname mainHostname,
            IAppSettings appSettings,
            GuestHoleState guestHoleState,
            ITokenStorage tokenStorage,
            Config config)
        {
            _logger = logger;
            _dohClients = dohClients;
            _mainHostname = mainHostname;
            _appSettings = appSettings;
            _guestHoleState = guestHoleState;
            _tokenStorage = tokenStorage;
            _apiHost = new Uri(config.Urls.ApiUrl).Host;
            _activeBackendHost = _apiHost;
            _fetchProxies = new SingleAction(FetchProxies);

            InnerHandler = cancellingHandler;
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _isDisconnected = e.State.Status == VpnStatus.Disconnected;
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
                    _logger.Info<ApiLog>($"Sending request using {_activeBackendHost}");

                    return await SendInternalAsync(request, token);
                }
                catch (Exception e) when (e.IsPotentialBlocking())
                {
                    _logger.Info<ApiErrorLog>($"Request failed while DoH active. Host: {_activeBackendHost}");

                    ResetBackendHost();
                    return await SendAsync(request, token);
                }
            }

            try
            {
                return await SendInternalAsync(request, token);
            }
            catch (Exception e) when (_isDisconnected && e.IsPotentialBlocking())
            {
                _logger.Info<ApiErrorLog>("Request failed due to potentially not reachable api.");

                await _fetchProxies.Run();

                if (_appSettings.AlternativeApiBaseUrls.Count == 0)
                {
                    throw;
                }

                if (await IsApiReachable(request, token))
                {
                    _logger.Info<ApiLog>("Ping success, retrying original request.");

                    try
                    {
                        return await SendInternalAsync(request, token);
                    }
                    catch (Exception ex) when (ex.IsPotentialBlocking())
                    {
                        throw;
                    }
                }

                Result<HttpResponseMessage> alternativeResult = await TryAlternativeHosts(request, token);
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
                HttpResponseMessage result = await base.SendAsync(GetPingRequest(request), token);
                return result.IsSuccessStatusCode;
            }
            catch (Exception e) when (e.IsPotentialBlocking())
            {
                return false;
            }
        }

        private HttpRequestMessage GetPingRequest(HttpRequestMessage request)
        {
            HttpRequestMessage pingRequest = new();
            UriBuilder uriBuilder = new(request.RequestUri)
            {
                Host = _activeBackendHost,
                Path = "tests/ping",
                Query = string.Empty,
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
            foreach (string host in _appSettings.AlternativeApiBaseUrls)
            {
                try
                {
                    _activeBackendHost = host;
                    HttpResponseMessage result = await SendInternalAsync(request, token);
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
                   _isDisconnected &&
                   DateTime.Now.Subtract(_appSettings.LastPrimaryApiFail).TotalHours < MAX_HOURS_WITH_PROXY &&
                   !string.IsNullOrEmpty(_appSettings.ActiveAlternativeApiBaseUrl);
        }

        private HttpRequestMessage GetRequest(HttpRequestMessage request)
        {
            UriBuilder uriBuilder = new(request.RequestUri) { Host = _activeBackendHost };
            request.Headers.Host = uriBuilder.Host;
            request.RequestUri = uriBuilder.Uri;

            return request;
        }

        private async Task FetchProxies()
        {
            _appSettings.LastPrimaryApiFail = DateTime.Now;
            _appSettings.AlternativeApiBaseUrls = new StringCollection();
            ResetBackendHost();

            List<Client> clients = _dohClients.Get();
            foreach (Client dohClient in clients)
            {
                try
                {
                    string host = _mainHostname.Value(_isUserLoggedIn ? _tokenStorage.Uid : string.Empty);
                    _logger.Info<AppLog>($"Resolving alternative hosts from {host}");
                    List<string> alternativeHosts = await dohClient.ResolveTxtAsync(host);
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
            StringCollection collection = new();
            foreach (string element in list)
            {
                collection.Add(element);
            }

            return collection;
        }

        public void OnUserLoggedIn()
        {
            _isUserLoggedIn = true;
        }

        public void OnUserLoggedOut()
        {
            _isUserLoggedIn = false;
        }
    }
}