/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.Client.Common.Observers;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Common.Legacy.OS.Net.Http;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Connection;

public class P2PTrafficObserver : PollingObserverBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private const string P2P_TRAFFIC_INDICATOR = "<!--P2P_WARNING-->";

    private readonly IConfiguration _configuration;
    private readonly IConnectionManager _connectionManager;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IHttpClient _httpClient;

    public P2PTrafficObserver(
        IConfiguration configuration,
        IConnectionManager connectionManager,
        IUrlsBrowser urlsBrowser,
        IEventMessageSender eventMessageSender,
        IHttpClients httpClients,
        ILogger logger,
        IIssueReporter issueReporter) : base(logger, issueReporter)
    {
        _configuration = configuration;
        _connectionManager = connectionManager;
        _urlsBrowser = urlsBrowser;
        _eventMessageSender = eventMessageSender;
        _httpClient = httpClients.Client(new HttpClientHandler
        {
            UseCookies = false
        });
    }

    protected override TimeSpan PollingInterval => _configuration.P2PTrafficDetectionInterval;

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (_connectionManager.IsConnected)
        {
            StartTimer();
        }
        else
        {
            StopTimer();
        }
    }

    protected override async Task OnTriggerAsync()
    {
        if (!_connectionManager.IsConnected)
        {
            return;
        }

        string? content = await GetResponseAsync();
        if (content is not null && content.Contains(P2P_TRAFFIC_INDICATOR))
        {
            Logger.Info<AppLog>("P2P traffic detected.");
            StopTimer();
            _eventMessageSender.Send<P2PTrafficDetectedMessage>();
        }
    }

    private async Task<string?> GetResponseAsync()
    {
        try
        {
            using (IHttpResponseMessage response = await _httpClient.GetAsync(_urlsBrowser.P2PStatusPage))
            {
                return response.IsSuccessStatusCode
                    ? await response.Content.ReadAsStringAsync()
                    : null;
            }
        }
        catch (Exception e)
        {
            Logger.Error<AppLog>("Failed to check for P2P traffic.", e);
            return null;
        }
    }
}