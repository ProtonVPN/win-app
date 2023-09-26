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
using ProtonVPN.Client.Logic.Connection.Contracts.Wrappers;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.GuestHole;

public class GuestHoleConnector : IGuestHoleConnector
{
    private readonly Random _random = new();

    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly IServiceCaller _serviceCaller;
    private readonly IGuestHoleServersFileStorage _guestHoleServersFileStorage;
    private readonly IGuestHoleConnectionRequestWrapper _connectionRequestWrapper;
    private readonly IDisconnectionRequestWrapper _disconnectionRequestWrapper;

    public bool AreServersAvailable => GetGuestHoleServers().Any();

    public GuestHoleConnector(
        ILogger logger,
        IConfiguration config,
        IServiceCaller serviceCaller,
        IGuestHoleServersFileStorage guestHoleServersFileStorage,
        IGuestHoleConnectionRequestWrapper connectionRequestWrapper,
        IDisconnectionRequestWrapper disconnectionRequestWrapper)
    {
        _logger = logger;
        _config = config;
        _serviceCaller = serviceCaller;
        _guestHoleServersFileStorage = guestHoleServersFileStorage;
        _connectionRequestWrapper = connectionRequestWrapper;
        _disconnectionRequestWrapper = disconnectionRequestWrapper;
    }

    public async Task ConnectAsync()
    {
        ConnectionRequestIpcEntity request = _connectionRequestWrapper.Wrap(GetGuestHoleServers());

        _logger.Info<ConnectTriggerLog>("Guest hole connection requested.");
        await _serviceCaller.ConnectAsync(request);
    }

    public async Task DisconnectAsync()
    {
        // TODO: use VpnError.NoneKeepEnabledKillSwitch for disconnect
        DisconnectionRequestIpcEntity request = _disconnectionRequestWrapper.Wrap();

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