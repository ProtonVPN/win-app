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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Connection;

public class VpnStatePollingObserver : PollingObserverBase,
    IVpnStatePollingObserver,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IConfiguration _configuration;
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly IConnectionManager _connectionManager;

    private readonly object _timerLock = new();

    // No jitter is needed for IPC (inter-process communication)
    private TimeSpan? _pollingInterval;
    protected override TimeSpan PollingInterval => _pollingInterval ?? _configuration.ServiceCheckInterval;

    public VpnStatePollingObserver(ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IConfiguration configuration,
        IVpnServiceCaller vpnServiceCaller,
        IConnectionManager connectionManager)
        : base(logger, issueReporter)
    {
        _settings = settings;
        _configuration = configuration;
        _vpnServiceCaller = vpnServiceCaller;
        _connectionManager = connectionManager;
    }

    protected override async Task OnTriggerAsync()
    {
        Logger.Debug<AppLog>("Requesting VPN state refresh");

        await _vpnServiceCaller.RepeatStateAsync();

        bool isPortForwardingEnabled = _connectionManager.CurrentConnectionIntent is IConnectionProfile profile
            ? profile.Settings.IsPortForwardingEnabled
            : _settings.IsPortForwardingEnabled;

        if (isPortForwardingEnabled)
        {
            await _vpnServiceCaller.RepeatPortForwardingStateAsync();
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        lock (_timerLock)
        {
            if (IsTimerEnabled)
            {
                SetAndRestartTimer(message.ConnectionStatus);
            }
        }
    }

    private void SetAndRestartTimer(ConnectionStatus connectionStatus)
    {
        TimeSpan newPollingInterval = connectionStatus == ConnectionStatus.Connecting
            ? _configuration.VpnStatePollingInterval
            : _configuration.ServiceCheckInterval;

        if (_pollingInterval != newPollingInterval || !IsTimerEnabled)
        {
            StopTimer();
            _pollingInterval = newPollingInterval;
            if (connectionStatus == ConnectionStatus.Connecting)
            {
                Logger.Info<AppLog>($"Starting VPN state refresh timer '{PollingInterval}'");
                StartTimer();
            }
            else
            {
                Logger.Info<AppLog>($"Starting VPN state refresh timer with initial trigger '{PollingInterval}'");
                StartTimerAndTriggerOnStart();
            }
        }
    }

    public void Initialize()
    {
        lock (_timerLock)
        {
            StartTimer();
        }
    }
}