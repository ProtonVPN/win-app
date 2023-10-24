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
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

public class ServiceCallerMock : IServiceCaller
{
    private readonly IEventMessageSender _eventMessageSender;

    private bool _isCancelRequested;

    public ServiceCallerMock(IEventMessageSender eventMessageSender)
    {
        _eventMessageSender = eventMessageSender;
    }

    public async Task ConnectAsync(ConnectionRequestIpcEntity connectionRequest)
    {
        _isCancelRequested = false;

        await Task.Delay(2000);

        if(_isCancelRequested)
        {
            return;
        }

        _eventMessageSender.Send(new VpnStateIpcEntity()
        {
            EndpointIp = "103.107.197.6",
            Status = VpnStatusIpcEntity.Connected,
            VpnProtocol = VpnProtocolIpcEntity.WireGuardUdp,
            Error = VpnErrorTypeIpcEntity.None,
            Label = string.Empty,
            NetworkBlocked = false,
            OpenVpnAdapterType = OpenVpnAdapterIpcEntity.Tap
        });
    }

    public Task DisconnectAsync(DisconnectionRequestIpcEntity connectionRequest)
    {
        _isCancelRequested = true;

        _eventMessageSender.Send(new VpnStateIpcEntity()
        {
            Status = VpnStatusIpcEntity.Disconnected,
        });

        return Task.CompletedTask;
    }

    public Task RegisterClientAsync(int appServerPort, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}