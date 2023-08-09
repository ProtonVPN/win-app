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
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectLogs;
using ProtonVPN.Logging.Contracts.Events.GuestHoleLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.GuestHole;

public class GuestHoleConnector : IGuestHoleConnector
{
    private readonly Random _random = new();

    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IConfiguration _config;
    private readonly IServiceCaller _serviceCaller;
    private readonly IEntityMapper _entityMapper;
    private readonly IGuestHoleServersFileStorage _guestHoleServersFileStorage;

    public GuestHoleConnector(
        ILogger logger,
        ISettings settings,
        IConfiguration config,
        IServiceCaller serviceCaller,
        IEntityMapper entityMapper,
        IGuestHoleServersFileStorage guestHoleServersFileStorage)
    {
        _logger = logger;
        _settings = settings;
        _config = config;
        _serviceCaller = serviceCaller;
        _entityMapper = entityMapper;
        _guestHoleServersFileStorage = guestHoleServersFileStorage;
    }

    public async Task ConnectAsync()
    {
        ConnectionRequestIpcEntity request = new()
        {
            Config = VpnConfig(),
            Credentials = CreateVpnCredentials(),
            Protocol = VpnProtocolIpcEntity.Smart,
            Servers = _entityMapper.Map<VpnHost, VpnServerIpcEntity>(Servers()).ToArray(),
            Settings = GetSettings(),
        };

        _logger.Info<ConnectTriggerLog>("Guest hole connection requested.");
        await _serviceCaller.ConnectAsync(request);
    }

    public async Task DisconnectAsync()
    {
        //TODO: use VpnError.NoneKeepEnabledKillSwitch for disconnect
        await _serviceCaller.DisconnectAsync();
    }

    public bool AreServersAvailable => Servers().Any();

    private IReadOnlyList<VpnHost> Servers()
    {
        IEnumerable<GuestHoleServerContract> servers = _guestHoleServersFileStorage.Get();
        return servers != null
            ? servers.Select(server => new VpnHost(server.Host, server.Ip, server.Label, null, server.Signature))
                .OrderBy(_ => _random.Next())
                .ToList()
            : new List<VpnHost>();
    }

    private MainSettingsIpcEntity GetSettings()
    {
        return new MainSettingsIpcEntity
        {
            KillSwitchMode = KillSwitchModeIpcEntity.Off,
            OpenVpnAdapter = OpenVpnAdapterIpcEntity.Tap,
            VpnProtocol = VpnProtocolIpcEntity.OpenVpnTcp,
            AllowNonStandardPorts = false,
            Ipv6LeakProtection = true,
            ModerateNat = false,
            NetShieldMode = 0,
            PortForwarding = false,
            SplitTcp = false,
            SplitTunnel = new SplitTunnelSettingsIpcEntity
            {
                AppPaths = Array.Empty<string>(),
                Ips = Array.Empty<string>(),
                Mode = SplitTunnelModeIpcEntity.Disabled
            }
        };
    }

    private VpnCredentialsIpcEntity CreateVpnCredentials()
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

    private VpnConfigIpcEntity VpnConfig()
    {
        return new VpnConfigIpcEntity
        {
            Ports = new Dictionary<VpnProtocolIpcEntity, int[]>
            {
                { VpnProtocolIpcEntity.OpenVpnTcp, _settings.OpenVpnTcpPorts },
                { VpnProtocolIpcEntity.OpenVpnUdp, _settings.OpenVpnUdpPorts },
            },
            SplitTunnelMode = SplitTunnelModeIpcEntity.Disabled,
            NetShieldMode = 0,
            VpnProtocol = VpnProtocolIpcEntity.OpenVpnTcp,
            PreferredProtocols = new List<VpnProtocolIpcEntity>
            {
                VpnProtocolIpcEntity.OpenVpnTcp,
                VpnProtocolIpcEntity.OpenVpnUdp
            },
            ModerateNat = false,
            SplitTcp = true,
            AllowNonStandardPorts = true,
            PortForwarding = false,
        };
    }
}