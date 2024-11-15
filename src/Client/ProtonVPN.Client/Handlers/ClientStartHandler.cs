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

using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Models.SystemTimes;

namespace ProtonVPN.Client.Handlers;

public class ClientStartHandler : IHandler,
    IEventMessageReceiver<ApplicationStartingMessage>,
    IEventMessageReceiver<ApplicationStartedMessage>
{
    private readonly IVpnStatePollingObserver _vpnStatePollingObserver;
    private readonly ISystemTimeValidator _systemTimeValidator;

    public ClientStartHandler(
        IVpnStatePollingObserver vpnStatePollingObserver,
        ISystemTimeValidator systemTimeValidator)
    {
        _vpnStatePollingObserver = vpnStatePollingObserver;
        _systemTimeValidator = systemTimeValidator;
    }

    public void Receive(ApplicationStartingMessage message)
    {
        _vpnStatePollingObserver.Initialize();
    }

    public async void Receive(ApplicationStartedMessage message)
    {
        await _systemTimeValidator.CheckAsync(new CancellationTokenSource().Token);
    }
}