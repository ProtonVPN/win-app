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
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Observers;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Logic.Servers.Observers;

public class ServersObserver :
    PollingObserverBase,
    IServersObserver,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<DeviceLocationChangedMessage>
{
    private readonly IServersUpdater _serversUpdater;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IConfiguration _config;

    private DateTime _lastServerUpdate = DateTime.MinValue;

    protected override TimeSpan PollingInterval => _config.ServerLoadUpdateInterval;

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
        // Reset flag to force servers update on login
        _lastServerUpdate = DateTime.MinValue;

        StartTimerAndTriggerOnStart();
    }

    public void Receive(LoggedOutMessage message)
    {
        StopTimer();
    }

    public void Receive(DeviceLocationChangedMessage message)
    {
        if (_userAuthenticator.IsLoggedIn)
        {
            RestartTimerAndTriggerOnStart();
        }
    }

    protected override async Task OnTriggerAsync()
    {
        DateTime now = DateTime.UtcNow;
        if (now - _lastServerUpdate >= _config.ServerUpdateInterval)
        {
            _lastServerUpdate = now;

            await _serversUpdater.UpdateAsync();
        }
        else
        {
            await _serversUpdater.UpdateLoadsAsync();
        }
    }
}