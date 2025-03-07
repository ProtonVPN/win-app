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

using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.RequestCreators;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.RequestCreators;

public class GuestHoleConnectionRequestCreator : ConnectionRequestCreatorBase, IGuestHoleConnectionRequestCreator
{
    private readonly IConnectionKeyManager _connectionKeyManager;

    public GuestHoleConnectionRequestCreator(
        ILogger logger,
        ISettings settings,
        IEntityMapper entityMapper,
        IConnectionKeyManager connectionKeyManager,
        IMainSettingsRequestCreator mainSettingsRequestCreator)
        : base(logger, settings, entityMapper, mainSettingsRequestCreator)
    {
        _connectionKeyManager = connectionKeyManager;
    }

    public async Task<ConnectionRequestIpcEntity> CreateAsync(IEnumerable<GuestHoleServerContract> servers)
    {
        MainSettingsIpcEntity settings = GetSettings();
        settings.VpnProtocol = VpnProtocolIpcEntity.WireGuardTls;

        ConnectionRequestIpcEntity request = new()
        {
            RetryId = Guid.NewGuid(),
            Config = GetVpnConfig(settings),
            Credentials = await GetVpnCredentialsAsync(),
            Protocol = VpnProtocolIpcEntity.WireGuardTls,
            Servers = GetVpnServers(servers),
            Settings = settings,
        };

        return request;
    }

    protected override VpnConfigIpcEntity GetVpnConfig(MainSettingsIpcEntity settings, IConnectionIntent? connectionIntent = null)
    {
        return new()
        {
            VpnProtocol = settings.VpnProtocol,
            PreferredProtocols = [ VpnProtocolIpcEntity.WireGuardTls ],
            Ports = { { VpnProtocolIpcEntity.WireGuardTls, Settings.WireGuardTlsPorts } },
        };
    }

    protected override Task<VpnCredentialsIpcEntity> GetVpnCredentialsAsync()
    {
        VpnCredentialsIpcEntity credentials = EntityMapper.Map<VpnCredentials, VpnCredentialsIpcEntity>(
            new VpnCredentials(_connectionKeyManager.GenerateTemporaryKeyPair()));

        return Task.FromResult(credentials);
    }

    private VpnServerIpcEntity[] GetVpnServers(IEnumerable<GuestHoleServerContract> servers)
    {
        return servers
            .Select(s => EntityMapper.Map<GuestHoleServerContract, VpnServerIpcEntity>(s))
            .Where(s => s is not null)
            .ToArray();
    }
}