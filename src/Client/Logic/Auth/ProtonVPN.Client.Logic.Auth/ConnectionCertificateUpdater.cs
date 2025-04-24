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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.UserCertificateLogs;

namespace ProtonVPN.Client.Logic.Auth;

public class ConnectionCertificateUpdater : IConnectionCertificateUpdater,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggingOutMessage>
{
    private readonly IConfiguration _config;
    private readonly IConnectionCertificateManager _connectionCertificateManager;
    private readonly ILogger _logger;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private Timer? _timer;

    public ConnectionCertificateUpdater(IConfiguration config,
        IConnectionCertificateManager connectionCertificateManager,
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _config = config;
        _connectionCertificateManager = connectionCertificateManager;
        _logger = logger;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    private void Timer_OnTick(object? sender)
    {
        // TODO: Does this need to be done on the UI thread?
        _uiThreadDispatcher.TryEnqueue(() => _connectionCertificateManager.RequestNewCertificateAsync());
    }

    public void Receive(LoggedInMessage message)
    {
        TimeSpan interval = _config.ConnectionCertificateUpdateInterval;
        _timer = new(Timer_OnTick);
        _timer.Change(interval, interval);
        _logger.Info<UserCertificateScheduleRefreshLog>(
            $"Connection certificate refresh scheduled for every '{interval}'.");
    }

    public void Receive(LoggingOutMessage message)
    {
        _timer?.Dispose();
    }
}