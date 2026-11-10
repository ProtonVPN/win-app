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
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Observers;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Servers.Observers;

public class ServersObserver : PollingObserverBase, IServersObserver,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<DeviceLocationChangedMessage>,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>
{
    private static readonly TimeSpan _firstRetryDelay = TimeSpan.FromSeconds(20);

    private readonly IServersUpdater _serversUpdater;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IConfiguration _config;

    private TimeSpan? _retryDelay;

    protected override TimeSpan PollingInterval => TimeSpanExtensions.Min(_config.ServerLoadUpdateInterval, _config.ServerUpdateInterval);

    public ServersObserver(ILogger logger,
        IIssueReporter issueReporter,
        IServersUpdater serversUpdater,
        IUserAuthenticator userAuthenticator,
        IConfiguration config)
        : base(logger, issueReporter)
    {
        _serversUpdater = serversUpdater;
        _userAuthenticator = userAuthenticator;
        _config = config;
    }

    public void Receive(LoggedInMessage message)
    {
        _retryDelay = null;
        StartTimer();

        // The first update is made by the authenticator before logging in.
        if (_serversUpdater.IsRefreshNeeded())
        {
            ScheduleRetry();
        }
    }

    public void Receive(LoggedOutMessage message)
    {
        StopTimer();
        _retryDelay = null;

        _serversUpdater.ClearCacheAsync().FireAndForget();
    }

    public void Receive(DeviceLocationChangedMessage message)
    {
        TriggerAction.Run();
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        if (message.IsMainWindowVisible)
        {
            TriggerAction.Run();
        }
    }

    protected override async Task OnTriggerAsync()
    {
        if (!_userAuthenticator.IsLoggedIn)
        {
            return;
        }

        bool isUpdated = false;

        try
        {
            await _serversUpdater.UpdateAsync();
            isUpdated = !_serversUpdater.IsRefreshNeeded();
        }
        finally
        {
            // The user may have logged out while the update was running.
            if (_userAuthenticator.IsLoggedIn)
            {
                if (!isUpdated)
                {
                    ScheduleRetry();
                }
                else if (_retryDelay is not null)
                {
                    _retryDelay = null;
                    StopTimer();
                    StartTimer();
                }
            }
        }
    }

    private void ScheduleRetry()
    {
        TimeSpan delay = _retryDelay * 2 ?? _firstRetryDelay;
        if (delay >= PollingInterval)
        {
            // The polling takes over, the backoff restarts only after a successful update.
            Logger.Info<AppLog>("Servers still need a refresh, retrying at the next polling.");
            return;
        }

        _retryDelay = delay;

        Logger.Info<AppLog>($"Servers still need a refresh, retrying in {delay.TotalSeconds:0} seconds.");

        StopTimer();
        StartTimerWithDelay(delay);
    }
}