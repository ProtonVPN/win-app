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

using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.GuestHole;

public class GuestHoleConnector : IGuestHoleConnector
{
    private readonly Random _random = new();

    private readonly ILogger _logger;
    private readonly IServiceCaller _serviceCaller;
    private readonly IGuestHoleServersFileStorage _guestHoleServersFileStorage;
    private readonly IGuestHoleConnectionRequestCreator _connectionRequestCreator;
    private readonly IDisconnectionRequestCreator _disconnectionRequestCreator;

    public bool AreServersAvailable => GetGuestHoleServers().Any();

    public GuestHoleConnector(
        ILogger logger,
        IServiceCaller serviceCaller,
        IGuestHoleServersFileStorage guestHoleServersFileStorage,
        IGuestHoleConnectionRequestCreator connectionRequestCreator,
        IDisconnectionRequestCreator disconnectionRequestCreator)
    {
        _logger = logger;
        _serviceCaller = serviceCaller;
        _guestHoleServersFileStorage = guestHoleServersFileStorage;
        _connectionRequestCreator = connectionRequestCreator;
        _disconnectionRequestCreator = disconnectionRequestCreator;
    }

    public async Task ConnectAsync()
    {
        ConnectionRequestIpcEntity request = _connectionRequestCreator.Create(GetGuestHoleServers());

        _logger.Info<ConnectTriggerLog>("Guest hole connection requested.");
        await _serviceCaller.ConnectAsync(request);
    }

    public async Task DisconnectAsync()
    {
        // TODO: use VpnError.NoneKeepEnabledKillSwitch for disconnect
        DisconnectionRequestIpcEntity request = _disconnectionRequestCreator.Create();

        await _serviceCaller.DisconnectAsync(request);
    }

    private IReadOnlyList<GuestHoleServerContract> GetGuestHoleServers()
    {
        return _guestHoleServersFileStorage
            .Get()
            .OrderBy(_ => _random.Next())
            .ToList();
    }
}