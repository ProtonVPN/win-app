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

using ProtonVPN.Api.Contracts;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.GuestHoleLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.GuestHole;

public class GuestHoleActionExecutor : IGuestHoleActionExecutor, IEventMessageReceiver<VpnStateIpcEntity>
{
    private const int TIMEOUT_IN_SECONDS = 30;

    private static SemaphoreSlim _semaphore = new(1, 1);
    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly IGuestHoleState _guestHoleState;
    private readonly IGuestHoleConnector _guestHoleConnector;

    private Func<Task> _onConnectedFunc;
    private Result _result = Result.Fail();
    private VpnStatusIpcEntity _lastVpnStatus = VpnStatusIpcEntity.Disconnected;
    private int _reconnectCount;
    private bool _isActive;

    public GuestHoleActionExecutor(ILogger logger, IConfiguration config, IGuestHoleState guestHoleState,
        IGuestHoleConnector guestHoleConnector)
    {
        _logger = logger;
        _config = config;
        _guestHoleState = guestHoleState;
        _guestHoleConnector = guestHoleConnector;
    }

    public async Task<Result> ExecuteAsync(Func<Task> onConnectedFunc)
    {
        _onConnectedFunc = onConnectedFunc;

        _guestHoleState.SetState(true);
        await _guestHoleConnector.ConnectAsync();
        _semaphore = new(0, 1);

        Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(TIMEOUT_IN_SECONDS));
        Task task = await Task.WhenAny(_semaphore.WaitAsync(), timeoutTask);
        if (task == timeoutTask)
        {
            _logger.Error<GuestHoleLog>("Failed to execute action inside guest hole due to timeout.");
            _result = Result.Fail();
        }

        return _result;
    }

    public bool IsActive()
    {
        return _isActive;
    }

    public async void Receive(VpnStateIpcEntity message)
    {
        if (_lastVpnStatus == message.Status || _semaphore.CurrentCount == 1)
        {
            return;
        }

        switch (message.Status)
        {
            case VpnStatusIpcEntity.Connected:
                _reconnectCount = 0;
                _isActive = true;
                try
                {
                    await _onConnectedFunc();
                    _result = Result.Ok();
                }
                catch (Exception e)
                {
                    _logger.Error<GuestHoleLog>("Failed to execute action inside guest hole", e);
                    _result = Result.Fail();
                }

                ReleaseLock();

                break;
            case VpnStatusIpcEntity.Reconnecting:
                _reconnectCount++;
                if (_reconnectCount >= _config.MaxGuestHoleRetries)
                {
                    _reconnectCount = 0;
                    await _guestHoleConnector.DisconnectAsync();
                }

                break;
            case VpnStatusIpcEntity.Disconnected:
                if (message.Error != VpnErrorTypeIpcEntity.None)
                {
                    _logger.Error<GuestHoleLog>($"Failed to connect to a guest hole: {message.Error}");
                    _result = Result.Fail();
                }

                _isActive = false;
                _guestHoleState.SetState(false);
                ReleaseLock();
                break;
        }

        _lastVpnStatus = message.Status;
    }

    public async Task DisconnectAsync()
    {
        await _guestHoleConnector.DisconnectAsync();
    }

    private void ReleaseLock()
    {
        _semaphore.Release();
    }
}