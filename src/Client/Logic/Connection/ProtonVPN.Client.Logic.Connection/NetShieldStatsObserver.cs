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

using ProtonVPN.Client.Common.Observers;
using ProtonVPN.Client.Contracts;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;

namespace ProtonVPN.Client.Logic.Connection;

public class NetShieldStatsObserver : PollingObserverBase,
    INetShieldStatsObserver,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<SettingChangedMessage>, 
    IEventMessageReceiver<NetShieldStatisticIpcEntity>,
    IEventMessageReceiver<WindowStateChangeMessage>
{
    private const int TIMER_INTERVAL_IN_SECONDS = 20;
    private const int MINIMUM_REQUEST_TIMEOUT_IN_SECONDS = 20;

    private readonly ILogger _logger;
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;

    private readonly TimeSpan _requestTimeout;
    private readonly object _lock = new();

    protected override TimeSpan PollingInterval => TimeSpan.FromSeconds(TIMER_INTERVAL_IN_SECONDS);

    private bool _isConnected;
    private bool _isMainAppWindowFocused = true;
    private DateTime _nextRequestDateUtc = DateTime.MinValue;

    public NetShieldStatsObserver(ILogger logger,
        IIssueReporter issuesIssueReporter,
        IVpnServiceCaller vpnServiceCaller,
        ISettings settings,
        IEventMessageSender eventMessageSender,
        IConfiguration config) : base(logger, issuesIssueReporter)
    {
        _logger = logger;
        _vpnServiceCaller = vpnServiceCaller;
        _settings = settings;
        _eventMessageSender = eventMessageSender;

        TimeSpan requestInterval = config.NetShieldStatisticRequestInterval;
        TimeSpan minimumRequestTimeout = TimeSpan.FromSeconds(MINIMUM_REQUEST_TIMEOUT_IN_SECONDS);
        _requestTimeout = requestInterval > minimumRequestTimeout ? requestInterval : minimumRequestTimeout;
        _logger.Info<AppLog>($"NetShield Stats - Request timeout set to {_requestTimeout}.");
    }

    protected override async Task OnTriggerAsync()
    {
        RequestIfAllowed();
    }

    private void RequestIfAllowed()
    {
        bool isToRequest;
        lock (_lock)
        {
            isToRequest = CanRequestNetShieldStats() && _nextRequestDateUtc <= DateTime.UtcNow;
            if (isToRequest)
            {
                SetNextRequestDateUtc();
            }
        }

        if (isToRequest)
        {
            _logger.Debug<AppLog>("NetShield Stats - Request made");
            _vpnServiceCaller.RequestNetShieldStatsAsync();
        }
    }

    private bool CanRequestNetShieldStats()
    {
        return _settings.IsNetShieldEnabled && _isConnected && _isMainAppWindowFocused;
    }

    private void SetNextRequestDateUtc()
    {
        _nextRequestDateUtc = DateTime.UtcNow + _requestTimeout;
    }

    public void Receive(ConnectionStatusChanged message)
    {
        lock (_lock)
        {
            if (!_isConnected && message.ConnectionStatus == ConnectionStatus.Connected)
            {
                _logger.Debug<AppLog>("NetShield Stats - Connection established, resetting next request date");
                SetNextRequestDateUtc();
            }

            _isConnected = message.ConnectionStatus == ConnectionStatus.Connected;
            SwitchTimerIfNeeded();
        }
    }

    private void SwitchTimerIfNeeded()
    {
        bool canRequestNetShieldStats = CanRequestNetShieldStats();
        if (canRequestNetShieldStats && !IsTimerEnabled)
        {
            StartTimer();
        }
        else if (!canRequestNetShieldStats && IsTimerEnabled)
        {
            StopTimer();
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.IsNetShieldEnabled))
        {
            lock (_lock)
            {
                SwitchTimerIfNeeded();
            }
        }
    }

    public void Receive(WindowStateChangeMessage message)
    {
        bool isToRequestImmediately;
        lock (_lock)
        {
            isToRequestImmediately = !_isMainAppWindowFocused && message.IsActive;
            _isMainAppWindowFocused = message.IsActive;
            SwitchTimerIfNeeded();
        }
        if (isToRequestImmediately)
        {
            RequestIfAllowed();
        }
    }

    public void Receive(NetShieldStatisticIpcEntity message)
    {
        _eventMessageSender.Send(new NetShieldStatsChanged
        {
            NumOfMaliciousUrlsBlocked = message.NumOfMaliciousUrlsBlocked,
            NumOfAdvertisementUrlsBlocked = message.NumOfAdvertisementUrlsBlocked,
            NumOfTrackingUrlsBlocked = message.NumOfTrackingUrlsBlocked,
        });
    }
}