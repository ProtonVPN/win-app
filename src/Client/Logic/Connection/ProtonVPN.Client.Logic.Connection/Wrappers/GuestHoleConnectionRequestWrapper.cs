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
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.Wrappers;

public class GuestHoleConnectionRequestWrapper : ConnectionRequestWrapperBase, IGuestHoleConnectionRequestWrapper
{
    private readonly IConfiguration _config;

    public GuestHoleConnectionRequestWrapper(
        ISettings settings,
        IEntityMapper entityMapper,
        IConfiguration config)
        : base(settings, entityMapper)
    {
        _config = config;
    }

    public ConnectionRequestIpcEntity Wrap(IEnumerable<GuestHoleServerContract> servers)
    {
        MainSettingsIpcEntity settings = GetSettings();

        ConnectionRequestIpcEntity request = new()
        {
            Config = GetVpnConfig(settings),
            Credentials = GetVpnCredentials(),
            Protocol = settings.VpnProtocol,
            Servers = GetVpnServers(servers),
            Settings = settings,
        };

        return request;
    }

    protected override VpnCredentialsIpcEntity GetVpnCredentials()
    {
        return new()
        {
            Username = AddSuffixToUsername(_config.GuestHoleVpnUsername),
            Password = _config.GuestHoleVpnPassword,
        };
    }

    private string AddSuffixToUsername(string username)
    {
        return username + _config.VpnUsernameSuffix;
    }

    private VpnServerIpcEntity[] GetVpnServers(IEnumerable<GuestHoleServerContract> servers)
    {
        return servers
            .Select(s => EntityMapper.Map<GuestHoleServerContract, VpnServerIpcEntity>(s))
            .Where(s => s is not null)
            .ToArray();
    }
}