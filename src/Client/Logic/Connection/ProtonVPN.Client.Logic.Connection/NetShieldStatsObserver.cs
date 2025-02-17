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
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.NetShield;

namespace ProtonVPN.Client.Logic.Connection;

public class NetShieldStatsObserver : PollingObserverBase,
    INetShieldStatsObserver,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<NetShieldStatisticIpcEntity>,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>,
    IEventMessageReceiver<ProfilesChangedMessage>
{
    private const int TIMER_INTERVAL_IN_SECONDS = 20;
    private const int MINIMUM_REQUEST_TIMEOUT_IN_SECONDS = 20;

    private readonly ILogger _logger;
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IConnectionManager _connectionManager;

    private readonly TimeSpan _requestTimeout;
    private readonly object _lock = new();

    private DateTime _nextRequestDateUtc = DateTime.MinValue;
    private bool _isMainWindowVisible;

    protected override TimeSpan PollingInterval => TimeSpan.FromSeconds(TIMER_INTERVAL_IN_SECONDS);

    public NetShieldStatsObserver(ILogger logger,
        IIssueReporter issuesIssueReporter,
        IVpnServiceCaller vpnServiceCaller,
        ISettings settings,
        IEventMessageSender eventMessageSender,
        IConfiguration config,
        IConnectionManager connectionManager)
        : base(logger, issuesIssueReporter)
    {
        _logger = logger;
        _vpnServiceCaller = vpnServiceCaller;
        _settings = settings;
        _eventMessageSender = eventMessageSender;
        _connectionManager = connectionManager;

        TimeSpan requestInterval = config.NetShieldStatisticRequestInterval;
        TimeSpan minimumRequestTimeout = TimeSpan.FromSeconds(MINIMUM_REQUEST_TIMEOUT_IN_SECONDS);
        _requestTimeout = TimeSpanExtensions.Max(requestInterval, minimumRequestTimeout);
        _logger.Info<AppLog>($"NetShield Stats - Request timeout set to {_requestTimeout}.");
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        InvalidateTimer();
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.IsNetShieldEnabled))
        {
            InvalidateTimer();
        }
    }

    public void Receive(NetShieldStatisticIpcEntity message)
    {
        _eventMessageSender.Send(new NetShieldStatsChangedMessage
        {
            NumOfMaliciousUrlsBlocked = message.NumOfMaliciousUrlsBlocked,
            NumOfAdvertisementUrlsBlocked = message.NumOfAdvertisementUrlsBlocked,
            NumOfTrackingUrlsBlocked = message.NumOfTrackingUrlsBlocked,
        });
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        _isMainWindowVisible = message.IsMainWindowVisible;

        InvalidateTimer();
    }

    public void Receive(ProfilesChangedMessage message)
    {
        if (_connectionManager.IsConnected)
        {
            InvalidateTimer();
        }
    }

    protected override async Task OnTriggerAsync()
    {
        DateTime utcNow = DateTime.UtcNow;

        if (CanRequestNetShieldStats() && _nextRequestDateUtc <= utcNow)
        {
            _nextRequestDateUtc = utcNow + _requestTimeout;

            _logger.Debug<AppLog>("NetShield Stats - Request made");
            await _vpnServiceCaller.RequestNetShieldStatsAsync();
        }
    }

    private bool CanRequestNetShieldStats()
    {
        return _isMainWindowVisible && _connectionManager.IsConnected && IsNetShieldEnabled();
    }

    private bool IsNetShieldEnabled()
    {
        return _connectionManager.CurrentConnectionIntent is IConnectionProfile profile
            ? profile.Settings.IsNetShieldEnabled
            : _settings.IsNetShieldEnabled;
    }

    private void InvalidateTimer()
    {
        lock (_lock)
        {
            if (CanRequestNetShieldStats())
            {
                StartTimerAndTriggerOnStart();
            }
            else
            {
                StopTimer();
            }
        }
    }
}