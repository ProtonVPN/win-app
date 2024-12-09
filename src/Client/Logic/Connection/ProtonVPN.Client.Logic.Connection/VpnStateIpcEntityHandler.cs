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

using ProtonVPN.Client.Common.Messages;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection;

public class VpnStateIpcEntityHandler : IEventMessageReceiver<VpnStateIpcEntity>,
    IEventMessageReceiver<ApplicationStoppedMessage>
{
    private readonly ILogger _logger;
    private readonly IConnectionErrorHandler _connectionErrorHandler;
    private readonly IInternalConnectionManager _connectionManager;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private bool _isNetworkBlocked;

    public VpnStateIpcEntityHandler(ILogger logger,
        IConnectionErrorHandler connectionErrorHandler,
        IInternalConnectionManager connectionManager)
    {
        _logger = logger;
        _connectionErrorHandler = connectionErrorHandler;
        _connectionManager = connectionManager;
    }

    public void Receive(ApplicationStoppedMessage message)
    {
        _cancellationTokenSource.Cancel();
    }

    public async void Receive(VpnStateIpcEntity message)
    {
        await _semaphore.WaitAsync();
        try
        {
            await HandleAsync(message);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task HandleAsync(VpnStateIpcEntity message)
    {
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        ConnectionErrorHandlerResult connectionErrorHandlerResponse = await _connectionErrorHandler.HandleAsync(message);

        if (((message.Error != VpnErrorTypeIpcEntity.None && connectionErrorHandlerResponse == ConnectionErrorHandlerResult.SameAsLast) ||
            (message.Error == VpnErrorTypeIpcEntity.NoneKeepEnabledKillSwitch && message.Status == VpnStatusIpcEntity.Disconnected)) && message.NetworkBlocked == _isNetworkBlocked)
        {
            _logger.Info<ConnectionStateChangeLog>($"Ignoring VPN state with Status '{message.Status}' " +
                $"and Error '{message.Error}' handled with '{connectionErrorHandlerResponse}'.");
            return;
        }

        _isNetworkBlocked = message.NetworkBlocked;

        LogHandlerResponse(message, connectionErrorHandlerResponse);

        if (connectionErrorHandlerResponse == ConnectionErrorHandlerResult.Reconnecting)
        {
            _logger.Info<ConnectionStateChangeLog>($"Changing VPN state from Status " +
                $"'{message.Status}' to '{VpnStatusIpcEntity.Reconnecting}'.");
            message.Status = VpnStatusIpcEntity.Reconnecting;
        }

        await _connectionManager.HandleAsync(message);
    }

    private void LogHandlerResponse(VpnStateIpcEntity message,
        ConnectionErrorHandlerResult connectionErrorHandlerResponse)
    {
        string logMessage = $"The VPN state with Status '{message.Status}' and " +
            $"Error '{message.Error}' was handled with '{connectionErrorHandlerResponse}'.";

        if (connectionErrorHandlerResponse == ConnectionErrorHandlerResult.SameAsLast)
        {
            _logger.Debug<ConnectionStateChangeLog>(logMessage);
        }
        else
        {
            _logger.Info<ConnectionStateChangeLog>(logMessage);
        }
    }
}