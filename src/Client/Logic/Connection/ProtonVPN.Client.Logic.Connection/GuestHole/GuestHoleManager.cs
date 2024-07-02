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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.GuestHoleLogs;

namespace ProtonVPN.Client.Logic.Connection.GuestHole;

public class GuestHoleManager : IGuestHoleManager, IEventMessageReceiver<ConnectionStatusChanged>
{
    private readonly ILogger _logger;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IGuestHoleConnector _guestHoleConnector;

    private bool _isActive;
    private bool _wasConnected;
    private Func<Task<Result>>? _onConnectedFunc;
    private TaskCompletionSource<Result?>? _tcs;
    private ConnectionStatus _lastVpnStatus = ConnectionStatus.Disconnected;

    public bool IsActive => _isActive;

    public GuestHoleManager(ILogger logger, IEventMessageSender eventMessageSender,
        IGuestHoleConnector guestHoleConnector)
    {
        _logger = logger;
        _eventMessageSender = eventMessageSender;
        _guestHoleConnector = guestHoleConnector;
    }

    public async Task<T?> ExecuteAsync<T>(Func<Task<Result>> onConnectedFunc) where T : Result
    {
        _onConnectedFunc = onConnectedFunc;
        _tcs = new TaskCompletionSource<Result?>();

        SetStatus(true);
        await _guestHoleConnector.ConnectAsync();

        Result? result = await _tcs.Task;
        if (result is null)
        {
            await DisconnectAsync();
        }

        return (T?)result;
    }

    private void SetStatus(bool isActive)
    {
        _isActive = isActive;
        _eventMessageSender.Send(new GuestHoleStatusChangedMessage(isActive));
    }

    public async void Receive(ConnectionStatusChanged message)
    {
        if (!_isActive || _lastVpnStatus == message.ConnectionStatus)
        {
            return;
        }

        _lastVpnStatus = message.ConnectionStatus;

        switch (message.ConnectionStatus)
        {
            case ConnectionStatus.Connected when _tcs is not null &&
                                                 _onConnectedFunc is not null:
                _logger.Info<GuestHoleLog>("Connected to guest hole");

                _wasConnected = true;
                Result? result;
                try
                {
                    result = await _onConnectedFunc();
                }
                catch (Exception e)
                {
                    _logger.Error<GuestHoleLog>("Failed to execute action inside guest hole", e);
                    result = null;
                }

                SetTaskCompletionSourceResult(result);
                break;
            case ConnectionStatus.Disconnected:
                if (!_wasConnected)
                {
                    SetTaskCompletionSourceResult(null);
                }

                SetStatus(false);
                _logger.Info<GuestHoleLog>("Disconnected from guest hole.");
                break;
        }
    }

    private void SetTaskCompletionSourceResult(Result? result)
    {
        if (_tcs is null)
        {
            return;
        }

        _tcs.TrySetResult(result);
        _tcs = null;
        _onConnectedFunc = null;
        _wasConnected = false;
    }

    public async Task DisconnectAsync()
    {
        await _guestHoleConnector.DisconnectAsync();
    }
}