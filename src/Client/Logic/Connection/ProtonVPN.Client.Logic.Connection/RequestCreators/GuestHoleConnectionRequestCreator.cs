/*
 * Copyright (c) 2026 Proton AG
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
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.RequestCreators;

public class GuestHoleConnectionRequestCreator : ConnectionRequestCreatorBase, IGuestHoleConnectionRequestCreator
{
    private readonly IConfiguration _config;
    private readonly IConnectionKeyManager _connectionKeyManager;

    public GuestHoleConnectionRequestCreator(
        IConfiguration config,
        ILogger logger,
        ISettings settings,
        IEntityMapper entityMapper,
        IConnectionKeyManager connectionKeyManager,
        IFeatureFlagsObserver featureFlagsObserver,
        IMainSettingsRequestCreator mainSettingsRequestCreator)
        : base(logger, settings, entityMapper, featureFlagsObserver, mainSettingsRequestCreator)
    {
        _config = config;
        _connectionKeyManager = connectionKeyManager;
    }

    public async Task<ConnectionRequestIpcEntity> CreateAsync(IEnumerable<GuestHoleServerContract> servers)
    {
        MainSettingsIpcEntity settings = GetSettings();

        ConnectionRequestIpcEntity request = new()
        {
            RetryId = Guid.NewGuid(),
            Config = GetVpnConfig(settings),
            Credentials = await GetVpnCredentialsAsync(),
            Protocol = settings.VpnProtocol,
            Servers = GetVpnServers(servers),
            Settings = settings,
        };

        return request;
    }

    protected override MainSettingsIpcEntity GetSettings(IConnectionIntent? connectionIntent = null)
    {
        return MainSettingsRequestCreator.CreateForGuestHole();
    }

    protected override VpnConfigIpcEntity GetVpnConfig(MainSettingsIpcEntity settings, IConnectionIntent? connectionIntent = null)
    {
        return new()
        {
            VpnProtocol = settings.VpnProtocol,
            SplitTunnelMode = settings.SplitTunnel.Mode,
            SplitTunnelIPs = settings.SplitTunnel.Ips.ToList(),
            ModerateNat = settings.ModerateNat,
            NetShieldMode = settings.NetShieldMode,
            PortForwarding = settings.PortForwarding,
            SplitTcp = settings.SplitTcp,
            PreferredProtocols = 
            [
                VpnProtocolIpcEntity.WireGuardTls,
                VpnProtocolIpcEntity.OpenVpnTcp,
            ],
            Ports = 
            {
                { VpnProtocolIpcEntity.WireGuardTls, Settings.WireGuardTlsPorts },
                { VpnProtocolIpcEntity.OpenVpnTcp, Settings.OpenVpnTcpPorts },
            },
            CustomDns = [],
            IsIpv6Enabled = settings.IsIpv6Enabled,
            WireGuardConnectionTimeout = settings.WireGuardConnectionTimeout,
            DnsBlockMode = settings.DnsBlockMode,
            ShouldDisableWeakHostSetting = DefaultSettings.ShouldDisableWeakHostSetting,
            IsWireGuardServerRouteEnabled = DefaultSettings.IsWireGuardServerRouteEnabled,
        };
    }

    protected override Task<VpnCredentialsIpcEntity> GetVpnCredentialsAsync()
    {
        VpnCredentialsIpcEntity credentials = EntityMapper.Map<VpnCredentials, VpnCredentialsIpcEntity>(
            new VpnCredentials(
                _connectionKeyManager.GenerateTemporaryKeyPair(),
                AddSuffixToUsername(_config.GuestHoleVpnUsername),
                _config.GuestHoleVpnPassword));

        return Task.FromResult(credentials);
    }

    private string AddSuffixToUsername(string username)
    {
        return username + _config.VpnUsernameSuffix;
    }

    private VpnServerIpcEntity[] GetVpnServers(IEnumerable<GuestHoleServerContract> servers)
    {
        return servers
            .Select(EntityMapper.Map<GuestHoleServerContract, VpnServerIpcEntity>)
            .Where(s => s is not null)
            .ToArray();
    }
}