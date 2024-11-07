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
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Polly.Timeout;
using ProtonVPN.Api.Contracts.Exceptions;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Dns.Contracts.AlternativeRouting;
using ProtonVPN.Dns.Contracts.Exceptions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ApiLogs;

namespace ProtonVPN.Api.Handlers;

public class AlternativeHostHandler : DelegatingHandler,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<GuestHoleStatusChangedMessage>
{
    public const string API_PING_TEST_PATH = "tests/ping";
    private const string NO_ALTERNATIVE_HOSTS_ERROR_MESSAGE = "No alternative hosts exist. Alternative routing failed.";
    private const string ALL_ALTERNATIVE_HOSTS_FAILED_ERROR_MESSAGE = "All alternative hosts failed. Alternative routing failed.";

    private readonly ILogger _logger;
    private readonly IDnsManager _dnsManager;
    private readonly IAlternativeRoutingHostGenerator _alternativeRoutingHostGenerator;
    private readonly IAlternativeHostsManager _alternativeHostsManager;
    private readonly ISettings _settings;
    private readonly string _defaultApiHost;
    private readonly TimeSpan _alternativeRoutingCheckInterval;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private bool _isGuestHoleActive;
    private bool _isDisconnected = true;
    private bool _isUserLoggedIn;
    private DateTime? _lastAlternativeRoutingCheckDateUtc;

    public AlternativeHostHandler(
        ILogger logger,
        IDnsManager dnsManager,
        IAlternativeRoutingHostGenerator alternativeRoutingHostGenerator,
        IAlternativeHostsManager alternativeHostsManager,
        ISettings settings,
        IConfiguration config)
    {
        _logger = logger;
        _dnsManager = dnsManager;
        _alternativeRoutingHostGenerator = alternativeRoutingHostGenerator;
        _alternativeHostsManager = alternativeHostsManager;
        _settings = settings;
        _defaultApiHost = new Uri(config.Urls.ApiUrl).Host;
        _alternativeRoutingCheckInterval = config.AlternativeRoutingCheckInterval;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        _isDisconnected = message.ConnectionStatus == ConnectionStatus.Disconnected;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (IsAlternativeRoutingEnabled())
        {
            if (IsAlternativeRoutingAllowed())
            {
                if (IsLastAlternativeRoutingCheckDateNullOrTooOld())
                {
                    bool isApiAvailable = await IsApiAvailableAsync(request, cancellationToken);
                    if (isApiAvailable)
                    {
                        await DisableAlternativeRoutingAsync();
                    }
                    else
                    {
                        return await SendRequestWithActiveAlternativeHostAsync(request, cancellationToken);
                    }
                }
                else
                {
                    return await SendRequestWithActiveAlternativeHostAsync(request, cancellationToken);
                }
            }
            else
            {
                await DisableAlternativeRoutingAsync();
            }
        }

        return await TrySendRequestAsync(request, cancellationToken);
    }

    private bool IsAlternativeRoutingEnabled()
    {
        return !_settings.ActiveAlternativeApiBaseUrl.IsNullOrEmpty();
    }

    private async Task<HttpResponseMessage> SendRequestWithActiveAlternativeHostAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string alternativeHost = _settings.ActiveAlternativeApiBaseUrl;
        if (!alternativeHost.IsNullOrEmpty())
        {
            try
            {
                HttpResponseMessage httpResponseMessage = await SendRequestWithActiveAlternativeHostAsync(
                    alternativeHost, request, cancellationToken);
                return httpResponseMessage;
            }
            catch (Exception ex)
            {
                _logger.Error<ApiErrorLog>($"Alternative host '{alternativeHost}' failed.", ex);
            }
        }

        await DisableAlternativeRoutingAsync();
        return await TrySendRequestAsync(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendRequestWithActiveAlternativeHostAsync(string alternativeHost,
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        IList<IpAddress> alternativeHostIpAddresses = await _dnsManager.GetAsync(alternativeHost, cancellationToken);
        ThrowIfAlternativeHostHasNoIpAddresses(alternativeHost, alternativeHostIpAddresses);
        foreach (IpAddress alternativeHostIpAddress in alternativeHostIpAddresses)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = await SendRequestToAlternativeHostAsync(
                    alternativeHostIpAddress, alternativeHost, request, cancellationToken);
                return httpResponseMessage;
            }
            catch (Exception ex)
            {
                _logger.Error<ApiErrorLog>($"Alternative host '{alternativeHost}' with IP address " +
                    $"'{alternativeHostIpAddress}' failed.", ex);
            }
        }
        throw new AlternativeRoutingException($"Alternative host '{alternativeHost}' failed.");
    }

    private bool IsAlternativeRoutingAllowed()
    {
        return _isDisconnected && !_isGuestHoleActive && IsAlternativeRoutingSettingEnabled();
    }

    private bool IsAlternativeRoutingSettingEnabled()
    {
        return _settings.IsAlternativeRoutingEnabled;
    }

    private async Task<HttpResponseMessage> TrySendRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage httpResponseMessage;
        try
        {
            httpResponseMessage = await SendRequestAsync(request, cancellationToken);
        }
        catch (Exception ex) when (IsAlternativeRoutingAllowed() && IsPotentialBlockingException(ex))
        {
            bool isApiAvailable = await IsApiAvailableAsync(request, cancellationToken);
            if (isApiAvailable)
            {
                httpResponseMessage = await SendOriginalRequestAndRetryWithAlternativeRoutingAsync(request, cancellationToken);
            }
            else
            {
                httpResponseMessage = await SendRequestWithAlternativeRoutingAsync(request, cancellationToken);
            }
        }
        return httpResponseMessage;
    }

    private async Task<HttpResponseMessage> SendOriginalRequestAndRetryWithAlternativeRoutingAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage httpResponseMessage;
        try
        {
            httpResponseMessage = await SendRequestAsync(request, cancellationToken);
        }
        catch (Exception ex) when (IsAlternativeRoutingAllowed() && IsPotentialBlockingException(ex))
        {
            httpResponseMessage = await SendRequestWithAlternativeRoutingAsync(request, cancellationToken);
        }
        return httpResponseMessage;
    }

    public bool IsPotentialBlockingException(Exception ex)
    {
        return ex is TimeoutException or TimeoutRejectedException or DnsException
               || ex.GetBaseException() is AuthenticationException;
    }

    private bool IsLastAlternativeRoutingCheckDateNullOrTooOld()
    {
        return _lastAlternativeRoutingCheckDateUtc is null ||
               _lastAlternativeRoutingCheckDateUtc < (DateTime.UtcNow - _alternativeRoutingCheckInterval);
    }

    private async Task<bool> IsApiAvailableAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.Info<ApiLog>("Checking for API availability.");
        IList<IpAddress> defaultApiIpAddresses = await _dnsManager.ResolveWithoutCacheAsync(_defaultApiHost, cancellationToken);
        if (defaultApiIpAddresses.IsNullOrEmpty())
        {
            defaultApiIpAddresses = _dnsManager.GetFromCache(_defaultApiHost);
            if (defaultApiIpAddresses.IsNullOrEmpty())
            {
                await UpdateLastAlternativeRoutingCheckDateAsync();
                _logger.Warn<ApiErrorLog>("The API is unavailable due to a failure in the DNS step. " +
                    "No IP addresses were able to be resolved or fetched from cache.");
                return false;
            }
        }

        bool isApiPingSuccessful = await SendApiPingRequestAsync(request, cancellationToken);
        await UpdateLastAlternativeRoutingCheckDateAsync();
        LogApiAvailabilityCheckResult(isApiPingSuccessful);
        return isApiPingSuccessful;
    }

    private void LogApiAvailabilityCheckResult(bool isApiPingSuccessful)
    {
        if (isApiPingSuccessful)
        {
            _logger.Info<ApiLog>("The API availability check was successful.");
        }
        else
        {
            _logger.Warn<ApiErrorLog>("The API is unavailable due to a request failure.");
        }
    }

    private async Task UpdateLastAlternativeRoutingCheckDateAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _lastAlternativeRoutingCheckDateUtc = DateTime.UtcNow;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<bool> SendApiPingRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage result = await base.SendAsync(CreateApiPingRequest(request), cancellationToken);
            return result.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private HttpRequestMessage CreateApiPingRequest(HttpRequestMessage request)
    {
        HttpRequestMessage pingRequest = new();
        UriBuilder uriBuilder = new(request.RequestUri)
        {
            Host = _defaultApiHost,
            Path = API_PING_TEST_PATH,
            Query = string.Empty,
        };
        pingRequest.Headers.Host = _defaultApiHost;
        pingRequest.RequestUri = uriBuilder.Uri;
        pingRequest.Method = HttpMethod.Get;

        return pingRequest;
    }

    private async Task DisableAlternativeRoutingAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _lastAlternativeRoutingCheckDateUtc = null;
            _settings.ActiveAlternativeApiBaseUrl = null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendRequestWithAlternativeRoutingAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string alternativeRoutingHost = _alternativeRoutingHostGenerator.Generate(_isUserLoggedIn ? _settings.UniqueSessionId : null);
        IList<string> alternativeHosts = await _alternativeHostsManager.GetAsync(alternativeRoutingHost, cancellationToken);
        ThrowIfNoAlternativeHostsExist(alternativeHosts);

        HttpResponseMessage httpResponseMessage = null;
        string chosenAlternativeHost = null;
        foreach (string alternativeHost in alternativeHosts)
        {
            try
            {
                httpResponseMessage = await SendRequestWithAlternativeHostAsync(alternativeHost, request, cancellationToken);
                chosenAlternativeHost = alternativeHost;
                await EnableAlternativeRoutingAsync(chosenAlternativeHost);
                break;
            }
            catch (Exception ex)
            {
                _logger.Error<ApiErrorLog>($"Alternative host '{alternativeHost}' failed.", ex);
            }
        }

        ThrowIfAllAlternativeHostsFailed(chosenAlternativeHost);
        return httpResponseMessage;
    }

    private void ThrowIfNoAlternativeHostsExist(IList<string> alternativeHosts)
    {
        if (alternativeHosts.IsNullOrEmpty())
        {
            _logger.Error<ApiErrorLog>(NO_ALTERNATIVE_HOSTS_ERROR_MESSAGE);
            throw new AlternativeRoutingException(NO_ALTERNATIVE_HOSTS_ERROR_MESSAGE);
        }
    }

    private void ThrowIfAllAlternativeHostsFailed(string chosenAlternativeHost)
    {
        if (chosenAlternativeHost.IsNullOrEmpty())
        {
            _logger.Error<ApiErrorLog>(ALL_ALTERNATIVE_HOSTS_FAILED_ERROR_MESSAGE);
            throw new AlternativeRoutingException(ALL_ALTERNATIVE_HOSTS_FAILED_ERROR_MESSAGE);
        }
    }

    private async Task<HttpResponseMessage> SendRequestWithAlternativeHostAsync(string alternativeHost,
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        IList<IpAddress> alternativeHostIpAddresses = await _dnsManager.GetAsync(alternativeHost, cancellationToken);
        ThrowIfAlternativeHostHasNoIpAddresses(alternativeHost, alternativeHostIpAddresses);
        foreach (IpAddress alternativeHostIpAddress in alternativeHostIpAddresses)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = await SendRequestToAlternativeHostAsync(
                    alternativeHostIpAddress, alternativeHost, request, cancellationToken);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return httpResponseMessage;
                }
            }
            catch (Exception ex)
            {
                _logger.Error<ApiErrorLog>($"Alternative host '{alternativeHost}' with IP address " +
                    $"'{alternativeHostIpAddress}' failed.", ex);
            }
        }
        throw new AlternativeRoutingException($"Alternative host '{alternativeHost}' failed.");
    }

    private async Task EnableAlternativeRoutingAsync(string alternativeHost)
    {
        await _semaphore.WaitAsync();
        try
        {
            _lastAlternativeRoutingCheckDateUtc = DateTime.UtcNow;
            _settings.ActiveAlternativeApiBaseUrl = alternativeHost;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void ThrowIfAlternativeHostHasNoIpAddresses(string alternativeHost, IList<IpAddress> alternativeHostIpAddresses)
    {
        if (alternativeHostIpAddresses.IsNullOrEmpty())
        {
            string errorMessage = $"No IP addresses were found for alternative host '{alternativeHost}'.";
            _logger.Error<ApiErrorLog>(errorMessage);
            throw new AlternativeRoutingException(errorMessage);
        }
    }

    private async Task<HttpResponseMessage> SendRequestToAlternativeHostAsync(IpAddress ipAddress,
        string alternativeHost, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string oldUriHost = request.RequestUri.Host;
        string oldHeaderHost = request.Headers.Host;
        SetRequestHost(request, uriHost: ipAddress.ToString(), headerHost: alternativeHost);
        HttpResponseMessage httpResponseMessage;
        try
        {
            httpResponseMessage = await SendRequestAsync(request, cancellationToken);
        }
        finally
        {
            SetRequestHost(request, uriHost: oldUriHost, headerHost: oldHeaderHost);
        }
        return httpResponseMessage;
    }

    private void SetRequestHost(HttpRequestMessage request, string uriHost, string headerHost)
    {
        UriBuilder uriBuilder = new(request.RequestUri) { Host = uriHost };
        request.Headers.Host = headerHost;
        request.RequestUri = uriBuilder.Uri;
    }

    public void Receive(LoggedInMessage message)
    {
        _isUserLoggedIn = true;
    }

    public void Receive(LoggedOutMessage message)
    {
        _isUserLoggedIn = false;
    }

    public void Receive(GuestHoleStatusChangedMessage message)
    {
        _isGuestHoleActive = message.IsActive;
    }
}